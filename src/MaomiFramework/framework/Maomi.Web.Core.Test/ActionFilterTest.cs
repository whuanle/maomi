using Maomi.I18n;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Maomi.Web.Core.Test
{
    public class ActionFilterTest
    {
        // 测试在中英文情况下，模型验证失败时返回统一格式
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
                                option.AddJson<I18nTest>(basePath);
                            });
                        })
                        .Configure(app =>
                        {
                            app.UseI18n();
                            app.UseRouting();
                            app.UseEndpoints(configure =>
                            {
								configure.MapGet("/test",() =>
								{
									return "test";
								});
							});
                        });
                })
                .StartAsync();



            var httpClient = host.GetTestClient();
            httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            var response = await httpClient.GetStringAsync("/test");
            Assert.Equal("Product name", response);

            httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
			response = await httpClient.GetStringAsync("/test?culture=en-US&ui-culture=en-US");
            Assert.Equal("Product name", response);

			response = await httpClient.GetStringAsync("/test?culture=zh-CN&ui-culture=zh-CN");
            Assert.Equal("商品名称", response);
        }
    }
}
