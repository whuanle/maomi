using Demo5.HttpApi;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Localization;

namespace Demo5.GrpcApi.Services;

/// <summary>
/// 业务异常拦截器.
/// </summary>
public class BusinessInterceptor : Interceptor
{
    private readonly ILogger<BusinessInterceptor> _logger;
    private readonly IStringLocalizer _stringLocalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessInterceptor"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="stringLocalizer"></param>
    public BusinessInterceptor(ILogger<BusinessInterceptor> logger, IStringLocalizer stringLocalizer)
    {
        _logger = logger;
        _stringLocalizer = stringLocalizer;
    }

    /// <inheritdoc/>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        try
        {
            var response = await continuation(request, context);
            return response;
        }
        catch (BusinessException ex)
        {
            // ... 打印日志 ...

            string message = string.Empty;
            if (ex.Paramters != null)
            {
                message = _stringLocalizer[ex.Message, ex.Paramters];
            }
            else
            {
                message = _stringLocalizer[ex.Message];
            }

            throw new RpcException(new Status(StatusCode.Internal, message));
        }
        catch (Exception ex)
        {
            // ... 打印日志 ...

            if (ex is RpcException)
            {
                throw;
            }

            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
