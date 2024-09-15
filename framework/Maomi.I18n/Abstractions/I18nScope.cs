// <copyright file="I18nScope.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Globalization;

namespace Maomi.I18n;

/// <summary>
/// i18n 作用域.
/// </summary>
public class I18nScope : IDisposable
{
    private readonly CultureInfo _defaultCultureInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="I18nScope"/> class.
    /// </summary>
    /// <param name="language"></param>
    public I18nScope(string language)
    {
        _defaultCultureInfo = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture(language);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        CultureInfo.CurrentCulture = _defaultCultureInfo;
    }
}
