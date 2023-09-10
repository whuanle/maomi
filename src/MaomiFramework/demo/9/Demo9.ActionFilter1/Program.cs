using Demo9.ActionFilter1;
using Maomi;
using Maomi.I18n;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen();
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
