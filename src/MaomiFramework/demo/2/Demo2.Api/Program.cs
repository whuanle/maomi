using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.CreateLogger();
builder.Host.UseSerilog(Log.Logger);
//builder.Host.UseSerilog();


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
	// 避免打印大量的请求和响应内容，只打印 4kb
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//app.UseSerilogRequestLogging(options =>
//{
//	options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
//	{
//		diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
//	};
//});

app.UseHttpLogging();

app.UseAuthorization();

app.MapControllers();

app.Run();
