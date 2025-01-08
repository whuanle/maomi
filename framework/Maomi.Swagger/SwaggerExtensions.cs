// <copyright file="SwaggerExtensions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace Maomi.Web.Core;

/// <summary>
/// Swagger 扩展.
/// </summary>
public static class SwaggerExtensions
{
    private static readonly HashSet<Assembly> ApiAssemblies = new();

    /// <summary>
    /// Swagger 配置，用于生成 swwagger.json 文件.
    /// </summary>
    /// <param name="services">.</param>
    /// <param name="setupMaomiSwaggerAction">swagger 配置.</param>
    /// <param name="setupSwaggerAction">自定义配置.</param>
    /// <param name="setupApiVersionAction">设置 API 版本号.</param>
    /// <param name="setupApiExplorerAction">设置界面如何显示 API 版本号.</param>
    public static void AddMaomiSwaggerGen(
        this IServiceCollection services,
        Action<MaomiSwaggerOptions>? setupMaomiSwaggerAction = null,
        Action<SwaggerGenOptions>? setupSwaggerAction = null,
        Action<ApiVersioningOptions>? setupApiVersionAction = null,
        Action<ApiExplorerOptions>? setupApiExplorerAction = null)
    {
        if (setupMaomiSwaggerAction == null)
        {
            setupMaomiSwaggerAction = option => { };
        }

        services.Configure(setupMaomiSwaggerAction);

        if (setupApiVersionAction != null)
        {
            // 配置 Api 版本信息
            Action<ApiVersioningOptions> defaultSetupApiVersionAction = (setup) =>
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
            };

            defaultSetupApiVersionAction += setupApiVersionAction;
            services.AddApiVersioning(defaultSetupApiVersionAction);
        }

        if (setupApiExplorerAction != null)
        {
            var defaultVersion = services.BuildServiceProvider()
                .GetRequiredService<IOptions<ApiVersioningOptions>>()
                .Value.DefaultApiVersion;

            // 在 swagger 中显示版本信息，
            // 进一步使用版本号进行隔分
            Action<ApiExplorerOptions> defaultSetupApiExplorerAction = setup =>
            {
            };

            defaultSetupApiExplorerAction += setupApiExplorerAction;

            services.AddVersionedApiExplorer(defaultSetupApiExplorerAction);
        }

        services.AddSwaggerGen(options =>
        {
            // 模型类过滤器
            options.SchemaFilter<MaomiSwaggerSchemaFilter>();

            var ioc = services.BuildServiceProvider();

            // 提供对程序中所有 ApiDescriptionGroup 对象的访问，
            // ApiDescriptionGroup 记录 Controller 的分组描述信息
            var descriptionProvider = ioc.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
            var apiVersionDescriptionProvider = ioc.GetRequiredService<IApiVersionDescriptionProvider>();
            var apiVersionoptions = ioc.GetRequiredService<IOptions<ApiVersioningOptions>>();
            var maomiSwaggerOptions = ioc.GetRequiredService<IOptions<MaomiSwaggerOptions>>();

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
                            if (assembly != null)
                            {
                                ApiAssemblies.Add(assembly);
                            }
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
                {
                    options.IncludeXmlComments(item.FullName);
                }
            }

            options.BuildGroupApis(maomiSwaggerOptions.Value);

            // 最后使用用户自定义配置代码
            if (setupSwaggerAction != null)
            {
                setupSwaggerAction.Invoke(options);
            }
        });
    }

    /// <summary>
    /// swagger 页面显示配置.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="setupAction">配置 swagger.</param>
    /// <param name="setupUIAction">配置显示规则.</param>
    /// <returns><see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseMaomiSwagger(
        this IApplicationBuilder app,
        Action<SwaggerOptions>? setupAction = null,
        Action<SwaggerUIOptions>? setupUIAction = null)
    {
        var ioc = app.ApplicationServices;
        var descriptionProvider = ioc.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
        var maomiSwaggerOptions = ioc.GetRequiredService<IOptions<MaomiSwaggerOptions>>();

        app.UseSwagger(setupAction);

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

            // 执行用户自定义配置
            if (setupUIAction != null)
            {
                setupUIAction.Invoke(options);
            }
        });

        return app;
    }

    // 配置每个分组中有哪些 Action
    private static void BuildGroupApis(this SwaggerGenOptions swaggerGenOptions, MaomiSwaggerOptions maomiSwaggerOptions)
    {
        // docname == GroupName
        swaggerGenOptions.DocInclusionPredicate((string docname, ApiDescription apiDescription) =>
        {
            if (!apiDescription.TryGetMethodInfo(out MethodInfo methodInfo))
            {
                return false;
            }

            // 属于默认分组
            if (docname == maomiSwaggerOptions.DefaultGroupName && apiDescription.GroupName == null)
            {
                return true;
            }

            return apiDescription.GroupName == docname;
        });
    }
}