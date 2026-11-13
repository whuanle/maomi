# Maomi.Swagger 使用指南

`Maomi.Swagger` 为 ASP.NET Core MVC 增加 Maomi 的 Swagger JSON/UI、文档分组以及可选的 API 版本探索。

- NuGet：`dotnet add package Maomi.Swagger --version 2.4.0`
- 目标框架：`net8.0`、`net9.0`
- 扩展方法命名空间：`Maomi.Web.Core`
- 若使用 `Maomi.Web.Core` 模块封装，参见 [`Maomi.Web.Core/USAGE.md`](../Maomi.Web.Core/USAGE.md)

## 注册 Swagger 和 API version explorer

当前扩展在生成 Swagger 配置时会解析 `IApiVersionDescriptionProvider`，因此即使应用暂时只使用一个版本，也要同时传入 API Versioning 和 API Explorer 配置委托：

```csharp
using Maomi.Web.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddMaomiSwaggerGen(
    setupApiVersionAction: _ => { },
    setupApiExplorerAction: options => options.GroupNameFormat = "'v'VVV");

var app = builder.Build();
app.UseRouting();
app.UseMaomiSwagger();
app.MapControllers();
app.Run();
```

该扩展会启用 Swagger JSON 与 UI。未分组的 API 使用 `DefaultGroupName`（默认 `v1`）和 `DefaultGroupTitle`；带 `[ApiExplorerSettings(GroupName = "...")]` 的 API 使用自己的分组名。UI endpoint 根据应用启动时的 API 描述分组生成。若不需要 API Versioning，可直接使用标准 Swashbuckle 注册方式（`AddSwaggerGen` / `UseSwagger` / `UseSwaggerUI`），而不是当前 `AddMaomiSwaggerGen` 封装。

## 配置文档分组和 API 版本

自定义未分组文档名、标题、Swashbuckle 配置，以及 API Versioning / API Explorer：

```csharp
builder.Services.AddMaomiSwaggerGen(
    setupMaomiSwaggerAction: options =>
    {
        options.DefaultGroupName = "v1";
        options.DefaultGroupTitle = "Catalog API v1";
    },
    setupSwaggerAction: swagger =>
    {
        // swagger.SwaggerDoc / IncludeXmlComments 等自定义设置
    },
    setupApiVersionAction: options =>
    {
        // 默认值：1.0；未指定时采用默认版本；以 X-Api-Version header
        // 或 version query 读取版本；响应中报告版本信息。
    },
    setupApiExplorerAction: options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
```

`setupApiVersionAction` 和 `setupApiExplorerAction` 都要传入；只配置其中一个会导致服务依赖缺失。版本请求默认可通过 `?version=1.0` 或 `X-Api-Version: 1.0` 指定。Controller 可使用 MVC API Versioning 的 `[ApiVersion("1.0")]`，并用 `ApiExplorerSettings(GroupName = "v1")` 指定 Swagger 分组。文档分组名应和 API Explorer 实际产生的 `GroupName` 一致。

`UseMaomiSwagger` 可通过 `setupAction` 和 `setupUIAction` 分别扩展 `UseSwagger` / `UseSwaggerUI` 配置。需要鉴权或自定义 UI route 时，使用相应的 Swashbuckle options 配置。

## XML 注释

控制器项目设置：

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

确保生成的 XML 文件在应用输出目录。当前内置加载逻辑会从未分组 API 描述所涉及的控制器程序集查找 XML 文件；若某程序集只有已分组 API 描述，可能不会被自动加载，可通过 `setupSwaggerAction` 显式 `IncludeXmlComments`。

## 常见问题

- 在 `AddControllers()` 之后调用 `AddMaomiSwaggerGen()`，并传入 API Versioning 与 API Explorer 两个委托。
- 在 endpoints 执行前调用 `UseMaomiSwagger()`；使用路由时先 `UseRouting()`。
- API `GroupName` 必须与 Swagger 文档名一致，否则 endpoint 存在但文档中可能没有对应 API。
- `AddMaomiSwaggerGen()` 当前实现必须同时注册 API Versioning 与 API Explorer；不采用 Maomi 封装时，可改用原生 Swashbuckle 扩展。
- `Maomi.Web.Core` 目标为 `net10.0`，但此包的目标是 `net8.0`、`net9.0`；使用前确认应用 TFM 与依赖兼容。
