using Maomi.I18n;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using System.Net.Http.Headers;

namespace Maomi.Web.Core.Test
{
    public class I18nTest
    {
        [Fact]
        public async Task Request_I18n()
        {
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddControllers();
                            services.AddI18n(defaultLanguage: "zh-CN");
                            services.AddI18nResource(option =>
                            {
                                var basePath = "i18n";
                                option.ParseDirectory<I18nTest>(basePath);
                            });
                        })
                        .Configure(app =>
                        {
                            app.UseI18n();
                            app.UseRouting();
                            app.Use(async (HttpContext context, RequestDelegate next) =>
                            {
                                var localizer = context.RequestServices.GetRequiredService<IStringLocalizer>();
                                await context.Response.WriteAsync(localizer["购物车:商品名称"]);
                                return;
                            });
                        });
                })
                .StartAsync();

            var httpClient = host.GetTestClient();

            httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            var response = await httpClient.GetStringAsync("/test?culture=en-US&ui-culture=en-US");
            Assert.Equal("Product name", response);

            response = await httpClient.GetStringAsync("/test?culture=zh-CN&ui-culture=zh-CN");
            Assert.Equal("商品名称", response);

            httpClient.DefaultRequestHeaders.Add("Cookie", ".AspNetCore.Culture=c=en-US|uic=en-US");
            response = await httpClient.GetStringAsync("/test");
            Assert.Equal("Product name", response);

            httpClient.DefaultRequestHeaders.Add("Cookie", ".AspNetCore.Culture=c=zh-CN|uic=zh-CN");
            response = await httpClient.GetStringAsync("/test");
            Assert.Equal("商品名称", response);
            httpClient.DefaultRequestHeaders.Remove("Cookie");

            httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh-CN"));
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh", 0.9));
            response = await httpClient.GetStringAsync("/test");
            Assert.Equal("商品名称", response);

            httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 0.9));
            response = await httpClient.GetStringAsync("/test");
            Assert.Equal("Product name", response);

            httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("sv"));
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US", 0.9));
            response = await httpClient.GetStringAsync("/test");
            Assert.Equal("Product name", response);

        }
    }
}