using FreeRedis;

namespace Maomi.I18n.Redis
{
	/// <summary>
	/// I18n Redis 缓存
	/// </summary>
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
            RedisClient redis,
            string pathPrefix,
            TimeSpan expired,
            int capacity = 10
            )
        {
            var resource = new RedisI18nResource(redis, pathPrefix, expired, capacity);

			resourceFactory.Add(resource);
            return resourceFactory;
        }
    }
}
