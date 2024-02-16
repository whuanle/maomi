using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 配置 Api 版本信息
builder.Services.AddApiVersioning(setup =>
{
	// 全局默认 api 版本号
	setup.DefaultApiVersion = new ApiVersion(1, 0);
	// 用户请求未指定版本号时，使用默认版本号
	setup.AssumeDefaultVersionWhenUnspecified = true;
	// 响应时，在 header 中返回版本号
	setup.ReportApiVersions = true;
	// 从哪里读取版本号信息
	setup.ApiVersionReader =
	ApiVersionReader.Combine(
	   new HeaderApiVersionReader("X-Api-Version"),
	   new QueryStringApiVersionReader("version"));
});

// 在 swagger 中显示版本信息，
// 进一步使用版本号进行隔分
builder.Services.AddVersionedApiExplorer(o =>
{
	// 获取或设置版本参数到 url 地址中
	o.SubstituteApiVersionInUrl = true;
	// swagger 页面默认填入的版本号
	o.DefaultApiVersion = new ApiVersion(1, 0);
	// 显示的版本分组格式
	o.GroupNameFormat = "'v'VVV";
});

builder.Services.AddSwaggerGen(options =>
{
	var ioc = builder.Services.BuildServiceProvider();
	var apiVersionDescriptionProvider = ioc.GetRequiredService<IApiVersionDescriptionProvider>();
	var apiVersionoptions = ioc.GetRequiredService<IOptions<ApiVersioningOptions>>();
	foreach (var item in apiVersionDescriptionProvider.ApiVersionDescriptions)
	{
		// 给每个版本号创建 swagger.json 
		options.SwaggerDoc(item.GroupName, new OpenApiInfo
		{
			Version = apiVersionoptions.Value.DefaultApiVersion.ToString(),
			Title = item.GroupName,
		});
	}
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	// 配置 ui
	app.UseSwaggerUI(options =>
	{
		var ioc = app.Services;
		var apiVersionDescriptionProvider = ioc.GetRequiredService<IApiVersionDescriptionProvider>();
		var descriptions = apiVersionDescriptionProvider.ApiVersionDescriptions;

		// Build a swagger endpoint for each discovered API version
		foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
		{
			var url = $"/swagger/{description.GroupName}/swagger.json";
			var name = description.GroupName.ToUpperInvariant();
			options.SwaggerEndpoint(url, name);
		}
	});
}

app.UseAuthorization();

app.MapControllers();

app.Run();
