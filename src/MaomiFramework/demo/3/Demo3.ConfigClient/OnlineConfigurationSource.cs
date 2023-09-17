using Microsoft.Extensions.Configuration;

namespace Demo3.ConfigClient
{
    public class OnlineConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// 获取最新配置的 API 路径
        /// </summary>
        public string URL { get; init; }
        public string AppName { get; init; }
        public string Namespace { get; init; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new OnlineConfigurationProvider(this, builder);
        }
    }
}
