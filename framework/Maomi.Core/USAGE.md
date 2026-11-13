# Maomi.Core 使用指南

`Maomi.Core` 用模块组织服务注册，并可按模块程序集扫描带有 `InjectOn` 特性的服务。它适用于控制台、Web、WPF 和类库项目；不要求每个项目都定义模块——只有需要参与初始化或触发程序集扫描的项目才需要模块。

- NuGet：`dotnet add package Maomi.Core --version 2.4.0`
- 目标框架：`net8.0`、`net9.0`、`net10.0`
- 命名空间：`Maomi`
- 官方模块教程：[模块化、自动服务注册](https://docs.whuanle.cn/zh/maomi_framework/start/1.module)

## 先理解模块和程序集的关系

一个 DLL 可以定义零个、一个或多个 `IModule`。模块是显式初始化入口，程序集是服务扫描边界；**引用一个 DLL 不等于自动初始化其中所有模块**。

- `[InjectModule<T>]` 在模块之间建立依赖；入口模块可通过依赖链使用同 DLL 或其他 DLL 的多个模块。依赖模块先执行 `ConfigureServices`。
- 默认情况下，每个依赖树中的模块程序集只扫描一次。
- `CustomAssembies` 是另一种“从程序集发现模块”的入口。当前实现对每个自定义程序集只选择一个 `IModule`，不会把该 DLL 中的所有模块都自动初始化；需要精确选择多个模块时，用 `[InjectModule<T>]` 建立依赖。

例如，`Catalog.Data.dll` 可包含 `DatabaseModule` 和 `RepositoryModule`，由功能模块明确依赖它们：

```csharp
using Maomi;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddModule<AppModule>();
using var provider = services.BuildServiceProvider();

public sealed class DatabaseModule : IModule
{
    public void ConfigureServices(ServiceContext context)
    {
        context.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
    }
}

[InjectModule<DatabaseModule>]
public sealed class RepositoryModule : IModule
{
    public void ConfigureServices(ServiceContext context)
    {
        // 此时 DatabaseModule 已经执行。
    }
}

// App/API assembly explicitly composes the modules from Catalog.Data.dll.
[InjectModule<RepositoryModule>]
public sealed class AppModule : IModule
{
    public void ConfigureServices(ServiceContext context)
    {
        // 注册应用级服务。
    }
}

public interface IDbConnectionFactory { }
public sealed class DbConnectionFactory : IDbConnectionFactory { }
```

`AddModule<AppModule>()` 要在 `BuildServiceProvider()` / `builder.Build()` 之前调用。模块实现由容器以 Singleton 注册；框架先按依赖顺序调用 `ConfigureServices`，再扫描相关程序集中的服务。`ServiceContext` 可访问 `Services`、`Configuration` 和 `Modules`（本次模块树发现的模块及其程序集）。

## 自动注册服务

在模块树程序集中的具体类上标记生命周期特性：

```csharp
public interface IClock { DateTimeOffset Now { get; } }

[InjectOnScoped]
public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
```

可用特性为 `[InjectOn]`、`[InjectOnScoped]`、`[InjectOnSingleton]`、`[InjectOnTransient]`；默认生命周期是 Scoped。`InjectScheme` 可选 `OnlyInterfaces`、`OnlyBaseClass`、`Any`、`Some`、`None`。例如只注册显式列出的服务类型：

```csharp
[InjectOnScoped(Scheme = InjectScheme.Some,
    ServiceTypes = new Type[] { typeof(IClock) })]
public sealed class SystemClock : IClock { /* ... */ }
```

`ServiceKey` 可指定 .NET keyed DI 注册的 key；`ModuleOptions.FilterServiceTypes` 可排除不希望注册的接口或基类。

> **当前实现注意事项：** `OnlyInterfaces`、`OnlyBaseClass` 和 `Any` 在注册接口/父类之外也会注册实现类本身，即使 `Own` 为 `false`。`Some` 只注册 `ServiceTypes` 中列出的类型；设置 `Own = true` 也不会额外注册实现类。`None` 仅在 `Own = true` 时注册实现类。请按实际注册结果设计消费端，不要只按枚举名称推断。

## 显式加入额外程序集

若程序集不在 `[InjectModule<T>]` 依赖树中，可将它作为自定义程序集传入。参数选项重载目前是 `Type` 重载：

```csharp
using System.Reflection;

services.AddModule(typeof(AppModule), options =>
{
    options.CustomAssembies.Add(Assembly.Load("PluginAssembly"));
});
```

属性名 `CustomAssembies` 是 API 的既有拼写。DLL 中至少要有一个可实例化的 `IModule`；自定义程序集模块的依赖特性不会构建成入口模块的依赖树。若需要指定 DLL 中的多个模块及其顺序，改用显式模块依赖，而不要依赖程序集自动挑选。

## 生命周期与常见问题

- 模块构造函数从框架创建的临时 provider 解析依赖；不要让模块构造函数依赖本轮随后扫描才注册的服务。需要注册时，在 `ConfigureServices` 使用 `context.Services`。
- `AddModule` 内部会构建 provider；应在所有必要的基础服务注册后调用它，并始终早于最终 host/provider 构建。
- 入口模块必须实现 `IModule`；依赖环会导致 `InvalidOperationException`。
- 自动扫描只覆盖模块依赖树程序集和显式加入的自定义程序集，不会扫描进程里所有已加载 DLL。
- `AddModule<TModule>(options => ...)` 当前不会把 options 传给 `Type` 重载；需要 `CustomAssembies` 时使用 `AddModule(typeof(TModule), ...)`。
- Scoped 服务不要被 Singleton 捕获；需要 keyed 服务时，消费端也必须使用同一个 key 解析。
- 扫描边界、服务属性及高级行为详见[官方模块教程](https://docs.whuanle.cn/zh/maomi_framework/start/1.module)。
