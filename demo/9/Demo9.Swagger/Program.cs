using Maomi.Web.Core;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1，这里注入
builder.Services.AddSwaggerGen(options =>
{
	var ioc = builder.Services.BuildServiceProvider();
	// 提供对程序中所有 ApiDescriptionGroup 对象的访问，
	// ApiDescriptionGroup 记录 Controller 的分组描述信息
	var descriptionProvider = ioc.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
	var apiVersionoptions = ioc.GetRequiredService<IOptions<ApiVersioningOptions>>();
	var maomiSwaggerOptions = ioc.GetRequiredService<IOptions<MaomiSwaggerOptions>>();


	HashSet<Assembly> ApiAssemblies = new();
	// 配置分组信息
	// Items 是根据 ApiExplorerSettings.GroupName 进行分组的
	foreach (var description in descriptionProvider.ApiDescriptionGroups.Items)
	{
		// 如果 Controller 没有配置分组，则放到默认分组中
		if (description.GroupName == null)
		{
			options.SwaggerDoc(maomiSwaggerOptions.Value.DefaultGroupName, new OpenApiInfo
			{
				// 分组默认的 Api 版本号
				Version = apiVersionoptions.Value.DefaultApiVersion.ToString(),
				Title = maomiSwaggerOptions.Value.DefaultGroupTitle
			});

			// 保存每个 Action 反射的 MethodInfo
			foreach (var item in description.Items)
			{
				if (item.TryGetMethodInfo(out var methodInfo))
				{
					var assembly = methodInfo.DeclaringType?.Assembly;
					if (assembly != null) ApiAssemblies.Add(assembly);
				}
			}
		}
		else
		{
			options.SwaggerDoc(description.GroupName, new OpenApiInfo
			{
				Version = apiVersionoptions.Value.DefaultApiVersion.ToString(),
				Title = description.GroupName,
			});
		}
	}

	// 加载所有控制器对应程序集的文档
	var dir = new DirectoryInfo(AppContext.BaseDirectory);
	var files = dir.GetFiles().Where(x => x.Name.EndsWith(".xml")).ToArray();
	foreach (var item in files)
	{
		// 如果 Controller 程序集的 xml 文件存在，则加载
		if (ApiAssemblies.Any(x => item.Name.Equals(x.GetName().Name + ".xml", StringComparison.CurrentCultureIgnoreCase)))
			options.IncludeXmlComments(item.FullName);
	}

	BuildGroupApis(options, maomiSwaggerOptions.Value);

	// 配置每个分组中有哪些 Action
	void BuildGroupApis(SwaggerGenOptions swaggerGenOptions, MaomiSwaggerOptions maomiSwaggerOptions)
	{
		// docname == GroupName
		swaggerGenOptions.DocInclusionPredicate((string docname, ApiDescription apiDescription) =>
		{
			if (!apiDescription.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
			// 属于默认分组
			if (docname == maomiSwaggerOptions.DefaultGroupName && apiDescription.GroupName == null)
			{
				return true;
			}

			return apiDescription.GroupName == docname;
		});
	}
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	var ioc = app.Services;
	var descriptionProvider = ioc.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
	var maomiSwaggerOptions = ioc.GetRequiredService<IOptions<MaomiSwaggerOptions>>();

	app.UseSwagger();

	app.UseSwaggerUI(options =>
	{
		bool haveDefault = false;

		// 配置页面显示和使用哪些位置的 swagger.json 文件
		foreach (var description in descriptionProvider.ApiDescriptionGroups.Items)
		{
			if (description.GroupName == null)
			{
				haveDefault = true;
				continue;
			}
			options.SwaggerEndpoint($"{description.GroupName}/swagger.json", description.GroupName);
		}

		// 有默认不带分组的
		if (haveDefault)
		{
			options.SwaggerEndpoint($"{maomiSwaggerOptions.Value.DefaultGroupName}/swagger.json", maomiSwaggerOptions.Value.DefaultGroupName);
		}
	});
}

app.UseAuthorization();

app.MapControllers();

app.Run();
