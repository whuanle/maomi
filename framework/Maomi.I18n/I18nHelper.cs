// <copyright file="I18nHelper.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Maomi.I18n;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Maomi;

/// <summary>
/// i18n 帮助器.
/// </summary>
public static class I18nHelper
{
    /// <summary>
    /// 创建语言资源工厂.
    /// </summary>
    /// <returns><see cref="I18nResourceFactory"/>.</returns>
    public static I18nResourceFactory CreateFactory()
    {
        return new InternalI18nResourceFactory();
    }

    /// <summary>
    /// 创建多语言翻译接口.
    /// </summary>
    /// <param name="context">多语言上下文.</param>
    /// <param name="resourceFactory">多语言资源工厂.</param>
    /// <param name="serviceProvider">服务提供器.</param>
    /// <returns><see cref="IStringLocalizer"/>.</returns>
    public static IStringLocalizer CreateStringLocalizer(I18nContext context, I18nResourceFactory? resourceFactory = null, IServiceProvider? serviceProvider = null)
    {
        if (resourceFactory == null)
        {
            resourceFactory = CreateFactory();
        }

        if (serviceProvider == null)
        {
            serviceProvider = new ServiceCollection().BuildServiceProvider();
        }

        return new I18nStringLocalizer(context, resourceFactory, serviceProvider);
    }
}
