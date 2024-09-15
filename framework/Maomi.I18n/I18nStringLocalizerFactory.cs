// <copyright file="I18nStringLocalizerFactory.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.Localization;

namespace Maomi.I18n;

/// <summary>
/// 表示创建<see cref="IStringLocalizer"/> 实例的工厂.
/// </summary>
public class I18nStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly I18nResourceFactory _i18nResourceFactory;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="I18nStringLocalizerFactory"/> class.
    /// </summary>
    /// <param name="i18nResourceFactory"></param>
    /// <param name="serviceProvider"></param>
    public I18nStringLocalizerFactory(I18nResourceFactory i18nResourceFactory, IServiceProvider serviceProvider)
    {
        _i18nResourceFactory = i18nResourceFactory;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public IStringLocalizer Create(Type resourceSource)
    {
        var type = typeof(I18nStringLocalizer<>).MakeGenericType(resourceSource);
        return (_serviceProvider.GetRequiredService(type) as IStringLocalizer)!;
    }

    /// <inheritdoc/>
    public IStringLocalizer Create(string baseName, string location)
    {
        return _serviceProvider.GetRequiredService<IStringLocalizer>();
    }
}
