using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Maomi.I18n
{
	public static class I18nExtensions
	{
		/// <summary>
		/// 添加 i18n 支持服务
		/// </summary>
		/// <param name="services"></param>
		/// <param name="isResource"></param>
		/// <param name="basePath"></param>
		/// <param name="defaultLanguage"></param>
		public static void AddI18n(this IServiceCollection services, bool isResource, string basePath, string defaultLanguage = "zh-CN")
		{
			var defaultCulture = new CultureInfo(defaultLanguage);
			List<CultureInfo> supportedCultures = new() { defaultCulture };
			if (isResource)
			{
				// 从 .dll 嵌入资源的读取，代码略
			}
			else
			{
				// 读取 json 文件，生成语言支持
				var files = new DirectoryInfo(basePath).GetFiles().Where(x => x.Name.EndsWith(".json"));
				foreach (var file in files)
				{
					var language = Path.GetFileNameWithoutExtension(file.Name);
					var text = File.ReadAllText(file.FullName);
					var dic = ReadJsonHelper.Read(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(text)), new JsonReaderOptions { AllowTrailingCommas = true });
					supportedCultures.Add(new CultureInfo(language));
					I18nStringLocalizerFactory.Add(language, new I18nStringLocalizer(language, dic));
				}
			}
			
			services.AddLocalization();
			services.AddSingleton<IStringLocalizerFactory, I18nStringLocalizerFactory>();
			services.AddScoped<I18nContext>();
			services.AddScoped<I18nMiddleware>(s => new I18nMiddleware(defaultCulture));
			services.AddScoped<IStringLocalizer>(s =>
			{
				var option = s.GetRequiredService<I18nContext>();
				var localizer = I18nStringLocalizerFactory.Get(option.Culture.Name);
				return localizer;
			});
			// 此配置会被注入到 app.UseRequestLocalization();
			services.Configure<RequestLocalizationOptions>(options =>
			{
				options.ApplyCurrentCultureToResponseHeaders = true;
				options.DefaultRequestCulture = new RequestCulture(culture: defaultLanguage, uiCulture: defaultLanguage);
				options.SupportedCultures = supportedCultures;
				options.SupportedUICultures = supportedCultures;
				// https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/localization/select-language-culture?view=aspnetcore-7.0
				// 默认自带了三个请求语言提供器，会先从这些提供器识别要使用的语言。
				// QueryStringRequestCultureProvider
				// CookieRequestCultureProvider
				// AcceptLanguageHeaderRequestCultureProvider
				// 自定义请求请求语言提供器
				options.RequestCultureProviders.Add(new I18nRequestCultureProvider(defaultLanguage));
			});
		}

		/// <summary>
		/// i18n 中间件
		/// </summary>
		/// <param name="app"></param>
		public static void UseI18n(this IApplicationBuilder app)
		{
			app.UseRequestLocalization();
			app.UseMiddleware<I18nMiddleware>();
		}
	}
}
