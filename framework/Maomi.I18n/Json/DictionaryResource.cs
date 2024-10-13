// <copyright file="DictionaryResource.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Maomi.I18n;

/// <summary>
/// 字典存储多语言文件资源.
/// </summary>
public class DictionaryResource : I18nResource
{
    /// <inheritdoc/>
    public CultureInfo SupportedCulture => _cultureInfo;

    private readonly CultureInfo _cultureInfo;
    private readonly IReadOnlyDictionary<string, LocalizedString> _kvs;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryResource"/> class.
    /// </summary>
    /// <param name="cultureInfo"></param>
    /// <param name="kvs"></param>
    public DictionaryResource(CultureInfo cultureInfo, IReadOnlyDictionary<string, object> kvs)
    {
        _cultureInfo = cultureInfo;
        _kvs = kvs.ToDictionary(x => x.Key, x => new LocalizedString(x.Key, x.Value.ToString()!));
    }

    /// <inheritdoc/>
    public virtual LocalizedString Get(string culture, string name)
    {
        if (culture != _cultureInfo.Name)
        {
            return new LocalizedString(name, name, resourceNotFound: true);
        }

        var value = _kvs.GetValueOrDefault(name);
        if (value == null)
        {
            return new LocalizedString(name, name, resourceNotFound: true);
        }

        return value;
    }

    /// <inheritdoc/>
    public virtual LocalizedString Get(string culture, string name, params object[] arguments)
    {
        if (culture != _cultureInfo.Name)
        {
            return new LocalizedString(name, name, resourceNotFound: true);
        }

        var value = _kvs.GetValueOrDefault(name);
        if (value == null)
        {
            return new LocalizedString(name, name, resourceNotFound: true);
        }

        return new LocalizedString(name, string.Format(value, arguments));
    }

    /// <inheritdoc/>
    public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return _kvs.Values;
    }
}
