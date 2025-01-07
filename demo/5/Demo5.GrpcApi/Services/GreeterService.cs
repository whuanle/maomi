using Demo5.GrpcApi;
using Demo5.HttpApi;
using Grpc.Core;
using Microsoft.AspNetCore.Builder.Extensions;
using System.Diagnostics;
using System.Text.Json;

namespace Demo5.GrpcApi.Services;
public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        if (request.Name == "error")
        {
            throw new BusinessException(500, "用户未填写手机号");
        }
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}
