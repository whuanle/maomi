using System.CommandLine;

namespace Maomi.Curl
{
	internal class Program
	{
		static async Task<int> Main(string[] args)
		{
			// 定义命令参数
			// http header
			var headers = new Option<Dictionary<string, string>?>(
				name: "-H",
				description: "header,ex: -H \"Accept-Language=zh-CN\".",
				parseArgument: result =>
				{
					var dic = new Dictionary<string, string>();
					if (result.Tokens.Count == 0) return dic;

					foreach (var item in result.Tokens)
					{
						var header = item.Value.Split("=");
						dic.Add(header[0], header[1]);
					}
					return dic;
				})
			{
				// 可以出现 0 或多次
				Arity = ArgumentArity.ZeroOrMore,
			};

			var cookie = new Option<string?>(
				name: "-b",
				description: "cookie.")
			{
				Arity = ArgumentArity.ZeroOrOne
			};

			var body = new Option<string?>(
				name: "-d",
				description: "post body.")
			{
				Arity = ArgumentArity.ZeroOrOne
			};

			var httpMethod = new Option<string?>(
				name: "-X",
				description: "GET/POST ...",
				getDefaultValue: () => "GET")
			{
				Arity = ArgumentArity.ZeroOrOne
			};

			// 其它无名的参数
			var otherArgument = new Argument<string>();

			// 构建命令行参数
			var rootCommand = new RootCommand("输入参数请求 url 地址");
			rootCommand.AddOption(headers);
			rootCommand.AddOption(cookie);
			rootCommand.AddOption(body);
			rootCommand.AddOption(httpMethod);
			rootCommand.Add(otherArgument);

			// 解析参数调用
			rootCommand.SetHandler(async (headers, cookie, body, httpMethod, otherArgument) =>
			{
				Console.WriteLine($"request: {otherArgument}");

				if (headers == null) headers = new Dictionary<string, string>();

				try
				{
					if (!string.IsNullOrEmpty(body) ||
					"POST".Equals(httpMethod, StringComparison.InvariantCultureIgnoreCase))
					{
						ArgumentNullException.ThrowIfNull(body);
						await PostAsync(otherArgument, headers, body, cookie);
					}
					else
					{
						await GetAsync(otherArgument, headers, cookie);
					}
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(ex.Message);
					Console.ResetColor();
				}

			}, headers, cookie, body, httpMethod, otherArgument);
			return await rootCommand.InvokeAsync(args);
		}

		private static async Task GetAsync(string url, IReadOnlyDictionary<string, string> headers, string? cookie = null)
		{
			var client = new HttpClient();
			BuildHeader(headers, cookie, client);

			var response = await client.GetAsync(new Uri(url));
			Console.WriteLine(await response.Content.ReadAsStringAsync());
		}

		private static async Task PostAsync(string url, IReadOnlyDictionary<string, string> headers, string body, string? cookie = null)
		{
			var client = new HttpClient();
			BuildHeader(headers, cookie, client);

			var jsonContent = new StringContent(body);
			jsonContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

			var response = await client.PostAsync(new Uri(url), jsonContent);
			Console.WriteLine(await response.Content.ReadAsStringAsync());
		}

		private static void BuildHeader(IReadOnlyDictionary<string, string> headers, string? cookie, HttpClient client)
		{
			if (headers != null && headers.Count > 0)
			{
				foreach (var item in headers)
					client.DefaultRequestHeaders.Add(item.Key, item.Value);
			}
			if (!string.IsNullOrEmpty(cookie))
			{
				client.DefaultRequestHeaders.Add("Cookie", cookie);
			}
		}
	}
}
