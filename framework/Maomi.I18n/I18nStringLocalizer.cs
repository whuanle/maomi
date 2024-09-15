// <copyright file="I18nStringLocalizer.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.Localization;

namespace Maomi.I18n;

/// <summary>
/// 表示提供本地化字符串的服务.
/// </summary>
public class I18nStringLocalizer : IStringLocalizer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly I18nContext _context;
    private readonly I18nResourceFactory _resourceFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="I18nStringLocalizer"/> class.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="resourceFactory"></param>
    /// <param name="serviceProvider"></param>
    public I18nStringLocalizer(I18nContext context, I18nResourceFactory resourceFactory, IServiceProvider serviceProvider)
    {
        _context = context;
        _resourceFactory = resourceFactory;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public LocalizedString this[string name] => Find(name);

    /// <inheritdoc/>
    public LocalizedString this[string name, params object[] arguments] => Find(name, arguments);

    /// <inheritdoc/>
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        foreach (var serviceType in _resourceFactory.ServiceResources)
        {
            var resource = _serviceProvider.GetRequiredService(serviceType) as I18nResource;
            if (resource == null)
            {
                continue;
            }

            foreach (var item in resource.GetAllStrings(includeParentCultures))
            {
                yield return item;
            }
        }

        foreach (var resource in _resourceFactory.Resources)
        {
            foreach (var item in resource.GetAllStrings(includeParentCultures))
            {
                yield return item;
            }
        }
    }

    private LocalizedString Find(string name)
    {
        foreach (var serviceType in _resourceFactory.ServiceResources)
        {
            var resource = _serviceProvider.GetRequiredService(serviceType) as I18nResource;
            if (resource == null)
            {
                continue;
            }

            var result = resource.Get(_context.Culture.Name, name);
            if (result == null || result.ResourceNotFound)
            {
                continue;
            }

            return result;
        }

        foreach (var resource in _resourceFactory.Resources)
        {
            var result = resource.Get(_context.Culture.Name, name);
            if (result == null || result.ResourceNotFound)
            {
                continue;
            }

            return result;
        }

        // 所有的资源都查找不到时，使用默认值
        return new LocalizedString(name, name);
    }

    private LocalizedString Find(string name, params object[] arguments)
    {
        foreach (var serviceType in _resourceFactory.ServiceResources)
        {
            var resource = _serviceProvider.GetRequiredService(serviceType) as I18nResource;
            if (resource == null)
            {
                continue;
            }

            var result = resource.Get(_context.Culture.Name, name, arguments);
            if (result == null || result.ResourceNotFound)
            {
                continue;
            }

            return result;
        }

        foreach (var resource in _resourceFactory.Resources)
        {
            var result = resource.Get(_context.Culture.Name, name, arguments);
            if (result == null || result.ResourceNotFound)
            {
                continue;
            }

            return result;
        }

        // 所有的资源都查找不到时，使用默认值
        return new LocalizedString(name, string.Format(name, arguments));
    }
}
