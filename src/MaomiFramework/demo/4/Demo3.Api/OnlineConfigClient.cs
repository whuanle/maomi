using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Demo3.Api
{
	public static class Extensions
	{
		public static IConfigurationBuilder AddReomteConfig(this IConfigurationBuilder builder, string url)
		{
			var source = new OnlineConfigurationSource()
			{
				URL = url,
			};
			builder.Add(source);
			return builder;
		}
	}

	public class OnlineConfigurationSource : IConfigurationSource
	{
		/// <summary>
		/// 获取最新配置的 API 路径
		/// </summary>
		public string URL { get; set; }
		public IConfigurationProvider Build(IConfigurationBuilder builder)
		{
			return new OnlineConfigurationProvider(URL, builder);
		}
	}

	public class OnlineConfigurationProvider : IConfigurationProvider, IDisposable
	{
		private const string TmpFile = "tmp_config.json";

		private readonly string _url;
		private readonly JsonConfigurationSource _jsonSource;
		private readonly IConfigurationProvider _provider;
		private readonly Timer _timer;
		private readonly HttpClient _httpClient;
		public OnlineConfigurationProvider(string url, IConfigurationBuilder builder)
		{
			_url = url;

			_jsonSource = new JsonConfigurationSource()
			{
				Path = TmpFile,
				ReloadOnChange = true,
			};
			_provider = _jsonSource.Build(builder);

			if (!File.Exists(TmpFile)) File.WriteAllText(TmpFile, "{}");
			_httpClient = new();
			_timer = new Timer(async _ => await Timing(), null, 0, period: 1000);
		}


		private async Task Timing()
		{
			Console.WriteLine("检测远程更新");

			try
			{
				var response = await _httpClient.GetAsync(_url);
				if (!response.IsSuccessStatusCode) return;
				var reomteStream = response.Content.ReadAsStream();

				// 每次清空文件重新写入内容
				using FileStream fs = new FileStream(TmpFile, FileMode.Truncate, FileAccess.ReadWrite);
				await reomteStream.CopyToAsync(fs);
				Console.WriteLine(response.Content.ReadAsStringAsync().Result);
			}
			catch { }
		}

		private bool _disposedValue;
		~OnlineConfigurationProvider() => Dispose(false);
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_httpClient.Dispose();
				}
				_disposedValue = true;
			}
		}

		public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath) => _provider.GetChildKeys(earlierKeys, parentPath);
		public IChangeToken GetReloadToken() => _provider.GetReloadToken();
		public void Load() => _provider.Load();
		public void Set(string key, string? value) => _provider.Set(key, value);
		public bool TryGet(string key, out string? value) => _provider.TryGet(key, out value);
	}
}
