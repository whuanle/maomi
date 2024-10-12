// <copyright file="MaomiWebModule.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Maomi.I18n;

namespace Maomi.Web.Core;

/// <summary>
/// Web 扩展方法.
/// </summary>
public class MaomiWebModule : IModule
{
    /// <inheritdoc/>
    public void ConfigureServices(ServiceContext context)
    {
        // i18n 服务
        context.Services.AddI18nAspNetCore();

        // 添加控制器
        context.Services.AddControllers(options =>
        {
        })
            .AddI18nDataAnnotation();
    }
}
