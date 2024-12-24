// <copyright file="ModuleCore.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace Maomi;

/// <summary>
/// 模块过滤器接口.
/// </summary>
public abstract class ModuleCore : IModule
{
    /// <inheritdoc/>
    public abstract void ConfigureServices(ServiceContext context);

    /// <summary>
    /// 扫描每个类型时会调用该接口.
    /// </summary>
    /// <param name="type"></param>
    public abstract void TypeFilter(Type type);
}