using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Maomi.I18n
{
    public class I18nStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly I18nResourceFactory _i18NResourceFactory;
        public I18nStringLocalizerFactory(I18nResourceFactory i18NResourceFactory)
        {
            _i18NResourceFactory = i18NResourceFactory;
        }

        /// <summary>
        /// 根据泛型类型创建 IStringLocalizer
        /// </summary>
        /// <param name="resourceSource"></param>
        /// <returns></returns>
        public IStringLocalizer Create(Type resourceSource)
        {
            return Activator.CreateInstance(typeof(I18nStringLocalizer<>).MakeGenericType(resourceSource),
                new object[]
                {
                        new I18nContext{ Culture = CultureInfo.CurrentCulture },
                        _i18NResourceFactory
                }) as IStringLocalizer;
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            throw new NotImplementedException();
        }
    }
}
