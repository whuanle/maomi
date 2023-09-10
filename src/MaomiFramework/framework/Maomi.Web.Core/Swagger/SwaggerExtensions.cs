using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Maomi.Web.Core
{
    /// <summary>
    /// Swagger 扩展
    /// </summary>
    public static class SwaggerExtensions
    {
        private static readonly HashSet<MethodInfo> Default = new HashSet<MethodInfo>();

        public class MaomiSwaggerOptions
        {
            public ApiVersion DefaultApiVersion { get; set; } = new ApiVersion(1, 0);
            private readonly DefaultGroupOptions _defaultGroup = new();
            public DefaultGroupOptions DefaultGroup => _defaultGroup;
            public class DefaultGroupOptions
            {
                /// <summary>
                /// 默认分组名称
                /// </summary>
                /// <value></value>
                public string Name { get; set; } = "default";

                /// <summary>
                /// 默认版本号
                /// </summary>
                /// <value></value>
                public string Version { get; set; } = "v1";

                /// <summary>
                /// 默认标题
                /// </summary>
                /// <value></value>
                public string Title { get; set; } = "default";

                /// <summary>
                /// 默认描述
                /// </summary>
                /// <value></value>
                public string Description { get; set; } = "Default";
            }

        }
        /// <summary>
        /// Swagger 配置
        /// </summary>
        /// <param name="swaggerOptions">swagger 配置</param>
        /// <param name="services"></param>
        /// <param name="setupAction">自定义配置</param>
        public static void AddMaomiSwaggerGen(this IServiceCollection services, MaomiSwaggerOptions? swaggerOptions = null, Action<SwaggerGenOptions>? setupAction = null)
        {
            services.AddSwaggerGen(options =>
            {
                if (swaggerOptions == null) swaggerOptions = new();

                services.AddApiVersioning(setup =>
                {
                    setup.DefaultApiVersion = swaggerOptions.DefaultApiVersion;
                    setup.AssumeDefaultVersionWhenUnspecified = true;
                    setup.ReportApiVersions = true;
                });


                // 提供对程序中所有 ApiDescriptionGroup 对象的访问，
                // ApiDescriptionGroup 记录 Controller 的分组描述信息
                var descriptionProvider = services.BuildServiceProvider().GetRequiredService<IApiDescriptionGroupCollectionProvider>();
                //IApiVersionProvider
                foreach (var description in descriptionProvider.ApiDescriptionGroups.Items)
                {
                    // 如果 Controller 没有配置分组，则放到默认分组中
                    if (description.GroupName == null)
                    {
                        options.SwaggerDoc(swaggerOptions.DefaultGroup.Name, new OpenApiInfo
                        {
                            Version = swaggerOptions.DefaultGroup.Version,
                            Title = swaggerOptions.DefaultGroup.Title,
                            Description = swaggerOptions.DefaultGroup.Description
                        });

                        // 保存每个 Action 反射的 MethodInfo
                        foreach (var item in description.Items)
                        {
                            if (item.TryGetMethodInfo(out var methodInfo))
                                Default.Add(methodInfo);
                        }
                    }
                    else
                    {
                        options.SwaggerDoc(description.GroupName, new OpenApiInfo
                        {
                            Version = "v1",
                            Title = description.GroupName,
                        });
                    }
                }

                var dir = new DirectoryInfo(AppContext.BaseDirectory);
                var files = dir.GetFiles().Where(x => x.Name.EndsWith(".xml")).ToArray();
                foreach (var item in files)
                {
                    options.IncludeXmlComments(item.FullName);
                }

                options.DoGroups();

                // 最后使用用户自定义配置代码
                if (setupAction != null)
                {
                    setupAction.Invoke(options);
                }
            });
        }

        public static void DoGroups(this SwaggerGenOptions swaggerGenOptions)
        {
            // docname == GroupName
            swaggerGenOptions.DocInclusionPredicate((docname, b) =>
            {
                if (!b.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

                if (docname == "v1")
                {
                    return Default.Any(x => x == methodInfo);
                }

                var ver = methodInfo.DeclaringType.GetCustomAttributes(true)
                .OfType<ApiExplorerSettingsAttribute>()
                .FirstOrDefault();

                if (ver == null) return false;
                return b.GroupName == docname;
            });
        }
    }

}