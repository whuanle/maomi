using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;

namespace Maomi.I18n
{
	//
	public class I18nStringLocalizerFactory : IStringLocalizerFactory
	{
		#region 静态
		private static readonly ConcurrentDictionary<string, IStringLocalizer> Cache = new();
		public static void Add(string language, IStringLocalizer localizer)
		{
			Cache[language] = localizer;
		}
		public static IStringLocalizer Get(string location) => Cache.GetValueOrDefault(location);
		#endregion


		public IStringLocalizer Create(Type resourceSource)
		{
			return null;
		}

		public IStringLocalizer Create(string baseName, string location) => Cache.GetValueOrDefault(location);
	}
}
