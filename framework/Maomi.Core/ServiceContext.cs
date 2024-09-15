// <copyright file="ServiceContext.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

#pragma warning disable CS1591
#pragma warning disable SA1401
#pragma warning disable SA1600

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maomi;

/// <summary>
/// 模块上下文.
/// </summary>
public abstract class ServiceContext
{
    protected readonly IServiceCollection _serviceCollection;
    protected readonly IConfiguration _configuration;
    protected readonly List<ModuleRecord> _modules;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceContext"/> class.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configuration"></param>
    internal ServiceContext(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        _serviceCollection = serviceCollection;
        _configuration = configuration;
        _modules = new List<ModuleRecord>();
    }

    /// <summary>
    /// 容器服务集合.
    /// </summary>
    public IServiceCollection Services => _serviceCollection;

    /// <summary>
    /// 配置.
    /// </summary>
    public IConfiguration Configuration => _configuration;

    /// <summary>
    /// 已识别到的模块列表.
    /// </summary>
    public IReadOnlyList<ModuleRecord> Modules => _modules;
}
