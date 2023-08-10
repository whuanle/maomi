using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Maomi.I18n
{
    /// <summary>
    /// i18n 资源管理
    /// </summary>
    public interface I18nResource
    {
        /// <summary>
        /// 该资源提供的语言
        /// </summary>
        IReadOnlyList<CultureInfo> SupportedCultures { get; }

        /// <summary>
        /// 该资源提供的 UI 语言
        /// </summary>
        IReadOnlyList<CultureInfo> SupportedUICultures { get; }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        LocalizedString Get(string culture, string name);

        /// <summary>
        /// 获取值，支持字符串插值
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        LocalizedString Get(string culture, string name, params object[] arguments);

        /// <summary>
        /// 获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="culture"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        LocalizedString Get<T>(string culture, string name);

        /// <summary>
        /// 支持字符串插值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="culture"></param>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        LocalizedString Get<T>(string culture, string name, params object[] arguments);

        /// <summary>
        /// 获取全部字符串
        /// </summary>
        /// <param name="includeParentCultures"></param>
        /// <returns></returns>
        IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures);
    }
}
