# Maomi.Core 使用指南

## 用途与环境

`Maomi.Core` 提供模块依赖发现、模块化服务配置以及基于特性的程序集服务注册。支持 `net8.0`、`net9.0`、`net10.0`。

```bash
dotnet add package Maomi.Core --version 2.3.0
```

包依赖 `Microsoft.Extensions.DependencyInjection` 和 `Microsoft.Extensions.Configuration.Abstractions`。应用需先准备 `IServiceCollection`；模块的 `ServiceContext` 可访问 `Services`、`Configuration` 和已识别的 `Modules`。

## 定义并注册模块

入口和依赖模块实现 `IModule.ConfigureServices`。用 `[InjectModule<T>]` 声明依赖，并在应用构建容器之前注册入口模块：

```csharp
using Maomi;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddModule<AppModule>();
using var provider = services.BuildServiceProvider();

public interface IClock { DateTimeOffset Now { get; } }

[InjectOnScoped]
public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}

[InjectModule<ClockModule>]
public sealed class AppModule : IModule
{
    public void ConfigureServices(ServiceContext context) { }
}

public sealed class ClockModule : IModule
{
    public void ConfigureServices(ServiceContext context) { }
}
```

`AddModule<TModule>()` 从入口模块递归发现 `[InjectModule]` 依赖，并扫描这些模块所在程序集。也可用 `services.AddModule(typeof(AppModule))` 注册入口。额外程序集通过 `AddModule(Type, options => options.CustomAssembies.Add(assembly))` 指定，框架会从每个程序集选取一个 `IModule` 实现；`CustomAssembies` 是源码中的拼写。模块依赖环会抛出 `InvalidOperationException`。

## 自动注册服务

扫描仅处理带有 `InjectOnAttribute` 的具体类。默认 `InjectOn` 生命周期是 Scoped，注册模式为 `OnlyInterfaces`；`InjectOnScoped`、`InjectOnSingleton`、`InjectOnTransient` 分别选择对应生命周期。`InjectScheme` 可选 `OnlyInterfaces`、`OnlyBaseClass`、`Any`、`Some`、`None`。

**当前实现行为（不要按模式名称推断意图）：** `OnlyInterfaces`、`OnlyBaseClass` 和 `Any` 都会注册实现类本身，并注册各自模式找到的接口和/或父类。`ModuleBuilder` 虽在注册接口或父类前初始化 `hasInterface`、`hasBaseClass` 为 `false`，但找到候选后没有将它们改为 `true`，因此自注册判断始终成立。编辑使用方代码时，不要假设这些命名模式能避免实现类自注册；这可能产生额外的容器注册。`Own = true` 也表示注册实现类，但对这三种模式而言即使 `Own = false` 也会自注册。

`Some` 只注册 `ServiceTypes` 列出的类型，随后直接结束该分支；即使设置 `Own = true` 也不会额外注册实现类，除非 `ServiceTypes` 本身包含实现类型。`ServiceTypes` 为空时不注册服务。`None` 默认不注册任何服务，只有 `Own = true` 时才注册实现类。

```csharp
public interface ICacheService { }

[InjectOnSingleton(InjectScheme.Any, Own = true)]
public sealed class CacheService : ICacheService { }
```

`InjectOnAttribute` 还支持 `ServiceKey`；筛选接口类型可在 `ModuleOptions.FilterServiceTypes` 配置。带属性的服务由其所在模块程序集或依赖模块程序集扫描注册。

## 生命周期与顺序

- 先实例化依赖模块，再实例化入口模块；随后扫描模块树中的程序集并注册服务。因此模块的 `ConfigureServices` 按依赖优先顺序执行，但此时尚未完成特性服务扫描。
- 模块实现本身由框架注册为 Singleton。模块构造函数不要依赖同一轮稍后扫描出来的服务；应在 `ConfigureServices` 中向 `context.Services` 注册框架服务。
- 在调用 `BuildServiceProvider()` / `builder.Build()` 前调用 `AddModule`，避免框架内部提前构建的 provider 与应用最终 provider 混用。
- 泛型重载 `AddModule<TModule>(options => ...)` 当前实现没有将 options 转交给 Type 重载；需要 `CustomAssembies` 时使用 `AddModule(typeof(TModule), ...)`。

## 常见问题

- 入口模块必须实现 `IModule`，否则注册时会抛出 `TypeLoadException`。
- 类没有 `InjectOn` 特性，或它的程序集不在模块依赖树中时，不会自动注册。
- Scoped 服务不要标为 Singleton；使用键控服务时，消费端也必须按对应 key 解析。
- 不要把特性扫描理解为扫描应用全部已加载程序集；默认扫描范围是模块树程序集，其他程序集需显式加入 `CustomAssembies`。
