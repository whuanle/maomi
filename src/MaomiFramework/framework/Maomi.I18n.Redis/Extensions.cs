using FreeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Maomi.I18n.Redis
{
    public static class Extensions
    {
        /// <summary>
        /// 添加 i18n redis 资源
        /// </summary>
        /// <param name="resourceFactory"></param>
        /// <param name="redis"></param>
        /// <param name="pathPrefix">key前缀</param>
        /// <param name="expired">本地缓存有效期</param>
        /// <param name="capacity">缓存的 key 数量</param>
        public static I18nResourceFactory AddRedis(this I18nResourceFactory resourceFactory,
            RedisClient.DatabaseHook redis,
            string pathPrefix,
            TimeSpan expired,
            int capacity = 10
            )
        {
            redis.UseClientSideCaching(new ClientSideCachingOptions
            {
                Capacity = capacity,
                KeyFilter = key => key.StartsWith(pathPrefix),
                CheckExpired = (key, dt) => DateTime.Now.Subtract(dt) > expired
            });

            var keys = redis.Keys(pathPrefix);
            resourceFactory.Add(new RedisI18nResource(redis, pathPrefix));
            return resourceFactory;
        }
    }
}
