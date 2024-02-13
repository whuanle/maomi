using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Demo6.HttpFactory
{
	internal class Program
	{
		static void Main()
		{
			var services = new ServiceCollection();
			services.AddScoped<Test1>();
			services.AddScoped<Test2>();
			services.AddScoped<Test3>();

			// 1
			services.AddHttpClient();

			// 2
			services.AddHttpClient("Default");

			// 3
			services.AddHttpClient<Program>();

			// 2
			services.AddTransient<MyDelegatingHandler>();
			services.AddHttpClient("Default")
				.ConfigureHttpClient(x =>
				{
					x.MaxResponseContentBufferSize = 1024;
					x.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "xxxxx");
				})
				.AddHttpMessageHandler<MyDelegatingHandler>();
		}
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
	}
}
