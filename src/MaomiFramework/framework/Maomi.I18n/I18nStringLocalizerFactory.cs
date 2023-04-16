using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;

namespace Maomi.I18n
{
    //
    public class I18nStringLocalizerFactory : IStringLocalizerFactory
    {
        #region 静态

        private static readonly ConcurrentDictionary<string, IStringLocalizer> Cache = new();
        public static void Add(string path, string language, IStringLocalizer localizer)
        {
            Cache[$"{path}.{language}"] = localizer;
        }

        public static IStringLocalizer Get(string location) => Cache.GetValueOrDefault(location);

        #endregion

        public IStringLocalizer Create(Type resourceSource) => Get($"{resourceSource.Assembly.GetName().Name}.{CultureInfo.CurrentCulture.Name}");
        public IStringLocalizer Create(string baseName, string location) => Get($"{baseName}.{location}");
    }
}
