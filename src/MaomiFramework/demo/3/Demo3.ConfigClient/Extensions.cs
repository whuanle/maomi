using Microsoft.Extensions.Configuration;

namespace Demo3.ConfigClient
{
    public static class Extensions
    {
        /// <summary>
        /// 添加远程配置
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="url"></param>
        /// <param name="appName"></param>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddReomteConfig(this IConfigurationBuilder builder, string url, string appName, string @namespace)
        {
            var source = new OnlineConfigurationSource()
            {
                URL = url,
                AppName = appName,
                Namespace = @namespace
            };
            builder.Add(source);
            return builder;
        }
    }
}
