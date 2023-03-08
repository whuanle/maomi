using System.Globalization;

namespace Maomi.I18n
{
	// 记录当前请求的 i18n 信息
	public class I18nContext
	{
		public CultureInfo Culture { get; set; }
	}
}
