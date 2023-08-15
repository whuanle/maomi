using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Demo7.Console
{
    public interface Demo7Client
    {
        [Get("/index/name")]
        Task<string> GetAsync([Query] string name);
    }

    public static class RefitTest
    {
        public static void AddDemo7Client<TDelegatingHandler>(this IServiceCollection services, string url)
            where TDelegatingHandler : DelegatingHandler
        {
            JsonSerializerSettings j1 = new JsonSerializerSettings();
            JsonSerializerOptions j2 = new JsonSerializerOptions();

            RefitSettings r1 = new RefitSettings(new NewtonsoftJsonContentSerializer(j1));
            RefitSettings r2 = new RefitSettings(new SystemTextJsonContentSerializer(j2));

            services.AddScoped<TDelegatingHandler>();

            var httpBuilder = services.AddRefitClient<Demo7Client>(r1)
                                .ConfigureHttpClient(c => c.BaseAddress = new Uri(url));

            httpBuilder
            .AddHttpMessageHandler<MyDelegatingHandler>()
            .SetHandlerLifetime(TimeSpan.FromSeconds(3));
        }
    }
}
