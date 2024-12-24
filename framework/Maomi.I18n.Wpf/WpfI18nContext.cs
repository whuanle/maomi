using System.Globalization;
using System.Linq;
using System.Windows;

namespace Maomi.I18n;

/// <summary>
/// i18n 上下文设置.
/// </summary>
public class WpfI18nContext : I18nContext
{
    /// <summary>
    /// 配置.
    /// </summary>
    protected readonly WpfI18nOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WpfI18nContext"/> class.
    /// </summary>
    /// <param name="options"></param>
    public WpfI18nContext(WpfI18nOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// 设置当前程序所用语言.
    /// </summary>
    /// <param name="language"></param>
    public void SetLanguage(string language)
    {
        var currentCultureInfo = GetValidCulture(language);
        CultureInfo.CurrentCulture = currentCultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = currentCultureInfo;

        Thread.CurrentThread.CurrentCulture = currentCultureInfo;
        Thread.CurrentThread.CurrentUICulture = currentCultureInfo;

        // 切换 xaml 资源字典
        LoadLocalizationResource(Application.Current.Resources, currentCultureInfo);
    }

    /// <summary>
    /// 将语言名称转为 CultureInfo.
    /// </summary>
    /// <param name="cultureName"></param>
    /// <returns><see cref="CultureInfo"/>.</returns>
    protected virtual CultureInfo GetValidCulture(string cultureName)
    {
        var name = cultureName switch
        {
            "zh-Hant" or "zh-HK" or "zh-MO" or "zh-TW" or "zh-CHT" => "zh-HK",
            "zh" or "zh-CN" or "zh-Hans" or "zh-CHS" or "zh-SG" => "zh-CN",
            "ja" or "ja-JP" => "jp",
            "ko" or "ko-KP" or "ko-KR" => "ko",
            "ru" or "ru-RU" => "ru",
            "en-US" or "en" => "en-US",
            _ => "zh-CN"
        };

        CultureInfo culture = CultureInfo.CreateSpecificCulture(name);
        return culture;
    }

    /// <summary>
    /// 加载字典文件到资源字典中.
    /// </summary>
    /// <param name="resources"></param>
    /// <param name="cultureInfo"></param>
    protected virtual void LoadLocalizationResource(ResourceDictionary resources, CultureInfo cultureInfo)
    {
        var origin = resources.MergedDictionaries.FirstOrDefault(x => x.Source.OriginalString.Contains(_options.Localization));

        if (origin != null)
        {
            resources.MergedDictionaries.Remove(origin);
        }

        var uri = new Uri(@$"pack://application:,,,/{_options.AppName};component/{_options.Localization}/{cultureInfo.Name}.xaml");
        resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = uri
        });
    }
}
