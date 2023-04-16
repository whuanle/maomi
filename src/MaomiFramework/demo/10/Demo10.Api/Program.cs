using Demo10.Api.Controllers;
using Maomi.I18n;
using Maomi.Web.Core;
using Maomi.Web.Core.Filtters;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);
// 注入 i18n 服务
builder.Services
    .AddI18n(
    isResource: false,
    basePath: AppDomain.CurrentDomain.BaseDirectory + "i18n",
    defaultLanguage: "zh-CN");
// 添加 .AddDataAnnotationsLocalization();
builder.Services.AddControllers()
    .AddI18nDataAnnotation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ActionFiltter>();


var app = builder.Build();

// i18n 中间件
app.UseI18n();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
