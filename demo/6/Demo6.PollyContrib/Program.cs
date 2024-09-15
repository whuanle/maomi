using Polly.Contrib.WaitAndRetry;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.Net;
using System.Net.Http;

namespace Demo6.PollyContrib
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddHttpClient();
			services.AddScoped<MyService>();
			var ioc = services.BuildServiceProvider();
			var myService = ioc.GetRequiredService<MyService>();
			await myService.GetAsync();
		}



	}

	public class MyService
	{
		private readonly HttpClient _httpClient;

		public MyService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<string> GetAsync()
		{
			var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

			var retryPolicy = Policy
				.Handle<HttpRequestException>(ex =>
				{
					// 请求时出现这几种情况，允许重试
					if (ex.StatusCode == HttpStatusCode.BadGateway ||
					ex.StatusCode == HttpStatusCode.GatewayTimeout ||
					ex.StatusCode == HttpStatusCode.ServiceUnavailable)
						return true;

					return false;
				})
				// 其它方面的异常捕获
				.WaitAndRetryAsync(delay);

			var result = await retryPolicy.ExecuteAsync<string>(async () =>
			{
				var responseMessage = await _httpClient.GetAsync("https://www.baidu.com");
				return await responseMessage.Content.ReadAsStringAsync();
			});
			return result;
		}
	}
}
