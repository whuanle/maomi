# Maomi.Web.Core 使用指南

`Maomi.Web.Core` 是 ASP.NET Core 应用的组合包：提供入口 `MaomiWebModule`、常用响应模型，并带入 Maomi 模块、i18n 和 Swagger 集成。它不是安装即生效的全局 host 配置，入口模块必须显式加入模块图。

- NuGet：`dotnet add package Maomi.Web.Core --version 2.4.0`
- 目标框架：`net10.0`
- 集成依赖：`Maomi.Core`、`Maomi.I18n`、`Maomi.I18n.AspNetCore`、`Maomi.Swagger`
- 官方模块教程：[模块化、自动服务注册](https://docs.whuanle.cn/zh/maomi_framework/start/1.module)
- 包含的语言集成：[`Maomi.I18n.AspNetCore`](../Maomi.I18n.AspNetCore/USAGE.md)
- Swagger 扩展：[`Maomi.Swagger`](../Maomi.Swagger/USAGE.md)

Web 项目需引用 ASP.NET Core shared framework。`MaomiWebModule` 会注册 `AddI18nAspNetCore()`、Controllers 和 `AddI18nDataAnnotation()`；不会自动启用 Swagger endpoint/UI。

## 接入模块

把 `MaomiWebModule` 声明为应用入口模块依赖，然后在 Web host 构建前调用 `AddModule`：

```csharp
using Maomi;
using Maomi.I18n;
using Maomi.Web.Core;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddModule<AppModule>();
var app = builder.Build();
app.UseRouting();
app.UseI18n();
app.MapControllers();
app.Run();

[InjectModule<MaomiWebModule>]
public sealed class AppModule : IModule
{
    public void ConfigureServices(ServiceContext context)
    {
        context.Services.AddHttpContextAccessor();
        context.Services.AddI18nResource(f =>
            f.AddJsonDirectory<AppModule>(Path.Combine(AppContext.BaseDirectory, "i18n")));
    }
}
```

依赖模块先执行 `ConfigureServices`，因此 `MaomiWebModule` 的 i18n/MVC 注册先于 `AppModule` 的资源注册。确认资源 JSON 已复制到输出目录。需要 Swagger 时，在 `AppModule.ConfigureServices`（或其它服务配置处）调用 `AddMaomiSwaggerGen`，并在 host pipeline 中调用 `UseMaomiSwagger`；加入该模块不会自动启用 Swagger endpoint/UI。

## 响应模型

`Res<T>` 提供 `Code`、`Msg`、`Data`，`IsSuccess` 的实现判断条件是 `Code == 200`。可用 `Res.Create` 创建响应：

```csharp
using Maomi.Web.Core;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/items")]
public sealed class ItemsController : ControllerBase
{
    [HttpGet]
    public ActionResult<Res<string>> Get() =>
        Ok(Res.Create(200, "ok", "item-list"));
}
```

分页模型为 `PageRes<T>`（`PageNo`、`PageSize`、`List`）；`PageListRes<T>` 继承 `Res<PageRes<IEnumerable<T>>>`，`PageArrayRes<T>` 继承 `Res<PageRes<T[]>>`。它们只定义数据形状，不会自动执行分页查询或设置 HTTP 状态码。

## 接入顺序与常见问题

- `MaomiWebModule` 是 `IModule`，需通过 `[InjectModule<MaomiWebModule>]` 加入模块依赖图；单纯安装包不会自动执行它。
- `UseI18n()` 要在 controller endpoint 执行前调用；如需从路由值读取文化，先 `UseRouting()`。使用 `HttpI18nContext` 时，确保 `IHttpContextAccessor` 已注册。
- `MaomiWebModule` 会注册 MVC 和数据注解本地化，但不会自动加载 JSON 资源；在后续模块的 `ConfigureServices` 中用 `AddI18nResource` 注册。
- `Res.IsSuccess` 仅检查数字 200，不会把其它成功码视为成功，也不会自动映射为 HTTP 状态码。
- `Maomi.Web.Core` 目标为 `net10.0`；消费应用需选择兼容的 .NET SDK / TFM。
- 资源文件路径、请求文化优先级及运行时新增 Web 语言的限制见 [`Maomi.I18n.AspNetCore/USAGE.md`](../Maomi.I18n.AspNetCore/USAGE.md)。
