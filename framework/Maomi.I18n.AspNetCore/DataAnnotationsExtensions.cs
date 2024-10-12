// <copyright file="DataAnnotationsExtensions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

namespace Maomi.I18n;

/// <summary>
/// 模型验证使用多语言.
/// </summary>
public static partial class DataAnnotationsExtensions
{
    /// <summary>
    /// 为 API 模型验证注入 i18n 服务.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns><see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddI18nDataAnnotation(this IMvcBuilder builder)
    {
        builder
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (modelType, stringLocalizerFactory) =>
                stringLocalizerFactory.Create(modelType);
            });
        return builder;
    }
}
