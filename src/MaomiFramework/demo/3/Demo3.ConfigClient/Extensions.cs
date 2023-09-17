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
        /// <returns></returns>
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
}
