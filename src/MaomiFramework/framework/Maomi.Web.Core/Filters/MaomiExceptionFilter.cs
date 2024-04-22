using Maomi.Attributes;
using Maomi.Module;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace Maomi.Web.Core.Filters
{
    /// <summary>
    /// 统一异常处理
    /// </summary>
    [InjectOn(Scheme = InjectScheme.None, Own = true)]
    public class MaomiExceptionFilter : IAsyncExceptionFilter
    {
        protected readonly ILogger<MaomiExceptionFilter> _logger;
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
        public virtual async Task OnExceptionAsync(ExceptionContext context)
        {
            // 未经处理的异常
            if (!context.ExceptionHandled)
            {
                var action = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                if (action == null) return;

                _logger.LogError(context.Exception,
                    """
                    RequestId: {0}
                    ControllerName: {1}
                    ActionName: {2}
                    """,
                    context.HttpContext.TraceIdentifier,
                    action?.ControllerName,
                    action?.ActionName
                    );

                Res response;

                var exceptionMessage = action.EndpointMetadata.OfType<ExceptionMessageAttribute>().FirstOrDefault();
                if (exceptionMessage != null)
                {
                    response = new Res()
                    {
                        Code = 500,
                        Msg = _stringLocalizer[exceptionMessage.Message],
                        Data = new
                        {
                            TraceIdentifier = (string)_stringLocalizer["500", context.HttpContext.TraceIdentifier]
                        }
                    };
                }
                else
                {
                    response = new Res()
                    {
                        Code = 500,
                        Msg = _stringLocalizer["500", context.HttpContext.TraceIdentifier],
                    };
                }

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
