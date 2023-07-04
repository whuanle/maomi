using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace Maomi.Web.Core.Filtters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ActionFiltter : ActionFilterAttribute, IActionFilter
    {
        private readonly IStringLocalizer<ActionFiltter> _localizer;
        public ActionFiltter(IStringLocalizer<ActionFiltter> stringLocalizer)
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
            context.Result = new JsonResult(R.C(400, _localizer["400"], errors));
        }
    }
}
