// <copyright file="ModuleExtensions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

#pragma warning disable CS1591
#pragma warning disable SA1401
#pragma warning disable SA1600

using Microsoft.Extensions.DependencyInjection;

namespace Maomi;

/// <summary>
/// 模块扩展.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// 注册模块化服务.
    /// </summary>
    /// <typeparam name="TModule">入口模块.</typeparam>
    /// <param name="services"><see cref="IServiceCollection"/>.</param>
    /// <param name="options">配置.</param>
    public static void AddModule<TModule>(this IServiceCollection services, Action<ModuleOptions>? options = null)
        where TModule : IModule
    {
        AddModule(services, typeof(TModule));
    }

    /// <summary>
    /// 注册模块化服务.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/>.</param>
    /// <param name="startupModule">入口模块.</param>
    /// <param name="optionBuilder">配置.</param>
    public static void AddModule(this IServiceCollection services, Type startupModule, Action<ModuleOptions>? optionBuilder = null)
    {
        ArgumentNullException.ThrowIfNull(startupModule, nameof(startupModule));

        if (startupModule.GetInterface(nameof(IModule)) == null)
        {
            throw new TypeLoadException($"{startupModule?.Name} does not implement {nameof(IModule)}");
        }

        ModuleOptions initOptions = new();
        if (optionBuilder != null)
        {
            optionBuilder.Invoke(initOptions);
        }

        var ioc = services.BuildServiceProvider();
        ModuleBuilder moduleBuilder = new(services, initOptions, startupModule);
        moduleBuilder.Build();
    }
}