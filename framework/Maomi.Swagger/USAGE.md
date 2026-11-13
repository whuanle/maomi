# Maomi.Swagger 使用指南

## 用途与环境

`Maomi.Swagger` 提供 Maomi 的 Swagger/OpenAPI 文档分组、API 版本探索和 UI 中间件扩展。项目声明支持 `net8.0`、`net9.0`。依赖 ASP.NET Core MVC API Versioning、Versioned API Explorer 和 Swashbuckle。

```bash
dotnet add package Maomi.Swagger --version 2.3.0
```

扩展方法位于 `Maomi.Web.Core` 命名空间。需先注册 MVC controller 服务；版本化 Swagger 还需提供 API Versioning 与 API Explorer 设置委托。

## 服务注册与版本分组

```csharp
using Maomi.Web.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddMaomiSwaggerGen(
    setupMaomiSwaggerAction: options =>
    {
        options.DefaultGroupName = "v1";
        options.DefaultGroupTitle = "API v1";
    },
    setupApiVersionAction: _ => { },
    setupApiExplorerAction: _ => { });
```

传入 `setupApiVersionAction` 时，扩展会先设默认版本 `1.0`、未指定版本时使用默认值、在响应 header 中返回版本信息，并通过 `X-Api-Version` header 或 `version` query 读取版本；你的委托可再覆盖这些设置。传入 `setupApiExplorerAction` 才会注册 versioned API explorer。Controller 可使用 MVC API Versioning 的 `[ApiVersion("1.0")]`，并通过 `ApiExplorerSettings(GroupName = "v1")` 设置分组；未分组 API 进入 `DefaultGroupName`。

`MaomiSwaggerOptions` 的公开选项为 `DefaultGroupName`、`DefaultGroupTitle`。`AddMaomiSwaggerGen` 还接受 `setupSwaggerAction` 自定义 `SwaggerGenOptions`，内置 schema filter，并尝试读取控制器程序集 XML 注释。

## 启用 JSON 与 UI

```csharp
var app = builder.Build();
app.UseRouting();
app.UseMaomiSwagger();
app.MapControllers();
app.Run();
```

`UseMaomiSwagger` 会启用 Swagger JSON 和 Swagger UI，按 API 描述分组自动注册 UI endpoint；未分组文档使用 `DefaultGroupName`。可用 `setupAction` 传给 `UseSwagger`，用 `setupUIAction` 传给 `UseSwaggerUI`，追加 UI 设置。版本请求可用 `?version=1.0` 或 HTTP header `X-Api-Version: 1.0`。

## 顺序与常见问题

- 先 `AddControllers()`，再 `AddMaomiSwaggerGen()`；后者会在配置 Swagger 时读取 API 描述服务。
- 需要此封装的版本分组时，两个版本设置委托都要传入；只传版本化 API Explorer 而没有 API Versioning 时，或只传 API Versioning 而没有 Explorer 时，服务解析可能失败。
- `UseMaomiSwagger()` 放在 endpoint 执行前。应用使用路由时先 `UseRouting()`。
- `DefaultGroupName` 是未分组 API 的文档名；带分组 API 的名称来自 `ApiDescription.GroupName`，需与期望的 UI 文档名匹配。
- 内置 XML 注释支持只会从未分组 API 描述中记录到的控制器程序集读取 XML 文件；仅包含分组 API 描述的程序集可能不会自动加载。请在控制器项目中设置 `<GenerateDocumentationFile>true</GenerateDocumentationFile>`，并确保相应 XML 文件复制到应用输出目录。
