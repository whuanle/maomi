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

        internal RedisI18nResource(RedisClient redisClient, string pathPrefix)
        {
            _redisClient = redisClient;
            _pathPrefix = pathPrefix;
            redisClient.UseClientSideCaching(new ClientSideCachingOptions
            {
                Capacity = capacity,
                KeyFilter = key => key.StartsWith(pathPrefix),
                CheckExpired = (key, dt) => DateTime.Now.Subtract(dt) > expired
            });
        }

        // aaa:zh_CN => new CultureInfo("zh_CN")
        public IReadOnlyList<CultureInfo> SupportedCultures => _databaseHook
            .Keys(_pathPrefix)
            .Select(x => new CultureInfo(x.Remove(0, _pathPrefix.Length + 1))).ToList();

        public IReadOnlyList<CultureInfo> SupportedUICultures => _databaseHook
            .Keys(_pathPrefix)
            .Select(x => new CultureInfo(x.Remove(0, _pathPrefix.Length + 1))).ToList();

        public LocalizedString Get(string culture, string name)
        {
            var key = $"{_pathPrefix}:{culture}";
            var value = _databaseHook.HGet<string>(key, name);
            if (string.IsNullOrEmpty(value)) return new LocalizedString(name, name, resourceNotFound: true);
            return new LocalizedString(name, value);
        }

        public LocalizedString Get(string culture, string name, params object[] arguments)
        {
            var key = $"{_pathPrefix}:{culture}";
            var value = _databaseHook.HGet<string>(key, name);
            if (string.IsNullOrEmpty(value)) return new LocalizedString(name, name, resourceNotFound: true);
            var v = string.Format(value, arguments);
            return new LocalizedString(name, v);
        }

        public LocalizedString Get<T>(string culture, string name)
        {
            var key = $"{_pathPrefix}:{culture}";
            var value = _databaseHook.HGet<string>(key, name);
            if (string.IsNullOrEmpty(value)) return new LocalizedString(name, name, resourceNotFound: true);
            return new LocalizedString(name, value);
        }

        public LocalizedString Get<T>(string culture, string name, params object[] arguments)
        {
            var key = $"{_pathPrefix}:{culture}";
            var value = _databaseHook.HGet<string>(key, name);
            if (string.IsNullOrEmpty(value)) return new LocalizedString(name, name, resourceNotFound: true);
            var v = string.Format(value, arguments);
            return new LocalizedString(name, v);
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var keys = _databaseHook.Keys(_pathPrefix);
            foreach (var key in keys)
            {
                var vs = _databaseHook.HGetAll<string>(key);
                foreach (var item in vs)
                {
                    yield return new LocalizedString(item.Key, item.Value);
                }
            }
        }
    }
}
