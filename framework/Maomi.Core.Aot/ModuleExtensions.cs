// <copyright file="ModuleExtensions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maomi;

/// <summary>
/// 模块扩展.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// 注册模块化服务，在编译期由源生成器分析模块依赖树和服务注册，运行时零反射.
    /// </summary>
    /// <typeparam name="TModule">入口模块.</typeparam>
    /// <param name="services"><see cref="IServiceCollection"/>.</param>
    public static void AddModule<TModule>(this IServiceCollection services)
        where TModule : IModule
    {
        AddModule(services, typeof(TModule));
    }

    /// <summary>
    /// 注册模块化服务，在编译期由源生成器分析模块依赖树和服务注册，运行时零反射.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/>.</param>
    /// <param name="startupModule">入口模块.</param>
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "生成器无法生成工厂委托时的反射回退路径，该类型已通过 MAOMI003 提示，属于有意保留的行为")]
    public static void AddModule(this IServiceCollection services, Type startupModule)
    {
        ArgumentNullException.ThrowIfNull(startupModule, nameof(startupModule));

        if (!typeof(IModule).IsAssignableFrom(startupModule))
        {
            throw new TypeLoadException($"{startupModule.Name} does not implement {nameof(IModule)}");
        }

        if (!MaomiAotRegistry.HasGeneratedCode || !MaomiAotRegistry.TryGetDependencies(startupModule, out _))
        {
            throw new InvalidOperationException(
                $"Maomi.Core.Aot source generator did not generate a registration plan for module {startupModule.FullName}. " +
                $"Known modules: [{MaomiAotRegistry.DescribeKnownModules()}]. " +
                "Make sure the module class is defined in the current compilation or reachable from source-defined modules / AddModule call sites, " +
                "and that the assembly references the Maomi.Core.Aot package.");
        }

        // 构建模块执行顺序（深度优先、先依赖后入口），并做循环依赖防御检测
        List<Type> orderedModules = BuildOrderedModules(startupModule);

        // 将所有可达模块注册为单例，模块类可通过构造函数注入依赖
        foreach (var moduleType in orderedModules)
        {
            if (MaomiAotRegistry.TryGetFactory(moduleType, out var moduleFactory))
            {
                // 生成器已生成工厂委托：零反射注册
                services.AddSingleton(moduleType, moduleFactory);
            }
            else
            {
                // 回退路径：生成器无法生成工厂委托时按类型注册，由容器在运行时反射激活；
                // 该类型已被生成器通过 MAOMI003 提示，属于有意保留的反射回退
#pragma warning disable IL2072
                services.AddSingleton(moduleType);
#pragma warning restore IL2072
            }
        }

        // 与原版行为一致：先构建临时容器，用于实例化模块；此后模块 ConfigureServices 中注册的服务不会出现在临时容器中
        using ServiceProvider tempProvider = services.BuildServiceProvider();
        IConfiguration configuration = tempProvider.GetService<IConfiguration>()!;
        DefaultServiceContext serviceContext = new(services, configuration);
        foreach (var moduleType in orderedModules)
        {
            serviceContext.AddModule(new ModuleRecord
            {
                Type = moduleType,
                Assembly = moduleType.Assembly
            });
        }

        // 第一阶段：按依赖顺序实例化所有模块并执行 ConfigureServices（先依赖模块、后入口模块）
        foreach (var moduleType in orderedModules)
        {
            var module = (IModule)tempProvider.GetRequiredService(moduleType);
            module.ConfigureServices(serviceContext);
        }

        // 第二阶段：按模块顺序执行其所在程序集的服务注册代码块（同一程序集只执行一次）
        HashSet<Assembly> scannedAssemblies = new();
        foreach (var moduleType in orderedModules)
        {
            if (!scannedAssemblies.Add(moduleType.Assembly))
            {
                continue;
            }

            if (MaomiAotRegistry.TryGetChunk(moduleType, out var chunk))
            {
                chunk(services);
            }
        }
    }

    /// <summary>
    /// 从入口模块开始，深度优先构建模块执行顺序（依赖模块在前、入口模块在后），与原版 <c>ModuleBuilder</c> 行为一致.
    /// </summary>
    /// <param name="startupModule">入口模块.</param>
    /// <returns>模块执行顺序.</returns>
    private static List<Type> BuildOrderedModules(Type startupModule)
    {
        List<Type> orderedModules = new();
        HashSet<Type> doneModules = new();
        List<Type> visitingStack = new();

        Visit(startupModule);

        return orderedModules;

        void Visit(Type moduleType)
        {
            if (doneModules.Contains(moduleType))
            {
                return;
            }

            int index = visitingStack.IndexOf(moduleType);
            if (index >= 0)
            {
                var loopPath = visitingStack.Skip(index).Select(x => x.Name);
                throw new InvalidOperationException(
                    $"Loop dependent reference or duplicate reference detected.{string.Join(" -> ", loopPath)} -> {moduleType.Name}.");
            }

            visitingStack.Add(moduleType);
            if (MaomiAotRegistry.TryGetDependencies(moduleType, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    Visit(dependency);
                }
            }

            visitingStack.RemoveAt(visitingStack.Count - 1);
            doneModules.Add(moduleType);
            orderedModules.Add(moduleType);
        }
    }
}
