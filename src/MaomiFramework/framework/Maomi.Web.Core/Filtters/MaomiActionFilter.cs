using Maomi.Module;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace Maomi.Web.Core.Filters
{
    /// <summary>
    /// Action 过滤器
    /// </summary>
    [InjectOn(Scheme = InjectScheme.None, Own = true)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MaomiActionFilter : ActionFilterAttribute
    {
        private readonly IStringLocalizer<MaomiActionFilter> _localizer;
        public MaomiActionFilter(IStringLocalizer<MaomiActionFilter> stringLocalizer)
        {
            _localizer = stringLocalizer;
        }


        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                Dictionary<string, List<string>> errors = new();
                foreach (var item in context.ModelState)
                {
                    List<string> list = new();
                    foreach (var error in item.Value.Errors)
                    {
                        list.Add(error.ErrorMessage);
                    }
                    errors.Add(item.Key, list);
                }
                context.Result = new BadRequestObjectResult(R.Create(400, _localizer["400"], errors));
            }
        }
    }
}
