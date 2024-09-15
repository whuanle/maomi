// <copyright file="IModule.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

namespace Maomi;

/// <summary>
/// 模块接口.
/// </summary>
public interface IModule
{
    /// <summary>
    /// 模块中的依赖注入.
    /// </summary>
    /// <param name="context">模块服务上下文.</param>
    void ConfigureServices(ServiceContext context);
}
