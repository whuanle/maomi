using FreeRedis;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maomi.I18n.Redis
{
    /// <summary>
    /// i18n redis 资源
    /// </summary>
    public class RedisI18nResource : I18nResource
    {
        private readonly RedisClient _redisClient;
        private readonly string _pathPrefix;

        internal RedisI18nResource(RedisClient redisClient, string pathPrefix, TimeSpan expired, int capacity = 10)
        {
            _redisClient = redisClient;
            _pathPrefix = pathPrefix;

            // Redis client-side 模式
            redisClient.UseClientSideCaching(new ClientSideCachingOptions
            {
                Capacity = capacity,
                KeyFilter = key => key.StartsWith(pathPrefix),
                CheckExpired = (key, dt) => DateTime.Now.Subtract(dt) > expired
            });

			// FreeRedis 的 client-side 模式，使用 Hash 类型时，
			// 第一次需要先 HGetAll() ，框架将缓存拉取到本地
			GetAllStrings(default);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IReadOnlyList<CultureInfo> SupportedCultures => _redisClient
			.Keys(_pathPrefix)
            .Select(x => new CultureInfo(x.Remove(0, _pathPrefix.Length + 1))).ToList();

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public IReadOnlyList<CultureInfo> SupportedUICultures => _redisClient
			.Keys(_pathPrefix)
            .Select(x => new CultureInfo(x.Remove(0, _pathPrefix.Length + 1))).ToList();

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public LocalizedString Get(string culture, string name)
        {
            var key = $"{_pathPrefix}:{culture}";
            var value = _redisClient.HGet<string>(key, name);
            if (string.IsNullOrEmpty(value)) return new LocalizedString(name, name, resourceNotFound: true);
            return new LocalizedString(name, value);
        }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public LocalizedString Get(string culture, string name, params object[] arguments)
        {
            var key = $"{_pathPrefix}:{culture}";
            var value = _redisClient.HGet<string>(key, name);
            if (string.IsNullOrEmpty(value)) return new LocalizedString(name, name, resourceNotFound: true);
            var v = string.Format(value, arguments);
            return new LocalizedString(name, v);
        }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public LocalizedString Get<T>(string culture, string name)
        {
            var key = $"{_pathPrefix}:{culture}";
            var value = _redisClient.HGet<string>(key, name);
            if (string.IsNullOrEmpty(value)) return new LocalizedString(name, name, resourceNotFound: true);
            return new LocalizedString(name, value);
        }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public LocalizedString Get<T>(string culture, string name, params object[] arguments)
        {
            var key = $"{_pathPrefix}:{culture}";
            var value = _redisClient.HGet<string>(key, name);
            if (string.IsNullOrEmpty(value)) return new LocalizedString(name, name, resourceNotFound: true);
            var v = string.Format(value, arguments);
            return new LocalizedString(name, v);
        }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var keys = _redisClient.Keys(_pathPrefix);
            foreach (var key in keys)
            {
                var vs = _redisClient.HGetAll<string>(key);
                foreach (var item in vs)
                {
                    yield return new LocalizedString(item.Key, item.Value);
                }
            }
        }
    }
}
