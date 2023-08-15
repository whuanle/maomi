using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.Http;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo7.Console
{
    public static class PollyTest
    {
        public static void Test()
        {
            var services = new ServiceCollection();
            services.AddHttpClient("Default", client =>
            {
                client.BaseAddress = new Uri("http://127.0.0.1:5000");
            })
                .AddPolicyHandler(GetRetryPolicy())
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        public interface ICatalogService
        {
            Task<string> Get(int id);
        }
        public class CatalogService : ICatalogService
        {
            private readonly HttpClient _httpClient;

            public CatalogService(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }
            public async Task<string> Get(int id)
            {
                var responseMessage = await _httpClient.GetAsync("https://www.whuanle.cn");
                return await responseMessage.Content.ReadAsStringAsync();
            }
        }
    }
}
