using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Text;
using static Program;

public static class Extensions
{
	public static IConfigurationBuilder AddEnvFile(this IConfigurationBuilder builder, string path, bool reloadOnChange = false)
	{
		var source = new MyConfigurationSource()
		{
			Path = path,
			ReloadOnChange = reloadOnChange
		};
		builder.Add(source);
		return builder;
	}
}
public class MyConfigurationSource : IConfigurationSource
{
	/// <summary>
	/// 配置文件路径
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// 是否实时监听文件变化
	/// </summary>
	public bool ReloadOnChange { get; set; }
	public IConfigurationProvider Build(IConfigurationBuilder builder)
	{
		return new MyConfigurationProvider(this);
	}
}

public class MyConfigurationProvider : IConfigurationProvider
{
	private readonly MyConfigurationSource _source;
	private readonly IFileProvider _fileProvider;

	private readonly string _path;
	private readonly string _fileName;

	private readonly Dictionary<string, string> _cache;
	public MyConfigurationProvider(MyConfigurationSource source)
	{
		_source = source;
		_cache = new Dictionary<string, string>();

		_path = Directory.GetParent(_source.Path)!.FullName;
		_fileName = Path.GetFileName(_source.Path);

		_fileProvider = new PhysicalFileProvider(_path);
		if (_source.ReloadOnChange)
		{
			// 监听配置文件变化
			ChangeToken.OnChange(() => _fileProvider.Watch(_fileName), async () => await ReloadFileAsync());
		}
		else
		{
			ReloadFileAsync().Wait();
		}
	}

	// 重新加载配置文件到内存中
	private async Task ReloadFileAsync()
	{
		using var stream = _fileProvider.GetFileInfo(_fileName).CreateReadStream();
		using var streamReader = new StreamReader(stream);
		_cache.Clear();
		while (true)
		{
			var line = await streamReader.ReadLineAsync();
			if (line == null) break;
			var kv = line.Split(':')[0..2].Select(x => x.Trim(' ')).ToArray();
			_cache.Add(kv[0], kv[1]);
		}
	}

	public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath) => _cache.Keys;
	public IChangeToken GetReloadToken() => null;

	public void Load()
	{
		ReloadFileAsync().Wait();
	}

	public void Set(string key, string? value)
	{
		_cache[key] = value!;
		// 还可以保存到文件中
	}

	public bool TryGet(string key, out string? value)
	{
		return _cache.TryGetValue(key, out value);
	}
}

public class Program
{

	static void Main()
	{
		var configuration = new ConfigurationBuilder()
			.AddEnvFile("env.conf", true)
			.Build();
		while (true)
		{
			var value = configuration["A"];
			Console.WriteLine($"A = {value}");
			Thread.Sleep(1000);
		}
	}
}