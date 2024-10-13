// <copyright file="I18nExtensions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

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
    public static void AddI18nAspNetCore(this IServiceCollection services, string defaultLanguage = "zh-CN")
    {
        services.AddI18n(defaultLanguage);

        var resourceFactory = services.BuildServiceProvider().GetRequiredService<I18nResourceFactory>();

        // ASP.NET Core 自带的
        services.AddLocalization();

        // 配置 ASP.NET Core 的本地化服务
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.DefaultRequestCulture = new RequestCulture(culture: defaultLanguage, uiCulture: defaultLanguage);
            options.SupportedCultures = resourceFactory.SupportedCultures;
            options.SupportedUICultures = resourceFactory.SupportedCultures;

            // 默认自带了三个请求语言提供器，会先从这些提供器识别要使用的语言。
            // QueryStringRequestCultureProvider
            // CookieRequestCultureProvider
            // AcceptLanguageHeaderRequestCultureProvider
            // 自定义请求请求语言提供器
            options.RequestCultureProviders.Add(new InternalRequestCultureProvider(options));
        });

        // i18n 上下文
        services.AddScoped<I18nContext, HttpI18nContext>();
    }

    /// <summary>
    /// i18n 中间件.
    /// </summary>
    /// <param name="app"></param>
    public static void UseI18n(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>();
        app.UseRequestLocalization(options.Value);
    }
}
