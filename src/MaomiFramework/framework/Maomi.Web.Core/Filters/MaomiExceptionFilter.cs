using Maomi.Module;
using Maomi.Web.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Text.Json;

namespace Maomi.Web.Core.Filtters
{
    /// <summary>
    /// 统一异常处理
    /// </summary>
    [InjectOn(Scheme = InjectScheme.None, Own = true)]
    public class MaomiExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<MaomiExceptionFilter> _logger;
        private readonly IStringLocalizer<MaomiExceptionFilter> _stringLocalizer;

        /// <summary>
        /// 统一异常处理
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="stringLocalizer"></param>
        public MaomiExceptionFilter(ILogger<MaomiExceptionFilter> logger, IStringLocalizer<MaomiExceptionFilter> stringLocalizer)
        {
            _logger = logger;
            _stringLocalizer = stringLocalizer;
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            // 未经处理的异常
            if (!context.ExceptionHandled)
            {
                var action = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;

                _logger.LogCritical(context.Exception,
                    """
                    RequestId: {0}
                    ControllerName: {1}
                    ActionName: {2}
                    """,
                    context.HttpContext.TraceIdentifier,
                    action?.ControllerName,
                    action?.ActionName
                    );
                var response = new R()
                {
                    Code = 500,
                    Msg = _stringLocalizer["500", context.HttpContext.TraceIdentifier],
                };

                context.Result = new ObjectResult(response)
                {
                    StatusCode = 500,
                };

                context.ExceptionHandled = true;
            }

            await Task.CompletedTask;
        }
    }
}
