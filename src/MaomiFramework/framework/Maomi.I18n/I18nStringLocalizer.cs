using Microsoft.Extensions.Localization;

namespace Maomi.I18n
{
	public class I18nStringLocalizer : IStringLocalizer
	{
		private readonly string _language;
		public string Language => _language;
		private readonly IReadOnlyDictionary<string, LocalizedString> _kvs;
		public I18nStringLocalizer(string language, IReadOnlyDictionary<string, object> kvs)
		{
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
}
