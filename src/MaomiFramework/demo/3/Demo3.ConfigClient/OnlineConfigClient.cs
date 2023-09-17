using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Text.Json.Nodes;

namespace Demo3.ConfigClient
{

    public class OnlineConfigurationProvider : IConfigurationProvider, IDisposable
    {
        private const string TmpFile = "tmp_config.json";

        private readonly OnlineConfigurationSource _configurationSource;
        private readonly JsonConfigurationSource _jsonSource;
        private readonly IConfigurationProvider _provider;

        private readonly HubConnection _connection;

        public OnlineConfigurationProvider(OnlineConfigurationSource configurationSource, IConfigurationBuilder builder)
        {
            _jsonSource = new JsonConfigurationSource()
            {
                Path = TmpFile,
                ReloadOnChange = true,
            };
            _provider = _jsonSource.Build(builder);

            var path = Directory.GetParent(typeof(OnlineConfigurationProvider).Assembly.Location).FullName;
            if (!File.Exists(TmpFile)) File.WriteAllText(Path.Combine(path, TmpFile), "{}");


            _connection = new HubConnectionBuilder()
                .WithUrl(_configurationSource.URL, options =>
                {
                    options.Headers.Add("AppName", _configurationSource.AppName);
                    options.Headers.Add("Namespace", _configurationSource.Namespace);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<JsonObject>("Publish", async (json) =>
            {
                // 每次清空文件重新写入内容
                using FileStream fs = new FileStream(TmpFile, FileMode.Truncate, FileAccess.ReadWrite);
                await System.Text.Json.JsonSerializer.SerializeAsync(fs, json);
                Console.WriteLine($"已更新配置：{System.Text.Json.JsonSerializer.Serialize(json)}");
            });

            _connection.StartAsync().Wait();
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
                    _connection.DisposeAsync();
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
