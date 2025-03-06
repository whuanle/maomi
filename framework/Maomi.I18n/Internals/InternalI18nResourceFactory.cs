// <copyright file="InternalI18nResourceFactory.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Globalization;

namespace Maomi.I18n;

/// <summary>
/// i18n 语言资源管理器.
/// </summary>
public class InternalI18nResourceFactory : I18nResourceFactory
{
    private readonly HashSet<CultureInfo> _supportedCultures;
    private readonly HashSet<I18nResource> _resources;
    private readonly HashSet<Type> _serviceResources;

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalI18nResourceFactory"/> class.
    /// </summary>
    public InternalI18nResourceFactory()
    {
        _supportedCultures = new();
        _resources = new HashSet<I18nResource>();
        _serviceResources = new();
    }

    /// <inheritdoc/>
    public ICollection<CultureInfo> SupportedCultures => _supportedCultures;

    /// <inheritdoc/>
    public ICollection<I18nResource> Resources => _resources;

    /// <inheritdoc/>
    public ICollection<Type> ServiceResources => _serviceResources;

    /// <inheritdoc/>
    public I18nResourceFactory Add(I18nResource resource)
    {
        _supportedCultures.Add(resource.SupportedCulture);
        _resources.Add(resource);
        return this;
    }

    /// <inheritdoc/>
    public I18nResourceFactory Add<T>(I18nResource<T> resource)
    {
        _supportedCultures.Add(resource.SupportedCulture);
        _resources.Add(resource);
        return this;
    }

    /// <inheritdoc/>
    public I18nResourceFactory AddServiceType(Type resourceType)
    {
        _serviceResources.Add(resourceType);
        return this;
    }
}
