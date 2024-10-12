// <copyright file="MaomiSwaggerOptions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

namespace Maomi.Web.Core;

/// <summary>
/// swagger 配置.
/// </summary>
public class MaomiSwaggerOptions
{
    /// <summary>
    /// 默认分组名称.
    /// </summary>
    public string DefaultGroupName { get; set; } = "default";

    /// <summary>
    /// 默认标题.
    /// </summary>
    public string DefaultGroupTitle { get; set; } = "default";
}