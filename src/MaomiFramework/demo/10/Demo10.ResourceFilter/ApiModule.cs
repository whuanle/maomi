using Maomi.Module;
using Maomi.I18n;
using Maomi.Web.Core;
using Demo10.ResourceFilter.Controllers;
using FreeRedis;

namespace Demo10.ResourceFilter
{
    [InjectModule<MaomiWebModule>()]
    public class ApiModule : IModule
    {
        public void ConfigureServices(ServiceContext context)
        {
            context.Services.AddControllers(options =>
            {
                // 这里不会跟 MaomiWebModule 中的筛选器冲突，两者的筛选器会被合起来一起使用
                options.Filters.AddService<CacheResourceFilter>();
            });

            // 配置 FreeRedis
            RedisClient redis = new RedisClient("127.0.0.1:6379,defaultDatabase=0");
            redis.Serialize = obj => System.Text.Json.JsonSerializer.Serialize(obj);
            redis.Deserialize = (json, type) => System.Text.Json.JsonSerializer.Deserialize(json, type);

            context.Services.AddSingleton<IRedisClient, RedisClient>(s => redis);
        }
    }
}
