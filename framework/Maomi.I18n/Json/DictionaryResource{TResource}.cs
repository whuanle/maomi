// <copyright file="DictionaryResource{TResource}.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;

namespace Maomi.I18n;

/// <summary>
/// 字典存储多语言文件资源.
/// </summary>
/// <typeparam name="TResource">类型.</typeparam>
public class DictionaryResource<TResource> : DictionaryResource, I18nResource<TResource>
{
    private readonly Assembly _assembly;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryResource{TResource}"/> class.
    /// </summary>
    /// <param name="cultureInfo"></param>
    /// <param name="kvs"></param>
    /// <param name="assembly"></param>
    public DictionaryResource(CultureInfo cultureInfo, IReadOnlyDictionary<string, object> kvs, Assembly assembly)
        : base(cultureInfo, kvs)
    {
        _assembly = assembly;
    }

    /// <inheritdoc/>
    public override LocalizedString Get(string culture, string name)
    {
        if (typeof(TResource).Assembly != _assembly)
        {
            return new LocalizedString(name, name, resourceNotFound: true);
        }

        return base.Get(culture, name);
    }

    /// <inheritdoc/>
    public override LocalizedString Get(string culture, string name, params object[] arguments)
    {
        if (typeof(TResource).Assembly != _assembly)
        {
            return new LocalizedString(name, name, resourceNotFound: true);
        }

        return base.Get(culture, name, arguments);
    }
}
