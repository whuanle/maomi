// <copyright file="I18nRequestCultureProvider.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.AspNetCore.Localization;

namespace Maomi.I18n;

/// <summary>
/// 自定义如何从请求中解析请求语言.
/// </summary>
public class InternalRequestCultureProvider : RequestCultureProvider
{
    private const string RouteValueKey = "c";
    private const string UIRouteValueKey = "uic";

    private readonly RequestLocalizationOptions _requestLocalizationOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalRequestCultureProvider"/> class.
    /// </summary>
    /// <param name="requestLocalizationOptions"></param>
    public InternalRequestCultureProvider(RequestLocalizationOptions requestLocalizationOptions)
    {
        _requestLocalizationOptions = requestLocalizationOptions;
    }

    /// <inheritdoc/>
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var request = httpContext.Request;
        if (!request.RouteValues.Any())
        {
            return NullProviderCultureResult;
        }

        string? queryCulture = null;
        string? queryUICulture = null;

        // 从路由中解析
        if (!string.IsNullOrWhiteSpace(RouteValueKey))
        {
            queryCulture = request.RouteValues[RouteValueKey]?.ToString();
        }

        if (!string.IsNullOrWhiteSpace(UIRouteValueKey))
        {
            queryUICulture = request.RouteValues[UIRouteValueKey]?.ToString() ?? queryCulture;
        }

        if (queryCulture == null && queryUICulture == null)
        {
            return NullProviderCultureResult;
        }

        var providerResultCulture = new ProviderCultureResult(queryCulture, queryUICulture);

        return Task.FromResult<ProviderCultureResult?>(providerResultCulture);
    }
}
