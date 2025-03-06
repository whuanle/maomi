// <copyright file="I18nStringLocalizer{T}.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Maomi.I18n;

/// <summary>
/// 表示提供本地化字符串的服务.
/// </summary>
/// <typeparam name="T">类型.</typeparam>
public class I18nStringLocalizer<T> : IStringLocalizer<T>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly I18nContext _context;
    private readonly LocalizationOptions _localizationOptions;
    private readonly I18nResourceFactory _resourceFactory;
    private readonly Lazy<IReadOnlyList<I18nResource>> _iocLocalizerResources;
    private readonly Lazy<IReadOnlyList<I18nResource>> _staticLocalizerResources;

    /// <summary>
    /// Initializes a new instance of the <see cref="I18nStringLocalizer{T}"/> class.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="resourceFactory"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="localizationOptions"></param>
    public I18nStringLocalizer(I18nContext context, I18nResourceFactory resourceFactory, IServiceProvider serviceProvider, LocalizationOptions localizationOptions)
    {
        _context = context;
        _resourceFactory = resourceFactory;
        _serviceProvider = serviceProvider;
        _localizationOptions = localizationOptions;

        _iocLocalizerResources = new Lazy<IReadOnlyList<I18nResource>>(() =>
        {
            List<I18nResource> resources = new();
            foreach (var serviceType in _resourceFactory.ServiceResources)
            {
                if (!serviceType.IsGenericType || serviceType.GenericTypeArguments[0].Assembly != typeof(T).Assembly)
                {
                    continue;
                }

                var resource = _serviceProvider.GetRequiredService(serviceType) as I18nResource;
                if (resource == null)
                {
                    continue;
                }

                resources.Add(resource);
            }

            return resources;
        });

        _staticLocalizerResources = new Lazy<IReadOnlyList<I18nResource>>(() =>
        {
            List<I18nResource> resources = new();
            foreach (var resource in _resourceFactory.Resources)
            {
                if (!resource.GetType().IsGenericType || resource.GetType().GenericTypeArguments[0].Assembly != typeof(T).Assembly)
                {
                    continue;
                }

                resources.Add(resource);
            }

            return resources;
        });
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
        // 先查找静态实例
        var result = I18NStringLocalizerHelper.Find(_staticLocalizerResources.Value, _context.Culture.Name, name);
        if (!result.ResourceNotFound)
        {
            return result;
        }

        // 从容器中使用提供器查找
        result = I18NStringLocalizerHelper.Find(_iocLocalizerResources.Value, _context.Culture.Name, name);

        if (!result.ResourceNotFound)
        {
            return result;
        }

        // 降级使用默认语言
        if (result.ResourceNotFound == true && _localizationOptions.DefaultLanguage != _context.Culture.Name)
        {
            return result = I18NStringLocalizerHelper.Find(_iocLocalizerResources.Value, _localizationOptions.DefaultLanguage, name);
        }

        return result;
    }

    private LocalizedString Find(string name, params object[] arguments)
    {
        // 先查找静态实例
        var result = I18NStringLocalizerHelper.Find(_staticLocalizerResources.Value, _context.Culture.Name, name, arguments);
        if (!result.ResourceNotFound)
        {
            return result;
        }

        // 从容器中使用提供器查找
        result = I18NStringLocalizerHelper.Find(_iocLocalizerResources.Value, _context.Culture.Name, name, arguments);

        if (!result.ResourceNotFound)
        {
            return result;
        }

        // 降级使用默认语言
        if (result.ResourceNotFound == true && _localizationOptions.DefaultLanguage != _context.Culture.Name)
        {
            return result = I18NStringLocalizerHelper.Find(_iocLocalizerResources.Value, _localizationOptions.DefaultLanguage, name, arguments);
        }

        // 所有的资源都查找不到时，使用默认值
        return new LocalizedString(name, string.Format(name, arguments), resourceNotFound: true);
    }
}