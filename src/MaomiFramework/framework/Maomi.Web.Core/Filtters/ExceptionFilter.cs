using Microsoft.AspNetCore.Mvc.Filters;

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
            //_logger.LogError(context.Exception,
            //    """
            //    Request URL {1}
            //    """
            //    , context.HttpContext.Request.GetDisplayUrl());
        }
    }

}
