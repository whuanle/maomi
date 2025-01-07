using Demo5.GrpcApi.Services;
using Maomi.I18n;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// ��� i18n ������֧��
builder.Services.AddI18nAspNetCore(defaultLanguage: "zh-CN");

// ���ö�������Դ-json
builder.Services.AddI18nResource(option =>
{
    var basePath = "i18n";
    option.AddJsonDirectory(basePath);
});

builder.Services.AddGrpc(o =>
{
    o.Interceptors.Add<BusinessInterceptor>();
});


var app = builder.Build();

app.UseI18n();

app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
