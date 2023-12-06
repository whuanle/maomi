using Demo6.Console;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

public partial class Program
{
    public class Test1
    {
        public Test1(IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient();
        }
    }

    public class Test2
    {
        public Test2(IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient("Default");
        }
    }
    public class Test3
    {
        public Test3(HttpClient httpClient)
        {
        }
    }
    static async Task Main()
    {
        await HttpClientHelper.Json(new JsonModel
        {
            Id = "1",
            Name = "嘻嘻"
        });
        var services = new ServiceCollection();

        // 1
        services.AddHttpClient();

        // 2
        services.AddHttpClient("Default")
            .ConfigureHttpClient(x =>
            {
                x.MaxResponseContentBufferSize = 1024;
                x.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "xxxxx");
            })
            .AddHttpMessageHandler<MyDelegatingHandler>();

        // 3
        services.AddHttpClient<Program>();
    }
}