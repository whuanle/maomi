# Maomi.I18n.AspNetCore 使用指南

## 用途与环境

`Maomi.I18n.AspNetCore` 将 `Maomi.I18n` 接入 ASP.NET Core 请求本地化，包括请求文化解析、HTTP 上下文和 DataAnnotations。支持 `net8.0`、`net9.0`、`net10.0`；该包依赖 `Maomi.I18n`。

```bash
dotnet add package Maomi.I18n.AspNetCore --version 2.3.0
```

JSON 资源格式、路径和本地化器用法见 `Maomi.I18n` 的 `USAGE.md`。注册资源时使用 `AddI18nResource`，并在应用启动前将 JSON 文件复制到运行目录。

## DI、请求文化与中间件

```csharp
using Maomi.I18n;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddI18nAspNetCore(defaultLanguage: "zh-CN");
builder.Services.AddI18nResource(f =>
    f.AddJsonDirectory<Program>(Path.Combine(AppContext.BaseDirectory, "i18n")));
builder.Services.AddControllers().AddI18nDataAnnotation();

var app = builder.Build();
app.UseRouting();
app.UseI18n();
app.MapControllers();
app.Run();
```

`AddI18nAspNetCore` 注册基础本地化服务、`RequestLocalizationOptions` 和 `HttpI18nContext`。`UseI18n()` 获取这些 options 并执行 ASP.NET Core 的 Request Localization Middleware。将它放在 endpoint 执行前；若使用路由值选文化，需先 `UseRouting()`，让中间件能读取路由值。

内置选择器依序支持 query string、cookie 和 `Accept-Language` header；包内的路由选择器接在这些选择器之后，读取 route value `c`（culture）和 `uic`（UI culture，未指定时沿用 `c`）。默认 query 名称为 `culture` / `ui-culture`。仅接受已注册资源的 supported cultures；默认语言由 `AddI18nAspNetCore` 的参数设置。可在控制器中注入 `IStringLocalizer<T>` 使用字符串。

## DataAnnotations

`AddI18nDataAnnotation()` 是 `IMvcBuilder` 扩展，会启用 `AddDataAnnotationsLocalization`，并按模型类型创建本地化器：

```csharp
builder.Services.AddControllers().AddI18nDataAnnotation();

public sealed class CreateRequest
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "NameRequired")]
    public string? Name { get; set; }
}
```

确保资源中存在 `NameRequired` 键，且资源的泛型 marker 与模型位于同一应用程序集，才能按模型类型找到该资源。

## 顺序与常见问题

- `AddI18nAspNetCore` 先于 `AddI18nResource` 调用；在构建 host / 首次解析 `RequestLocalizationOptions` 前完成资源注册，让 supported cultures 包含所有语言。
- 必须在服务集合中注册 `IHttpContextAccessor`，以便 `HttpI18nContext` 读取请求文化；没有 HTTP 请求时使用默认文化。
- `UseI18n()` 不会自动注册资源，也不取代 `UseRouting()`、`MapControllers()`。
- 默认选择器的 query/cookie/header 优先于路由选择器；若它们已选出文化，路由 `c` / `uic` 不会覆盖。
- 路由文化键固定为 `c` / `uic`；任意路由段名不会被识别。不要等到所有 DI 注册完成后才添加 JSON 资源。
