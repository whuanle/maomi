// <copyright file="I18nResourceFactory.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Globalization;

namespace Maomi.I18n;

/// <summary>
/// I18n 资源工厂.
/// </summary>
public interface I18nResourceFactory
{
    /// <summary>
    /// 当前支持的语言.
    /// </summary>
    ICollection<CultureInfo> SupportedCultures { get; }

    /// <summary>
    /// 所有资源提供器.
    /// </summary>
    ICollection<I18nResource> Resources { get; }

    /// <summary>
    /// 在容器中的资源服务.
    /// </summary>
    ICollection<Type> ServiceResources { get; }

    /// <summary>
    /// 添加 i18n 语言资源，该类型将会被从容器中取出.
    /// </summary>
    /// <param name="resourceType">i18n 语言资源.</param>
    /// <returns><see cref="I18nResourceFactory"/>.</returns>
    I18nResourceFactory AddServiceType(Type resourceType);

    /// <summary>
    /// 添加 i18n 语言资源.
    /// </summary>
    /// <param name="resource">i18n 语言资源.</param>
    /// <returns><see cref="I18nResourceFactory"/>.</returns>
    I18nResourceFactory Add(I18nResource resource);

    /// <summary>
    /// 添加 i18n 语言资源.
    /// </summary>
    /// <typeparam name="T">类型.</typeparam>
    /// <param name="resource">i18n 语言资源.</param>
    /// <returns><see cref="I18nResourceFactory"/>.</returns>
    I18nResourceFactory Add<T>(I18nResource<T> resource);
}
