using Microsoft.Extensions.Localization;

namespace Maomi.I18n
{
    /// <summary>
    /// i18n 字符串本地化，从 I18nResource 获取字符串
    /// </summary>
    public class I18nStringLocalizer : IStringLocalizer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly I18nContext _context;
        private readonly IReadOnlyList<I18nResource> _resources;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="context"></param>
        /// <param name="resourceFactory"></param>
        public I18nStringLocalizer(IServiceProvider serviceProvider, I18nContext context, I18nResourceFactory resourceFactory)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _resources = resourceFactory.Resources.ToList();
        }

        public LocalizedString this[string name] => Find(name);

        public LocalizedString this[string name, params object[] arguments] => Find(name, arguments);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var rs = _serviceProvider.GetRequiredService<IEnumerable<I18nResource>>();
            foreach(var resource in rs)
            {
                foreach (var item in resource.GetAllStrings(includeParentCultures))
                {
                    yield return item;
                }
            }
            foreach (var resource in _resources)
            {
                foreach (var item in resource.GetAllStrings(includeParentCultures))
                {
                    yield return item;
                }
            }
        }

        private LocalizedString Find(string name)
        {
            var rs = _serviceProvider.GetRequiredService<IEnumerable<I18nResource>>();
            foreach (var resource in rs)
            {
                var result = resource.Get(_context.Culture.Name, name);
                if (result == null || result.ResourceNotFound) continue;
                return result;
            }
            foreach (var resource in _resources)
            {
                var result = resource.Get(_context.Culture.Name, name);
                if (result == null || result.ResourceNotFound) continue;
                return result;
            }
            // 所有的资源都查找不到时，使用默认值
            return new LocalizedString(name, name);
        }

        private LocalizedString Find(string name, params object[] arguments)
        {
            var rs = _serviceProvider.GetRequiredService<IEnumerable<I18nResource>>();
            foreach (var resource in rs)
            {
                var result = resource.Get(_context.Culture.Name, name, arguments);
                if (result == null || result.ResourceNotFound) continue;
                return result;
            }
            foreach (var resource in _resources)
            {
                var result = resource.Get(_context.Culture.Name, name, arguments);
                if (result == null || result.ResourceNotFound) continue;
                return result;
            }
            // 所有的资源都查找不到时，使用默认值
            return new LocalizedString(name, name);
        }
    }
}
