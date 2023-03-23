using Microsoft.Extensions.Localization;
using System.Reflection;

namespace Maomi.I18n
{
	public class I18nStringLocalizer : IStringLocalizer
	{
		protected readonly string _path;
		public string Path => _path;
		protected readonly string _language;
		public string Language => _language;
		private readonly IReadOnlyDictionary<string, LocalizedString> _kvs;

		public I18nStringLocalizer(string path, string language, IReadOnlyDictionary<string, object> kvs)
		{
			_path = path;
			_language = language;
			_kvs = kvs.ToDictionary(x => x.Key, x => new LocalizedString(x.Key, x.Value.ToString()));
		}

		// 如果查找不到 key，不要返回 null，可以返回 new LocalizedString(name, null)
		public LocalizedString this[string name] => _kvs.GetValueOrDefault(name) ?? new LocalizedString(name, null);

		// 支持字符串插值
		public LocalizedString this[string name, params object[] arguments]
		{
			get
			{
				var actualValue = this[name];
				if (actualValue.ResourceNotFound) return new LocalizedString(name, string.Format(actualValue.Value, arguments), false);
				return actualValue;
			}
		}

		public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => _kvs.Values;
	}

	public class I18nStringLocalizer<TType> : IStringLocalizer<TType>
	{
		private readonly IStringLocalizer _localizer;
		public I18nStringLocalizer(I18nContext context)
		{
			var localtion = $"{typeof(TType).Assembly.GetName().Name}.{context.Culture.Name}";
			var localizer = I18nStringLocalizerFactory.Get(localtion);
			_localizer = localizer;
		}

		public LocalizedString this[string name] => _localizer[name];
		public LocalizedString this[string name, params object[] arguments] => _localizer[name, arguments];
		public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => _localizer.GetAllStrings(includeParentCultures);
	}
}
