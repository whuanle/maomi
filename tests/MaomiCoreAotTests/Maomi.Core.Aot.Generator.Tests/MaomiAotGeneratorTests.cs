// <copyright file="MaomiAotGeneratorTests.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Maomi.Core.Aot.Generator.Tests;

/// <summary>
/// 源生成器单元测试.
/// </summary>
public class MaomiAotGeneratorTests
{
    private const string RegistryFileName = "MaomiAotRegistry.g.cs";

    [Fact]
    public void Generate_Registry_For_Source_Module_And_Services()
    {
        var source = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            public interface IMyService { }

            [InjectOnScoped]
            public class MyService : IMyService
            {
                public MyService() { }
            }

            public sealed class AppModule : IModule
            {
                public void ConfigureServices(ServiceContext context) { }
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(source);
        GeneratorTestRunner.AssertNoCompilationErrors(diagnostics);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning));
        var registry = Assert.Single(sources);
        Assert.EndsWith(RegistryFileName, registry.HintName, StringComparison.Ordinal);

        Assert.Contains("MaomiAotRegistry.RegisterModule(typeof(global::AppModule)", registry.SourceText);
        Assert.Contains("services.AddScoped(typeof(global::IMyService), static sp => new global::MyService()", registry.SourceText);
        Assert.Contains("MaomiAotInitialize", registry.SourceText);
    }

    [Fact]
    public void Report_Loop_Dependency_Error_And_Skip_Generation()
    {
        var source = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            [InjectModule<BModule>]
            public class AModule : IModule
            {
                public void ConfigureServices(ServiceContext context) { }
            }

            [InjectModule<AModule>]
            public class BModule : IModule
            {
                public void ConfigureServices(ServiceContext context) { }
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(source);
        GeneratorTestRunner.AssertNoCompilationErrors(diagnostics);

        var loopError = Assert.Single(diagnostics, d => d.Id == "MAOMI001");
        Assert.Equal(DiagnosticSeverity.Error, loopError.Severity);
        Assert.Contains("AModule", loopError.GetMessage());
        Assert.Contains("BModule", loopError.GetMessage());

        // 存在循环依赖时不生成注册表
        Assert.Empty(sources);
    }

    [Fact]
    public void Report_Error_When_Service_In_Assembly_Without_Module()
    {
        var source = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            public interface IMyService { }

            [InjectOnScoped]
            public class MyService : IMyService
            {
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(source);
        GeneratorTestRunner.AssertNoCompilationErrors(diagnostics);

        var withoutModule = Assert.Single(diagnostics, d => d.Id == "MAOMI002");
        Assert.Equal(DiagnosticSeverity.Info, withoutModule.Severity);
        Assert.Contains("MyService", withoutModule.GetMessage());

        Assert.Empty(sources);
    }

    [Fact]
    public void Report_Fallback_For_Multiple_Public_Constructors()
    {
        var source = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            public interface IMyService { }

            [InjectOnScoped]
            public class MyService : IMyService
            {
                public MyService() { }
                public MyService(int value) { }
            }

            public sealed class AppModule : IModule
            {
                public void ConfigureServices(ServiceContext context) { }
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(source);
        GeneratorTestRunner.AssertNoCompilationErrors(diagnostics);

        var fallback = Assert.Single(diagnostics, d => d.Id == "MAOMI003");
        Assert.Equal(DiagnosticSeverity.Info, fallback.Severity);
        Assert.Contains("MyService", fallback.GetMessage());

        // 回退为 typeof 注册
        var registry = Assert.Single(sources);
        Assert.Contains("services.AddScoped(typeof(global::IMyService), typeof(global::MyService));", registry.SourceText);
    }

    [Fact]
    public void Report_Warning_For_Empty_ServiceTypes_In_Some_Scheme()
    {
        var source = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            [InjectOnTransient(InjectScheme.Some)]
            public class SomeService
            {
            }

            public sealed class AppModule : IModule
            {
                public void ConfigureServices(ServiceContext context) { }
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(source);
        GeneratorTestRunner.AssertNoCompilationErrors(diagnostics);

        var emptySome = Assert.Single(diagnostics, d => d.Id == "MAOMI004");
        Assert.Equal(DiagnosticSeverity.Warning, emptySome.Severity);
        Assert.Contains("SomeService", emptySome.GetMessage());

        // Some 模式且 ServiceTypes 为空、Own = false：不注册任何服务
        var registry = Assert.Single(sources);
        Assert.Contains("MaomiAotRegistry.RegisterModule(typeof(global::AppModule)", registry.SourceText);
        Assert.DoesNotContain("services.AddTransient(typeof(global::SomeService)", registry.SourceText);
    }

    [Fact]
    public void Fixed_Self_Registration_Semantics()
    {
        var source = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            public interface IMyService { }

            // 有接口且 Own = false：只注册接口，不注册自身
            [InjectOnScoped]
            public class InterfaceService : IMyService
            {
            }

            // 没有任何候选：注册自身
            [InjectOnSingleton]
            public class SelfOnlyService
            {
            }

            // Own = true：接口与自身都注册
            [InjectOnSingleton(Own = true)]
            public class OwnService : IMyService
            {
            }

            public sealed class AppModule : IModule
            {
                public void ConfigureServices(ServiceContext context) { }
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(source);
        GeneratorTestRunner.AssertNoCompilationErrors(diagnostics);

        var registry = Assert.Single(sources);
        Assert.Contains("services.AddScoped(typeof(global::IMyService), static sp => new global::InterfaceService()", registry.SourceText);
        Assert.DoesNotContain("typeof(global::InterfaceService), static sp => new global::InterfaceService()", registry.SourceText);

        Assert.Contains("services.AddSingleton(typeof(global::SelfOnlyService), static sp => new global::SelfOnlyService()", registry.SourceText);

        Assert.Contains("services.AddSingleton(typeof(global::IMyService), static sp => new global::OwnService()", registry.SourceText);
        Assert.Contains("services.AddSingleton(typeof(global::OwnService), static sp => new global::OwnService()", registry.SourceText);
    }

    [Fact]
    public void Generate_Keyed_Service_With_FromKeyedServices_Parameter()
    {
        var source = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            public interface IKeyedService { }

            public interface IConsumer { }

            [InjectOnSingleton(ServiceKey = "alpha")]
            public class AlphaService : IKeyedService
            {
            }

            [InjectOnScoped]
            public class Consumer : IConsumer
            {
                public Consumer([FromKeyedServices("alpha")] IKeyedService keyed)
                {
                }
            }

            public sealed class AppModule : IModule
            {
                public void ConfigureServices(ServiceContext context) { }
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(source);
        GeneratorTestRunner.AssertNoCompilationErrors(diagnostics);

        var registry = Assert.Single(sources);
        Assert.Contains("services.AddKeyedSingleton(typeof(global::IKeyedService), \"alpha\", static (sp, serviceKey) => new global::AlphaService()", registry.SourceText);
        Assert.Contains("sp.GetRequiredKeyedService<global::IKeyedService>(\"alpha\")", registry.SourceText);
    }

    [Fact]
    public void Close_Over_Cross_Assembly_Module_Dependency()
    {
        var librarySource = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            namespace Library;

            public interface ILibraryService { }

            [InjectOnSingleton]
            public class LibraryService : ILibraryService
            {
            }

            public sealed class LibraryModule : IModule
            {
                public void ConfigureServices(ServiceContext context) { }
            }
            """;

        var libraryReference = GeneratorTestRunner.CompileLibrary(librarySource);

        var appSource = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            public interface IAppService { }

            [InjectOnScoped]
            public class AppService : IAppService
            {
            }

            [InjectModule<Library.LibraryModule>]
            public sealed class AppModule : IModule
            {
                public void ConfigureServices(ServiceContext context) { }
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(appSource, new[] { libraryReference });
        GeneratorTestRunner.AssertNoCompilationErrors(diagnostics);

        var registry = Assert.Single(sources);

        // 依赖模块在引用程序集中：闭包展开后包含它，且引用程序集的服务也被注册
        Assert.Contains("typeof(global::Library.LibraryModule)", registry.SourceText);
        Assert.Contains("typeof(global::Library.ILibraryService), static sp => new global::Library.LibraryService", registry.SourceText);
        Assert.Contains("typeof(global::IAppService), static sp => new global::AppService", registry.SourceText);

        // 两个程序集各有一个注册代码块
        Assert.Contains("Register__", registry.SourceText);
    }

    [Fact]
    public void Report_Error_When_Dependency_Is_Not_A_Module()
    {
        var source = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            public class NotAModule
            {
            }

            [InjectModule<NotAModule>]
            public sealed class AppModule : IModule
            {
                public void ConfigureServices(ServiceContext context) { }
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(source);
        GeneratorTestRunner.AssertNoCompilationErrors(diagnostics);

        Assert.Contains(diagnostics, d => d.Id == "MAOMI005" && d.Severity == DiagnosticSeverity.Error);
        Assert.Empty(sources);
    }

    [Fact]
    public void No_Output_When_Maomi_Not_Referenced()
    {
        var source = """
            public class PlainClass
            {
                public void DoSomething() { }
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(source);
        Assert.Empty(sources);
        Assert.Empty(diagnostics.Where(d => d.Id.StartsWith("MAOMI", StringComparison.Ordinal)));
    }

    [Fact]
    public void Register_Module_With_Constructor_Injection_And_Dependency_Order()
    {
        var source = """
            using Maomi;
            using Microsoft.Extensions.DependencyInjection;

            public sealed class OrderCollector
            {
                public System.Collections.Generic.List<string> Names { get; } = new();
            }

            public sealed class AModule : IModule
            {
                private readonly OrderCollector _collector;
                public AModule(OrderCollector collector)
                {
                    _collector = collector;
                }

                public void ConfigureServices(ServiceContext context)
                {
                    _collector.Names.Add("A");
                }
            }

            [InjectModule<AModule>]
            public sealed class BModule : IModule
            {
                private readonly OrderCollector _collector;
                public BModule(OrderCollector collector)
                {
                    _collector = collector;
                }

                public void ConfigureServices(ServiceContext context)
                {
                    _collector.Names.Add("B");
                }
            }
            """;

        var (diagnostics, sources) = GeneratorTestRunner.RunGenerator(source);
        GeneratorTestRunner.AssertNoCompilationErrors(diagnostics);

        var registry = Assert.Single(sources);
        // 两个模块都注册为模块类型 + 工厂委托
        Assert.Contains("typeof(global::AModule)", registry.SourceText);
        Assert.Contains("typeof(global::BModule)", registry.SourceText);
        Assert.Contains("new global::AModule(sp.GetRequiredService<global::OrderCollector>())", registry.SourceText);
        Assert.Contains("new global::BModule(sp.GetRequiredService<global::OrderCollector>())", registry.SourceText);
    }
}
