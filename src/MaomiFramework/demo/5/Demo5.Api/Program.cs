using Maomi.I18n;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加 i18n 多语言支持
builder.Services.AddI18n(defaultLanguage: "zh-CN");
// 设置多语言来源-json
builder.Services.AddI18nResource(option =>
{
    var basePath = "i18n";
    option.AddJson<Program>(basePath);
});

var app = builder.Build();

app.UseI18n();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseRouting();
app.Use(async (HttpContext context, RequestDelegate next) =>
{
    var localizer = context.RequestServices.GetRequiredService<IStringLocalizer>();
    await context.Response.WriteAsync(localizer["���ﳵ:��Ʒ����"]);
    return;
});

app.Run("http://*:5177");
