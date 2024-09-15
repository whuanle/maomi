// <copyright file="DefaultServiceContext.cs" company="Maomi">
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
public class DefaultServiceContext : ServiceContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultServiceContext"/> class.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configuration"></param>
    public DefaultServiceContext(IServiceCollection serviceCollection, IConfiguration configuration)
        : base(serviceCollection, configuration)
    {
    }

    public void AddModule(ModuleRecord initRecord)
    {
        _modules.Add(initRecord);
    }
}