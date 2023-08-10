using Maomi.I18n;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddI18n(defaultLanguage: "zh-CN");
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
    await context.Response.WriteAsync(localizer["购物车:商品名称"]);
    return;
});

app.Run("http://*:5177");
