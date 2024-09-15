// <copyright file="I18nResource.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Maomi.I18n;

/// <summary>
/// i18n 语言资源.
/// </summary>
/// <remarks>每个 I18nResource 对应一种语言的一个资源文件.</remarks>
public interface I18nResource
{
    /// <summary>
    /// 该资源提供的语言.
    /// </summary>
    CultureInfo SupportedCulture { get; }

    /// <summary>
    /// 获取具有给定名称的字符串资源.
    /// </summary>
    /// <param name="culture">语言名字.</param>
    /// <param name="name">字符串名称.</param>
    /// <returns><see cref="LocalizedString"/>.</returns>
    LocalizedString Get(string culture, string name);

    /// <summary>
    /// 获取具有给定名称的字符串资源.
    /// </summary>
    /// <param name="culture">语言名字.</param>
    /// <param name="name">字符串名称.</param>
    /// <param name="arguments">字符串插值参数.</param>
    /// <returns><see cref="LocalizedString"/>.</returns>
    LocalizedString Get(string culture, string name, params object[] arguments);

    /// <summary>
    /// 从 i18n 资源文件中获取所有字符串.
    /// </summary>
    /// <param name="includeParentCultures"></param>
    /// <returns><see cref="LocalizedString"/>.</returns>
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures);
}

/// <summary>
/// i18n 语言资源.
/// </summary>
/// <remarks>每个 I18nResource 对应一种语言的一个资源文件.</remarks>
/// <typeparam name="T">类型.</typeparam>
public interface I18nResource<T> : I18nResource
{
}
