using Maomi.Web.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1，这里注入
builder.Services.AddMaomiSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	// 2，这里配置中间件
	app.UseMaomiSwagger();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
