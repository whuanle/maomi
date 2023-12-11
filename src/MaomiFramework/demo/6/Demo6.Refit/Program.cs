using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly.Extensions.Http;
using Polly;
using Refit;
using System.Net.Http;
using System.Text.Json;

namespace Demo6.Refit
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			// # 1
			var services = new ServiceCollection();
			services.AddRefitClient<IDemo6Client>()
				.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://baidu.com"))
				.SetHandlerLifetime(TimeSpan.FromSeconds(3));

			var ioc = services.BuildServiceProvider();
			var client = ioc.GetRequiredService<IDemo6Client>();
			await client.GetAsync("test");



			// # 2
			JsonSerializerSettings j1 = new JsonSerializerSettings()
			{
				DateFormatString = "yyyy-MM-dd HH:mm:ss"
			};
			RefitSettings r1 = new RefitSettings(new NewtonsoftJsonContentSerializer(j1));

			//JsonSerializerOptions j2 = new JsonSerializerOptions();
			//RefitSettings r2 = new RefitSettings(new SystemTextJsonContentSerializer(j2));

			services.AddRefitClient<IDemo6Client>(r1)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://baidu.com"));


			// # 3
			services.AddRefitClient<IDemo6ClientDynamic>()
				.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://baidu.com"))
				.SetHandlerLifetime(TimeSpan.FromSeconds(3));

			ioc = services.BuildServiceProvider();
			var clientDynamic = ioc.GetRequiredService<IDemo6ClientDynamic>();

			clientDynamic.Client.BaseAddress = new Uri("https://baidu.com");
			await clientDynamic.GetAsync("test");


			// # 4
			services.AddRefitClient<IDemo6Client>()
				.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://baidu.com"))
				.SetHandlerLifetime(TimeSpan.FromSeconds(3))
				.AddPolicyHandler(BuildRetryPolicy());
		}

		// 构建重试策略
		static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy()
		{
			return HttpPolicyExtensions
				.HandleTransientHttpError()
				.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
				.WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
		}
	}


	public interface IDemo6Client
	{
		[Get("/index/name")]
		Task<string> GetAsync([Query] string name);
	}

	public interface IDemo6ClientDynamic
	{
		HttpClient Client { get; }

		[Get("/index/name")]
		Task<string> GetAsync([Query] string name);
	}
}
