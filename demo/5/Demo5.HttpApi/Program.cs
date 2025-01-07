using Demo5.HttpApi;
using Maomi.I18n;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(o =>
{
    o.Filters.Add<BusinessExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

// 添加 i18n 多语言支持
builder.Services.AddI18nAspNetCore(defaultLanguage: "zh-CN");
// 设置多语言来源-json
builder.Services.AddI18nResource(option =>
{
    var basePath = "i18n";
    option.AddJsonDirectory(basePath);
});

var app = builder.Build();

app.UseI18n();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
