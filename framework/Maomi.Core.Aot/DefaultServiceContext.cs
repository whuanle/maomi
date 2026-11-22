// <copyright file="DefaultServiceContext.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maomi;

/// <summary>
/// 模块上下文.
/// </summary>
public class DefaultServiceContext : ServiceContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultServiceContext"/> class.
    /// </summary>
    /// <param name="serviceCollection">容器服务集合.</param>
    /// <param name="configuration">配置.</param>
    public DefaultServiceContext(IServiceCollection serviceCollection, IConfiguration configuration)
        : base(serviceCollection, configuration)
    {
    }

    /// <summary>
    /// 添加模块记录.
    /// </summary>
    /// <param name="initRecord">模块记录.</param>
    public void AddModule(ModuleRecord initRecord)
    {
        AddModuleRecord(initRecord);
    }
}
