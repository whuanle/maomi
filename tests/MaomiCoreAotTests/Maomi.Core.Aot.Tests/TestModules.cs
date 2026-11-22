// <copyright file="TestModules.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Maomi;
using Microsoft.Extensions.DependencyInjection;

namespace Maomi.Core.Aot.Tests;

/// <summary>
/// 集成测试用的服务与模块定义（本程序集编译时由源生成器生成注册代码）.
/// </summary>
public static class TestFixture
{
    /// <summary>
    /// 模块初始化顺序记录器.
    /// </summary>
    public sealed class OrderRecorder
    {
        public List<string> Order { get; } = new();
    }

    #region 生命周期与注册模式

    public interface IClock
    {
        DateTimeOffset Now { get; }
    }

    [InjectOnScoped]
    public sealed class SystemClock : IClock
    {
        public DateTimeOffset Now => DateTimeOffset.UtcNow;
    }

    public interface ICounterService
    {
        int Next();
    }

    [InjectOnTransient]
    public sealed class CounterService : ICounterService
    {
        private int _index;
        public int Next() => ++_index;
    }

    [InjectOnSingleton]
    public sealed class SelfService
    {
        public bool Alive => true;
    }

    public interface IOwned
    {
    }

    [InjectOnSingleton(Own = true)]
    public sealed class OwnService : IOwned
    {
    }

    public interface ISomeA { }

    public interface ISomeB { }

    public interface ISomeC { }

    [InjectOnTransient(InjectScheme.Some, ServiceTypes = new[] { typeof(ISomeA), typeof(ISomeB) })]
    public sealed class SomeService : ISomeA, ISomeB, ISomeC
    {
    }

    public interface INever { }

    [InjectOn(scheme: InjectScheme.None)]
    public sealed class NeverService : INever
    {
    }

    [InjectOn(scheme: InjectScheme.None, Own = true)]
    public sealed class NeverOwnService : INever
    {
    }

    #endregion

    #region 键控服务

    public interface IKeyedRepo
    {
        string Name { get; }
    }

    [InjectOnSingleton(ServiceKey = "mysql")]
    public sealed class MySqlRepo : IKeyedRepo
    {
        public string Name => "mysql";
    }

    [InjectOnSingleton(ServiceKey = "redis")]
    public sealed class RedisRepo : IKeyedRepo
    {
        public string Name => "redis";
    }

    public interface IStorage
    {
        string Backend { get; }
    }

    [InjectOnScoped]
    public sealed class Storage : IStorage
    {
        public Storage([FromKeyedServices("mysql")] IKeyedRepo repo)
        {
            Backend = repo.Name;
        }

        public string Backend { get; }
    }

    #endregion

    #region 构造函数注入

    public interface IDeep
    {
    }

    [InjectOnSingleton]
    public sealed class DeepService : IDeep
    {
        public DeepService(IClock clock, bool flag = true)
        {
            Clock = clock;
            Flag = flag;
        }

        public IClock Clock { get; }

        public bool Flag { get; }
    }

    public interface IViaProvider
    {
        IServiceProvider Provider { get; }
    }

    [InjectOnSingleton]
    public sealed class ViaProviderService : IViaProvider
    {
        public ViaProviderService(IServiceProvider provider)
        {
            Provider = provider;
        }

        public IServiceProvider Provider { get; }
    }

    public interface IFiltered
    {
    }

    [InjectOnScoped]
    public sealed class FilteredService : IFiltered, IDisposable
    {
        public void Dispose()
        {
        }
    }

    public interface ILegacy
    {
    }

    [InjectOnSingleton]
    public sealed class LegacyService : ILegacy
    {
        public LegacyService()
        {
        }

        public LegacyService(string config)
        {
        }
    }

    #endregion

    #region 模块

    public interface IRegisterFromModule
    {
    }

    public sealed class FromModuleService : IRegisterFromModule
    {
    }

    public sealed class ModuleA : IModule
    {
        private readonly OrderRecorder _recorder;

