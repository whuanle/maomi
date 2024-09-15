// <copyright file="MaomiWebModule.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
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
        context.Services.AddI18n("zh-CN");
        context.Services.AddI18nResource(options =>
        {
            options.AddJsonDirectory<MaomiWebModule>("i18n/Maomi.Web.Core");
        });

        // 添加控制器
        context.Services.AddControllers(options =>
        {
        })
            .AddI18nDataAnnotation();
    }
}
