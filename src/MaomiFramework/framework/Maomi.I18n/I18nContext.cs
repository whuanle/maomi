using System.Globalization;

namespace Maomi.I18n
{
    /// <summary>
    /// 记录当前请求的 i18n 信息
    /// </summary>
    public class I18nContext
    {
        /// <summary>
        /// 当前用户请求的语言
        /// </summary>
		public CultureInfo Culture { get; internal set; }
    }
}
