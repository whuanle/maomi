// <copyright file="MaomiSwaggerOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
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