        public ModuleA(OrderRecorder recorder)
        {
            _recorder = recorder;
        }

        public void ConfigureServices(ServiceContext context)
        {
            _recorder.Order.Add("A");
            context.Services.AddSingleton<IRegisterFromModule, FromModuleService>();
            CapturedModuleCount = context.Modules.Count;
        }

        /// <summary>
        /// 捕获上下文中的模块数量，用于验证 ModuleRecord.
        /// </summary>
        public static int CapturedModuleCount { get; private set; }
    }

    [InjectModule<ModuleA>]
    public sealed class ModuleB : IModule
    {
        private readonly OrderRecorder _recorder;

        public ModuleB(OrderRecorder recorder)
        {
            _recorder = recorder;
        }

        public void ConfigureServices(ServiceContext context)
        {
            _recorder.Order.Add("B");
        }
    }

    [InjectModule<ModuleA>]
    public sealed class ModuleC : IModule
    {
        private readonly OrderRecorder _recorder;

        public ModuleC(ModuleA moduleA, OrderRecorder recorder)
        {
            _ = moduleA;
            _recorder = recorder;
        }

        public void ConfigureServices(ServiceContext context)
        {
            _recorder.Order.Add("C");
        }
    }

    public interface IDup
    {
    }

    [InjectOnSingleton]
    public sealed class DupService : IDup
    {
    }

    public sealed class ModuleD : IModule
    {
        public void ConfigureServices(ServiceContext context)
        {
        }
    }

    [InjectModule<ModuleD>]
    public sealed class ModuleE : IModule
    {
        public void ConfigureServices(ServiceContext context)
        {
        }
    }

    [InjectModule<ModuleB>]
    [InjectModule<ModuleC>]
    public sealed class RootModule : IModule
    {
        private readonly OrderRecorder _recorder;

        public RootModule(OrderRecorder recorder)
        {
            _recorder = recorder;
        }

        public void ConfigureServices(ServiceContext context)
        {
            _recorder.Order.Add("Root");
        }
    }

    #endregion
}

/// <summary>
/// 集成测试.
/// </summary>
public class MaomiCoreAotIntegrationTests
{
    private static (IServiceCollection Services, TestFixture.OrderRecorder Recorder) CreateCollection()
    {
        var services = new ServiceCollection();
        var recorder = new TestFixture.OrderRecorder();
        services.AddSingleton(recorder);
        return (services, recorder);
    }

    [Fact]
    public void Resolve_By_Interface_With_Scoped_Lifetime()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        var clock = provider.GetRequiredService<TestFixture.IClock>();
        Assert.IsType<TestFixture.SystemClock>(clock);

