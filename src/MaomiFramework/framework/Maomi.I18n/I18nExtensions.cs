using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Maomi.I18n
{
	public static class I18nExtensions
	{
		/// <summary>
		/// 添加 i18n 资源
		/// </summary>
		/// <param name="services"></param>
		/// <param name="resourceFactory"></param>
		public static void AddI18nResource(this IServiceCollection services, Action<I18nResourceFactory> resourceFactory)
		{
			var service = services.BuildServiceProvider().GetRequiredService<I18nResourceFactory>();
			resourceFactory.Invoke(service);
        }

		/// <summary>
		/// 添加 i18n 支持服务
		/// </summary>
		/// <param name="services"></param>
		/// <param name="defaultLanguage"></param>
		public static void AddI18n(this IServiceCollection services,
			string defaultLanguage = "zh-CN")
        {
            InternalI18nResourceFactory resourceFactory = new InternalI18nResourceFactory();

			// ASP.NET Core 自带的
			services.AddLocalization();

            // 配置 ASP.NET Core 的本地化服务
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.ApplyCurrentCultureToResponseHeaders = true;
                options.DefaultRequestCulture = new RequestCulture(culture: defaultLanguage, uiCulture: defaultLanguage);
                options.SupportedCultures = resourceFactory.SupportedCultures;
                options.SupportedUICultures = resourceFactory.SupportedUICultures;

                // 默认自带了三个请求语言提供器，会先从这些提供器识别要使用的语言。
                // QueryStringRequestCultureProvider
                // CookieRequestCultureProvider
                // AcceptLanguageHeaderRequestCultureProvider
                // 自定义请求请求语言提供器
                options.RequestCultureProviders.Add(new I18nRequestCultureProvider(defaultLanguage));
            });

            // i18n 中间件
            services.AddScoped<I18nContext>();
			services.AddScoped<I18nMiddleware>(s => new I18nMiddleware(new CultureInfo(defaultLanguage)));

			// 注入 i18n 服务
			services.AddSingleton<I18nResourceFactory>(s => resourceFactory);
            services.AddSingleton<IStringLocalizerFactory, I18nStringLocalizerFactory>();
            services.AddScoped<IStringLocalizer, I18nStringLocalizer>();
			services.TryAddEnumerable(new ServiceDescriptor(typeof(IStringLocalizer<>), typeof(I18nStringLocalizer<>), ServiceLifetime.Scoped));
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
