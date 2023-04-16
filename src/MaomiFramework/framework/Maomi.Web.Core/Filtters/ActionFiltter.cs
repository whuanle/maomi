using Maomi.I18n;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using System.Net;

namespace Maomi.Web.Core.Filtters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception,
                """
                Request URL {1}
                """
                , context.HttpContext.Request.GetDisplayUrl());
        }
    }

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