        using var scope = provider.CreateScope();
        Assert.NotSame(provider.GetRequiredService<TestFixture.IClock>(), scope.ServiceProvider.GetRequiredService<TestFixture.IClock>());
    }

    [Fact]
    public void Transient_Resolve_New_Instance_Every_Time()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<TestFixture.ICounterService>();
        var second = provider.GetRequiredService<TestFixture.ICounterService>();
        Assert.NotSame(first, second);
        Assert.Equal(1, first.Next());
        Assert.Equal(1, second.Next());
    }

    [Fact]
    public void Self_Registration_Without_Interface()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        var selfService = provider.GetRequiredService<TestFixture.SelfService>();
        Assert.True(selfService.Alive);
    }

    [Fact]
    public void Own_Registers_Interface_And_Implementation()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<TestFixture.OwnService>(provider.GetRequiredService<TestFixture.IOwned>());
        Assert.IsType<TestFixture.OwnService>(provider.GetRequiredService<TestFixture.OwnService>());
    }

    [Fact]
    public void Some_Scheme_Registers_Only_Listed_ServiceTypes()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<TestFixture.SomeService>(provider.GetRequiredService<TestFixture.ISomeA>());
        Assert.IsType<TestFixture.SomeService>(provider.GetRequiredService<TestFixture.ISomeB>());
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<TestFixture.ISomeC>());
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<TestFixture.SomeService>());
    }

    [Fact]
    public void None_Scheme_Registers_Nothing_Unless_Own()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<TestFixture.INever>());
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<TestFixture.NeverService>());
        Assert.IsType<TestFixture.NeverOwnService>(provider.GetRequiredService<TestFixture.NeverOwnService>());
    }

    [Fact]
    public void Keyed_Services_Resolve_By_Key()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        var mysql = provider.GetRequiredKeyedService<TestFixture.IKeyedRepo>("mysql");
        var redis = provider.GetRequiredKeyedService<TestFixture.IKeyedRepo>("redis");
        Assert.Equal("mysql", mysql.Name);
        Assert.Equal("redis", redis.Name);

        var storage = provider.GetRequiredService<TestFixture.IStorage>();
        Assert.Equal("mysql", storage.Backend);
    }

    [Fact]
    public void Constructor_Injection_With_Default_Value_Parameter()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        // 修正版语义：OnlyInterfaces 且 Own = false 时只注册接口
        var deepService = (TestFixture.DeepService)provider.GetRequiredService<TestFixture.IDeep>();
        Assert.IsType<TestFixture.SystemClock>(deepService.Clock);
        Assert.True(deepService.Flag);
    }

    [Fact]
    public void IServiceProvider_Parameter_Is_Injected()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        var viaProvider = provider.GetRequiredService<TestFixture.IViaProvider>();
        Assert.NotNull(viaProvider.Provider);
    }

    [Fact]
    public void Filtered_Types_Are_Not_Registered()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<TestFixture.FilteredService>(provider.GetRequiredService<TestFixture.IFiltered>());
        Assert.Null(provider.GetService<IDisposable>());
    }

    [Fact]
    public void Fallback_Registration_Resolves_Via_Reflection()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<TestFixture.LegacyService>(provider.GetRequiredService<TestFixture.ILegacy>());
    }

    [Fact]
    public void Module_Dependency_Initializes_Before_Entry()
    {
        var (services, recorder) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        Assert.Equal(new[] { "A", "B", "C", "Root" }, recorder.Order);
    }

    [Fact]
    public void Module_Can_Register_Services_In_ConfigureServices()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<TestFixture.FromModuleService>(provider.GetRequiredService<TestFixture.IRegisterFromModule>());
    }

    [Fact]
    public void Module_Constructor_Can_Inject_Another_Module()
    {
        var (services, recorder) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        // ModuleC 的构造函数注入了 ModuleA 实例，且 C 的初始化在 A 之后
        Assert.Equal(new[] { "A", "B", "C", "Root" }, recorder.Order);
    }

    [Fact]
    public void Same_Assembly_With_Multiple_Modules_Registers_Once()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<TestFixture.DupService>(provider.GetRequiredService<TestFixture.IDup>());
        Assert.Single(services.Where(descriptor => descriptor.ServiceType == typeof(TestFixture.IDup)));
    }

    [Fact]
    public void AddModule_By_Type_Overload()
    {
        var (services, recorder) = CreateCollection();
        services.AddModule(typeof(TestFixture.RootModule));
        using var provider = services.BuildServiceProvider();

        Assert.Equal(new[] { "A", "B", "C", "Root" }, recorder.Order);
        Assert.NotNull(provider.GetRequiredService<TestFixture.IClock>());
    }

    [Fact]
    public void AddModule_Throws_When_Entry_Is_Not_A_Module()
    {
        var (services, _) = CreateCollection();
        Assert.Throws<TypeLoadException>(() => services.AddModule(typeof(TestFixture.SystemClock)));
    }

    [Fact]
    public void Context_Modules_Contain_Recognized_Modules()
    {
        var (services, _) = CreateCollection();
        services.AddModule<TestFixture.RootModule>();
        using var provider = services.BuildServiceProvider();

        // 模块上下文中记录了全部可达模块：ModuleA、ModuleB、ModuleC、RootModule
        Assert.Equal(4, TestFixture.ModuleA.CapturedModuleCount);
    }
}
