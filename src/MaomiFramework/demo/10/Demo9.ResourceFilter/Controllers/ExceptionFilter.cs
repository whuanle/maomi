using Maomi.Web.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Text.Json;

namespace Demo10.ExceptionFilter
{
    public class ExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
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
                _logger.LogCritical(context.Exception, context.ActionDescriptor.DisplayName);
                var response = new R()
                {
                    Code = 500,
                    Msg = ,
                };

                context.Result = new ObjectResult(response)
                {
                    StatusCode = (int)HttpStateCode.InternalServerError,
       
                };

                context.ExceptionHandled = true;
            }
            else
            {
                _logger.LogError(context.Exception, context.ActionDescriptor.DisplayName);
            }

            await Task.CompletedTask;
        }
    }

}
