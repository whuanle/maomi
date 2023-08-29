using Demo10.ExceptionFilter;
using Maomi;
using Maomi.I18n;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen();
// 注册模块化服务，并设置 ApiModule 为入口
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
