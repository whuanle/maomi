// <copyright file="I18nContext.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Globalization;

namespace Maomi.I18n;

/// <summary>
/// 记录当前请求的 i18n 信息.
/// </summary>
public class I18nContext
{
    /// <summary>
    /// 当前用户请求的语言.
    /// </summary>
    public CultureInfo Culture { get; protected set; } = CultureInfo.CurrentCulture;
}
