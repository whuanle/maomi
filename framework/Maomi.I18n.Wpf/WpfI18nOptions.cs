using System.Globalization;
using System.Windows;

namespace Maomi.I18n;

/// <summary>
/// 配置.
/// </summary>
public class WpfI18nOptions
{
    /// <summary>
    /// 程序名称.
    /// </summary>
    public string AppName { get; init; } = default!;

    /// <summary>
    /// 路径.
    /// </summary>
    public string Localization { get; init; } = default!;
}
