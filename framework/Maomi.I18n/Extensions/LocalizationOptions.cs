// <copyright file="LocalizationOptions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace Maomi.I18n;

/// <summary>
/// 多语言配置.
/// </summary>
public class LocalizationOptions
{
    /// <summary>
    /// 多语言默认语言.
    /// </summary>
    public string DefaultLanguage { get; init; } = "zh-CN";
}