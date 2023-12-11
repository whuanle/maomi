using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.Http;
using Polly;

namespace Demo6.Polly
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();

			var policy = BuildRetryPolicy();
			services.AddHttpClient("Default", client =>
			{
				client.BaseAddress = new Uri("http://localhost:5000");
			})
				// 设置请求策略
				.AddPolicyHandler(policy)
				// 设置超时时间
				.SetHandlerLifetime(TimeSpan.FromMinutes(5));
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
}
