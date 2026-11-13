# Maomi.I18n.AspNetCore 使用指南

本包将 [`Maomi.I18n`](../Maomi.I18n/USAGE.md) 接入 ASP.NET Core：从请求选择语言、提供 HTTP 请求文化上下文，并本地化 DataAnnotations。

- NuGet：`dotnet add package Maomi.I18n.AspNetCore --version 2.4.0`
- 目标框架：`net8.0`、`net9.0`、`net10.0`
- 命名空间：`Maomi.I18n`
- 官方教程：[i18n 多语言支持](https://docs.whuanle.cn/zh/maomi_framework/start/2.i18n)

## 接入 ASP.NET Core

准备好 `i18n/zh-CN.json`、`i18n/en-US.json` 等资源文件并复制到输出目录（资源结构及 marker 类型规则见 [Maomi.I18n 指南](../Maomi.I18n/USAGE.md)）。然后配置服务及中间件：

```csharp
using Maomi.I18n;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddI18nAspNetCore(defaultLanguage: "zh-CN");
builder.Services.AddI18nResource(factory =>
    factory.AddJsonDirectory<Program>(Path.Combine(AppContext.BaseDirectory, "i18n")));
builder.Services.AddControllers().AddI18nDataAnnotation();

var app = builder.Build();
app.UseRouting();
app.UseI18n();
app.MapControllers();
app.Run();

public sealed class HomeController(IStringLocalizer<HomeController> localizer)
    : Microsoft.AspNetCore.Mvc.ControllerBase
{
    [Microsoft.AspNetCore.Mvc.HttpGet("/")]
    public string Get() => localizer["Hello"];
}
```

`AddI18nAspNetCore` 注册基础 i18n 和请求本地化配置；它不会加载 JSON 文件。资源必须在应用首次解析 `RequestLocalizationOptions`（通常是 `UseI18n()` 加入 pipeline）前注册。`UseI18n()` 应放在 endpoints 前；如需读取路由文化，先 `UseRouting()`。

## 请求如何选择文化

默认选择顺序为 query string、cookie、`Accept-Language` header，然后是 Maomi 的路由选择器。默认 query 参数是 `culture` / `ui-culture`；路由选择器只识别 `c`（culture）和 `uic`（UI culture，缺省沿用 `c`）。例如：

```text
/products?culture=ja-JP
/products?culture=ja-JP&ui-culture=ja-JP
/ja-JP/products  // 路由模板需将语言段命名为 c，例如 /{c}/products
```

前面的 provider 一旦选出受支持的文化，后面的路由 provider 不会覆盖。也可在控制器或服务中注入 `IStringLocalizer<T>`；请求文化由 `HttpI18nContext` 提供。

`AddI18nDataAnnotation()` 添加 MVC DataAnnotations 本地化。验证消息作为资源 key 使用，模型类型所在程序集应与对应 `IStringLocalizer<TModel>` 资源 marker 的程序集匹配：

```csharp
public sealed class CreateRequest
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "NameRequired")]
    public string? Name { get; set; }
}
```

## 运行时增加语言：资源和请求白名单是两件事

`I18nResourceFactory` 可运行时增加、替换或移除 `I18nResource`；具体步骤及集合一致性要求见 [Maomi.I18n 的运行时资源章节](../Maomi.I18n/USAGE.md#运行时增加替换和移除资源)。但 Web 请求还受 ASP.NET Core `RequestLocalizationOptions.SupportedCultures` / `SupportedUICultures` 限制。

当前 `AddI18nAspNetCore` 会在 `RequestLocalizationOptions` 初始化时从资源工厂复制支持文化列表，ASP.NET Core 的 localization middleware 也在 pipeline 初始化时建立文化查找表。因此，**启动后只给资源工厂加入 `ja-JP`，不会让已启动的 Web middleware 自动接受 `?culture=ja-JP` 或 `Accept-Language: ja-JP`**。应用启动时已注册的语言可正常请求；若部署后需新增/移除 Web 语言并即时生效，需要单独提供可动态更新的请求文化 middleware/策略（当前扩展没有此 API）。不要把 JSON 热更新等同于请求文化白名单热更新。

## 常见问题

- 必须注册 `IHttpContextAccessor`，`HttpI18nContext` 才能读取当前 HTTP 请求；没有请求时使用默认文化。
- 在调用 `UseI18n()` 前完成所有启动时资源注册，确保 `SupportedCultures` 能包含这些语言。
- `UseI18n()` 不会替代 `UseRouting()`、`MapControllers()`，也不会自动加载资源文件。
- query / cookie / header provider 优先于路由选择器；路由参数名称固定为 `c` / `uic`。
- 默认语言、JSON 加载、程序集隔离、自定义 `I18nResource` 见 [`Maomi.I18n/USAGE.md`](../Maomi.I18n/USAGE.md)。
