// <copyright file="HttpI18nContext.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace Maomi.I18n;

/// <summary>
/// 从 http 请求上下文中获取多语言信息.
/// </summary>
public class HttpI18nContext : I18nContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpI18nContext"/> class.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="requestLocalizationOptions"></param>
    public HttpI18nContext(IHttpContextAccessor httpContextAccessor, IOptions<RequestLocalizationOptions> requestLocalizationOptions)
    {
        var requestCultureFeature = httpContextAccessor.HttpContext!.Features.Get<IRequestCultureFeature>();
        var requestCulture = requestCultureFeature?.RequestCulture;
        if (requestCulture != null)
        {
            Culture = requestCulture.Culture;
        }
        else
        {
            Culture = requestLocalizationOptions.Value.DefaultRequestCulture.Culture;
        }
    }
}
