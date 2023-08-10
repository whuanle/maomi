using System.Globalization;

namespace Maomi.I18n
{
    /// <summary>
    /// I18n 资源工厂
    /// </summary>
    public interface I18nResourceFactory
    {
        /// <summary>
        /// 支持的语言
        /// </summary>
        IList<CultureInfo> SupportedCultures { get; }

        /// <summary>
        /// UI 支持的语言
        /// </summary>
        IList<CultureInfo> SupportedUICultures { get; }

        /// <summary>
        /// 所有资源提供器
        /// </summary>
        IReadOnlyList<I18nResource> Resources { get; }

        /// <summary>
        /// 添加资源提供器
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        I18nResourceFactory Add(I18nResource resource);
    }
}
