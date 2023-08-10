using Microsoft.AspNetCore.Localization;

namespace Maomi.I18n
{
    /// <summary>
    /// 自定义如何从请求中解析请求语言
    /// </summary>
    public class I18nRequestCultureProvider : RequestCultureProvider
	{
		private readonly string _defaultLanguage;
		public I18nRequestCultureProvider(string defaultLanguage)
		{
			_defaultLanguage = defaultLanguage;
		}

		private const string RouteValueKey = "c";
		private const string UIRouteValueKey = "uic";
		public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
		{
			var request = httpContext.Request;
			if (!request.RouteValues.Any())
			{
				return NullProviderCultureResult;
			}

			string? queryCulture = null;
			string? queryUICulture = null;

			// 从路由中解析
			if (!string.IsNullOrWhiteSpace(RouteValueKey))
			{
				queryCulture = request.RouteValues[RouteValueKey]?.ToString();
			}

			if (!string.IsNullOrWhiteSpace(UIRouteValueKey))
			{
				queryUICulture = request.RouteValues[UIRouteValueKey]?.ToString() ?? queryCulture;
			}

			if (queryCulture == null && queryUICulture == null)
			{
				return NullProviderCultureResult;
			}

			var providerResultCulture = new ProviderCultureResult(queryCulture, queryUICulture);

			return Task.FromResult<ProviderCultureResult?>(providerResultCulture);
		}
	}
}
