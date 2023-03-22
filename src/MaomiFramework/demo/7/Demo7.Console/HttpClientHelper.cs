using System.Net.Http.Headers;
using System.Text;

namespace Demo7.Console
{
	public static class HttpClientHelper
	{
		// basic 认证
		public static async Task<string> Basic(string url, string user, string password)
		{
			var httpclientHandler = new HttpClientHandler()
			{
				// 忽略 https 不安全等检查
				ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true,
			};
			using HttpClient client = new HttpClient(httpclientHandler);

			AuthenticationHeaderValue authentication = new AuthenticationHeaderValue(
				"Basic",
				Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}")
				));
			client.DefaultRequestHeaders.Authorization = authentication;

			var response = await client.GetAsync(url);
			return await response.Content.ReadAsStringAsync();
		}

		// jwt认证
		public static async Task<string> Jwt(string token, string url)
		{
			var httpclientHandler = new HttpClientHandler()
			{
				// 忽略 https 不安全等检查
				ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true,
			};

			using var client = new HttpClient(httpclientHandler);
			// 创建身份认证
			// System.Net.Http.Headers.AuthenticationHeaderValue;
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			var response = await client.GetAsync(url);
			return await response.Content.ReadAsStringAsync();
		}


		// 获取登录后的 HttpClient
		public static async Task<HttpClient> Cookie(string user, string password, string loginUrl)
		{
			var httpclientHandler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true,
				UseCookies = true
			};

			var loginContent = new FormUrlEncodedContent(new[]
			{
			 new KeyValuePair<string,string>("user",user),
			 new KeyValuePair<string, string>("password",password)
			});

			var httpClient = new HttpClient(httpclientHandler);
			var response = await httpClient.PostAsync(loginUrl, loginContent);
			if (response.IsSuccessStatusCode) return httpClient;
			throw new Exception($"请求失败，http 状态码：{response.StatusCode}");
		}

		// 自行设置 cookie
		public static async Task<string> Cookie(string cookie, string url)
		{
			var httpclientHandler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true,
			};

			using var client = new HttpClient(httpclientHandler);
			client.DefaultRequestHeaders.Add("Cookie", cookie);
			var response = await client.GetAsync(url);
			return await response.Content.ReadAsStringAsync();
		}

		// URL Query 参数
		public static async Task Query(string a, string b)
		{
			var httpclientHandler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true,
			};
			using var httpClient = new HttpClient(httpclientHandler);
			var response = await httpClient.GetAsync($"https://localhost:5001/test?a={a}&b={b}");
		}


		// Header 头
		public static async Task Header()
		{
			var httpclientHandler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true,
			};
			using var httpClient = new HttpClient(httpclientHandler);
			httpClient.DefaultRequestHeaders.Add("MyEmail", "123@qq.com");
			var response = await httpClient.GetAsync($"https://localhost:5179/Index");
			var result = await response.Content.ReadAsStringAsync();
		}

		// 表单提交
		// application/x-www-form-urlencoded
		public static async Task From()
		{
			var httpclientHandler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true,
			};

			var fromContent = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string,string>("Id","1"),
				new KeyValuePair<string,string>("Name","痴者工良"),
				new KeyValuePair<string, string>("Number","666666")
			});

			using var httpClient = new HttpClient(httpclientHandler);
			var response = await httpClient.PostAsync("https://localhost:5179/test", fromContent);
		}

		// 上传文件
		public static async Task SendFile(string filePath, string fromName, string url)
		{
			using var client = new HttpClient();

			FileStream imagestream = System.IO.File.OpenRead(filePath);
			// multipartFormDataContent.Add( ... ...);
			var multipartFormDataContent = new MultipartFormDataContent()
				{
					{
						new StreamContent(File.OpenRead(filePath)),
						// 对应 服务器 WebAPI 的传入参数
                        fromName,
						// 上传的文件名称
                        Path.GetFileName(filePath)
					},
					// 可上传多个文件
				};

			HttpResponseMessage response = await client.PostAsync(url, multipartFormDataContent);
		}

		// Json 等
		public static async Task Json(string json)
		{
			var jsonContent = new StringContent(json);

			// Json 是 StringContent，上传时要指定 Content-Type 属性，除此外还有
			// text/html
			// application/javascript
			// text/plain
			// application/xml
			jsonContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

			using var httpClient = new HttpClient();
			var response = httpClient.PostAsync("https://localhost:5001/test", jsonContent).Result;
			var result = await response.Content.ReadAsStringAsync();
		}
	}
}
