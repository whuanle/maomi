// <copyright file="I18nExtensions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace Maomi.I18n;

/// <summary>
/// i18n 扩展.
/// </summary>
public static class I18nExtensions
{
    /// <summary>
    /// 添加 i18n 支持服务.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="defaultLanguage">默认语言.</param>
    public static void AddI18n(this IServiceCollection services, string defaultLanguage = "zh-CN")
    {
        InternalI18nResourceFactory resourceFactory = new InternalI18nResourceFactory();

        services.AddSingleton(new LocalizationOptions { DefaultLanguage = defaultLanguage });

        // i18n 上下文
        services.AddScoped<I18nContext, DefaultI18nContext>();

        // 注入 i18n 服务
        services.AddSingleton<I18nResourceFactory>(s => resourceFactory);
        services.AddSingleton<IStringLocalizerFactory, I18nStringLocalizerFactory>();
        services.AddScoped<IStringLocalizer, I18nStringLocalizer>();
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IStringLocalizer<>), typeof(I18nStringLocalizer<>), ServiceLifetime.Scoped));
    }

    /// <summary>
    /// 添加 i18n 资源.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="resourceFactory"></param>
    public static void AddI18nResource(this IServiceCollection services, Action<I18nResourceFactory> resourceFactory)
    {
        var service = services.BuildServiceProvider().GetRequiredService<I18nResourceFactory>();
        resourceFactory.Invoke(service);
    }
}