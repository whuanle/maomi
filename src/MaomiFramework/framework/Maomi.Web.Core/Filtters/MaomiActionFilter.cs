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
            context.Result = new JsonResult(R.Create(400, _localizer["400"], errors));
        }
    }


    /// <summary>
    /// ResourceFilter 过滤器
    /// </summary>
    [InjectOn(Scheme = InjectScheme.None, Own = true)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MaomiResourceFilter : Attribute, IResourceFilter, IAsyncResourceFilter
    {
        private readonly IStringLocalizer<MaomiResourceFilter> _localizer;
        public MaomiResourceFilter(IStringLocalizer<MaomiResourceFilter> stringLocalizer)
        {
            _localizer = stringLocalizer;
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            throw new NotImplementedException();
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            throw new NotImplementedException();
        }

        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}
