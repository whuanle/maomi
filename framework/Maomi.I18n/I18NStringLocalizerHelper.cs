// <copyright file="I18nStringLocalizer.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Maomi.I18n;

/// <summary>
/// I18nStringLocalizer 帮助类.
/// </summary>
internal static class I18NStringLocalizerHelper
{
    /// <summary>
    /// 查找资源.
    /// </summary>
    /// <param name="resources"></param>
    /// <param name="language"></param>
    /// <param name="name"></param>
    /// <returns>LocalizedString.</returns>
    internal static LocalizedString Find(IEnumerable<I18nResource> resources, string language, string name)
    {
        foreach (var resource in resources)
        {
            if (language != resource.SupportedCulture.Name)
            {
                continue;
            }

            var result = resource.Get(language, name);
            if (result == null || result.ResourceNotFound)
            {
                continue;
            }

            return result;
        }

        foreach (var resource in resources)
        {
            if (language != resource.SupportedCulture.Name)
            {
                continue;
            }

            var result = resource.Get(language, name);
            if (result == null || result.ResourceNotFound)
            {
                continue;
            }

            return result;
        }

        // 所有的资源都查找不到时，使用默认值
        return new LocalizedString(name, name, resourceNotFound: true);
    }

    /// <summary>
    /// 查找资源.
    /// </summary>
    /// <param name="resources"></param>
    /// <param name="language"></param>
    /// <param name="name"></param>
    /// <param name="arguments"></param>
    /// <returns>LocalizedString.</returns>
    internal static LocalizedString Find(IEnumerable<I18nResource> resources, string language, string name, params object[] arguments)
    {
        foreach (var resource in resources)
        {
            if (language != resource.SupportedCulture.Name)
            {
                continue;
            }

            var result = resource.Get(language, name, arguments);
            if (result == null || result.ResourceNotFound)
            {
                continue;
            }

            return result;
        }

        foreach (var resource in resources)
        {
            if (language != resource.SupportedCulture.Name)
            {
                continue;
            }

            var result = resource.Get(language, name, arguments);
            if (result == null || result.ResourceNotFound)
            {
                continue;
            }

            return result;
        }

        // 所有的资源都查找不到时，使用默认值
        return new LocalizedString(name, string.Format(name, arguments), resourceNotFound: true);
    }
}
