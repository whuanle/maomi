// <copyright file="I18nStringLocalizerFactory.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Maomi.I18n;

/// <summary>
/// 表示创建<see cref="IStringLocalizer"/> 实例的工厂.
/// </summary>
public class I18nStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IServiceScopeFactory _serviceScope;

    /// <summary>
    /// Initializes a new instance of the <see cref="I18nStringLocalizerFactory"/> class.
    /// </summary>
    /// <param name="serviceScope"></param>
    public I18nStringLocalizerFactory(IServiceScopeFactory serviceScope)
    {
        _serviceScope = serviceScope;
    }

    /// <inheritdoc/>
    public IStringLocalizer Create(Type resourceSource)
    {
        var ioc = _serviceScope.CreateScope().ServiceProvider;
        var type = typeof(I18nStringLocalizer<>).MakeGenericType(resourceSource);
        return (ioc.GetRequiredService(type) as IStringLocalizer)!;
    }

    /// <inheritdoc/>
    public IStringLocalizer Create(string baseName, string location)
    {
        var ioc = _serviceScope.CreateScope().ServiceProvider;

        return ioc.GetRequiredService<IStringLocalizer>();
    }
}
