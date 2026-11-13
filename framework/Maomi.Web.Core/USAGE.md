# Maomi.Web.Core 使用指南

## 用途与环境

`Maomi.Web.Core` 为 ASP.NET Core 提供 `MaomiWebModule`、常用 API 响应模型，并集成 Maomi 模块、i18n 与 Swagger 包。支持 `net10.0`。

```bash
dotnet add package Maomi.Web.Core --version 2.3.0
```

项目依赖 `Maomi.Core`、`Maomi.I18n.AspNetCore`、`Maomi.I18n` 和 `Maomi.Swagger`。Web 项目需引用 ASP.NET Core shared framework。`MaomiWebModule` 会注册 `AddI18nAspNetCore()`、Controllers 和 `AddI18nDataAnnotation()`；不会自动启用 Swagger endpoint/UI。

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

依赖模块先执行 `ConfigureServices`，因此 `MaomiWebModule` 的 i18n/MVC 注册先于 `AppModule` 的资源注册。确认资源 JSON 已复制到输出目录。需要 Swagger 时，另在服务配置中调用 `AddMaomiSwaggerGen`，并在 host pipeline 中调用 `UseMaomiSwagger`。

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

## 顺序与常见问题

- `MaomiWebModule` 是 `IModule`，需通过 `[InjectModule<MaomiWebModule>]` 加入模块依赖图；单纯安装包不会自动执行它。
- `UseI18n()` 要在 controller endpoint 执行前调用；如需从路由值读取文化，先 `UseRouting()`。使用 `HttpI18nContext` 时，确保 `IHttpContextAccessor` 已注册。
- `MaomiWebModule` 会注册 MVC 和数据注解本地化，但不会自动加载 JSON 资源；在后续模块的 `ConfigureServices` 中用 `AddI18nResource` 注册。
- `Res.IsSuccess` 仅检查数字 200，不会把其它成功码视为成功，也不会自动映射为 HTTP 状态码。
- `Maomi.Web.Core` 目标为 `net10.0`；消费应用需选择兼容的 .NET SDK / TFM。
