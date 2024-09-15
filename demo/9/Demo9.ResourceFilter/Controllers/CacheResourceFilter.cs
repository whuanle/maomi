using FreeRedis;
using Maomi.Module;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace Demo9.ResourceFilter.Controllers
{
    /// <summary>
    /// ResourceFilter 过滤器
    /// </summary>
    [InjectOn(Scheme = InjectScheme.None, Own = true)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CacheResourceFilter : Attribute, IAsyncResourceFilter
    {
        private readonly IStringLocalizer<CacheResourceFilter> _localizer;
        private readonly IRedisClient _redisClient;
        public CacheResourceFilter(IStringLocalizer<CacheResourceFilter> stringLocalizer, IRedisClient redisClient)
        {
            _localizer = stringLocalizer;
            _redisClient = redisClient;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var action = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            ArgumentNullException.ThrowIfNull(action);
            var key = $"{action.ControllerName}:{action.ActionName}";
            var methodInfo = action.MethodInfo;
            var returnResult = methodInfo.ReturnType;
            if (returnResult.GetGenericTypeDefinition() == typeof(Task<>))
            {
                returnResult = returnResult.GetGenericArguments()[0];
            }

            // 如果有缓存
            var text = await _redisClient.GetAsync<string>(key);
            if (!string.IsNullOrEmpty(text))
            {
                var value = System.Text.Json.JsonSerializer.Deserialize(text, returnResult);
                context.Result = new JsonResult(value);
                return;
            }

            var newContext = await next();

            // 记录到缓存中，下次直接从缓存中取出
            var result = newContext.Result as ObjectResult;
            if (result != null)
            {
                await _redisClient.SetAsync(key, result.Value, timeoutSeconds: 10);
            }
        }
    }
}