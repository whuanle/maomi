using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;

namespace Maomi.I18n
{
    internal class InternalI18nResourceFactory : I18nResourceFactory
    {
        private readonly List<I18nResource> _resources;
        public IReadOnlyList<I18nResource> Resources => _resources;

        private readonly List<CultureInfo> _supportedCultures = new();
        public IList<CultureInfo> SupportedCultures => _supportedCultures;

        private readonly List<CultureInfo> _supportedUICultures = new();
        public IList<CultureInfo> SupportedUICultures => _supportedUICultures;

        internal InternalI18nResourceFactory()
        {
            _resources = new List<I18nResource>();
        }

        public I18nResourceFactory Add(I18nResource resource)
        {
            _resources.Add(resource);
            foreach (var item in resource.SupportedCultures)
            {
                if (_supportedCultures.Contains(item)) continue;
                _supportedCultures.Add(item);
            }
            foreach (var item in resource.SupportedUICultures)
            {
                if (_supportedUICultures.Contains(item)) continue;
                _supportedUICultures.Add(item);
            }
            return this;
        }
    }
}
