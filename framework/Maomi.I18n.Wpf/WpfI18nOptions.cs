// <copyright file="WpfI18nOptions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

namespace Maomi.I18n;

/// <summary>
/// 配置.
/// </summary>
public class WpfI18nOptions
{
    /// <summary>
    /// 程序名称.
    /// </summary>
    public string AppName { get; init; } = default!;

    /// <summary>
    /// 路径.
    /// </summary>
    public string Localization { get; init; } = default!;
}
