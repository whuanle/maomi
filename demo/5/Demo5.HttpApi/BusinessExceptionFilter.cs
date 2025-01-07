using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace Demo5.HttpApi;

/// <summary>
/// 统一异常处理.
/// </summary>
public class BusinessExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<BusinessExceptionFilter> _logger;
    private readonly IStringLocalizer _stringLocalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessExceptionFilter"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="stringLocalizer"></param>
    public BusinessExceptionFilter(ILogger<BusinessExceptionFilter> logger, IStringLocalizer stringLocalizer)
    {
        _logger = logger;
        _stringLocalizer = stringLocalizer;
    }

    /// <inheritdoc/>
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        // 未被处理的异常
        if (!context.ExceptionHandled)
        {
            object? response = default;

            // 如果抛出的是业务异常，转换为对应异常信息返回
            if (context.Exception is BusinessException ex)
            {
                string message = string.Empty;
                if (ex.Paramters != null && ex.Paramters.Length != 0)
                {
                    message = _stringLocalizer[ex.Message, ex.Paramters];
                }
                else
                {
                    message = _stringLocalizer[ex.Message];
                }

                response = new
                {
                    Code = 500,
                    Message = message
                };

                // ... 记录异常日志 ...
            }
            else
            {

                response = new
                {
                    Code = 500,
                    context.Exception.Message
                };

                // ... 记录异常日志 ...
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