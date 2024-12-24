using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows;

namespace Maomi.I18n;

/// <summary>
/// i18n 扩展方法.
/// </summary>
public static class I18nExtensions
{
    /// <summary>
    /// 添加 wpf 多语言支持.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="appName">程序名称.</param>
    /// <param name="localization">多语言资源文件路径.</param>
    public static void AddI18nWpf(this IServiceCollection services, string appName, string localization = "localization")
    {
        var wpfI18NOptions = new WpfI18nOptions
        {
            AppName = appName,
            Localization = localization
        };

        services.AddSingleton<WpfI18nOptions>(wpfI18NOptions);
        services.AddScoped<I18nContext, WpfI18nContext>();

        Dictionary<string, string> xamlFiles = new();

        // 读取 Localization 目录下的所有资源字典
        foreach (string resName in System.Windows.Application.ResourceAssembly.GetManifestResourceNames())
        {
            using var resStream = System.Windows.Application.ResourceAssembly.GetManifestResourceStream(resName);
            if (resStream == null)
            {
                continue;
            }

            using ResourceReader resourceReader = new ResourceReader(resStream);

            // 检查是否多语言资源文件
            foreach (DictionaryEntry resourceEntry in resourceReader)
            {
                var fileName = resourceEntry.Key.ToString();
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                if (fileName.StartsWith(localization, StringComparison.CurrentCultureIgnoreCase))
                {
                    xamlFiles.Add(Path.GetFileNameWithoutExtension(fileName), fileName);
                }
            }
        }

        services.AddI18nResource(f =>
        {
            // 读取每个多语言资源文件
            foreach (var item in xamlFiles)
            {
                string resourceDictionaryPath = $"pack://application:,,,/{localization}/{item.Key}.xaml"; // 替换为你的资源字典路径
                var resourceDictionary = new ResourceDictionary
                {
                    Source = new Uri(resourceDictionaryPath, UriKind.RelativeOrAbsolute)
                };

                Dictionary<string, object> dictionary = ResourceDictionaryToDictionary(resourceDictionary);
                f.Add(new DictionaryResource(new CultureInfo(item.Key), dictionary));
            }
        });

        // 解析 xaml 资源字典
        Dictionary<string, object> ResourceDictionaryToDictionary(ResourceDictionary resourceDictionary)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var key in resourceDictionary.Keys)
            {
                dictionary[key.ToString()!] = resourceDictionary[key];
            }

            return dictionary;
        }
    }
}