using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Maomi.I18n
{
    public class JsonResource<TResource> : I18nResource
    {
        protected readonly string _language;

        public IReadOnlyList<CultureInfo> SupportedCultures => new List<CultureInfo>() { new CultureInfo(_language) };
        public IReadOnlyList<CultureInfo> SupportedUICultures => new List<CultureInfo>() { new CultureInfo(_language) };


        private readonly IReadOnlyDictionary<string, LocalizedString> _kvs;

        public JsonResource(string language, IReadOnlyDictionary<string, object> kvs)
        {
            _language = language;
            _kvs = kvs.ToDictionary(x => x.Key, x => new LocalizedString(x.Key, x.Value.ToString()));
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => _kvs.Values;

        public LocalizedString Get(string culture, string name)
        {
            if (culture != _language) return new LocalizedString(name, name, resourceNotFound: true);

            var value = _kvs.GetValueOrDefault(name);
            if (value == null) return new LocalizedString(name, name, resourceNotFound: true);
            return value;
        }

        public LocalizedString Get(string culture, string name, params object[] arguments)
        {
            if (culture != _language) return new LocalizedString(name, name, resourceNotFound: true);

            var value = _kvs.GetValueOrDefault(name);
            if (value == null) return new LocalizedString(name, name, resourceNotFound: true);

            return new LocalizedString(name, string.Format(value, arguments));
        }

        public LocalizedString Get<T>(string culture, string name)
        {
            // 不是同一个程序集的资源，不处理
            if (typeof(TResource).Assembly != typeof(T).Assembly) return new LocalizedString(name, name, resourceNotFound: true);
            return Get(culture, name);
        }

        public LocalizedString Get<T>(string culture, string name, params object[] arguments)
        {
            if (typeof(TResource).Assembly != typeof(T).Assembly) return new LocalizedString(name, name, resourceNotFound: true);
            return Get(culture, name, arguments);
        }
    }
}
