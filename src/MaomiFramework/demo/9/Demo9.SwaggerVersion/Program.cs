using Maomi.Web.Core;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1，这里注入
builder.Services.AddMaomiSwaggerGen(
    setupMaomiSwaggerAction: null,
    setupSwaggerAction: null,
    setupApiVersionAction: null,
    setupApiExplorerAction: o =>
    {
        // 获取或设置版本参数到 url 地址中
        o.SubstituteApiVersionInUrl = true;
        // swagger 页面默认填入的版本号
        o.DefaultApiVersion = new ApiVersion(1, 0);
        // 显示的版本分组格式
        o.GroupNameFormat = "'v'VVV";
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // 2，这里配置中间件
    app.UseMaomiSwagger();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
