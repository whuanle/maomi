using Demo9.Swagger;
using Maomi;
using Maomi.I18n;
using Maomi.Web.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMaomiSwaggerGen();

// 配置 ApiModule 为模块入口
builder.Services.AddModule<ApiModule>();

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
