using System.Globalization;
using System.Runtime.InteropServices;

namespace Maomi.I18n
{
	/// <summary>
	/// 创建作用域
	/// </summary>
	public class CultureInfoScope : IDisposable
	{
		private readonly CultureInfo _defaultCultureInfo;

		/// <inheritdoc/>
		public CultureInfoScope(string language)
		{
			_defaultCultureInfo = CultureInfo.CurrentCulture;
			CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture(language);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			CultureInfo.CurrentCulture = _defaultCultureInfo;
		}
	}
}
