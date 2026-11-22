# Maomi.Core.Aot 使用指南

## 用途与环境

`Maomi.Core.Aot` 是 `Maomi.Core` 的 AOT 版本，使用 Roslyn 源生成器在**编译期**完成模块依赖分析和带 `InjectOn` 特性服务的发现，生成零反射的注册代码，适用于 NativeAOT、裁剪发布和普通发布。支持 `net8.0`、`net9.0`、`net10.0`、`net11.0`。

```bash
dotnet add package Maomi.Core.Aot --version 2.3.0
```

包依赖 `Microsoft.Extensions.DependencyInjection` 和 `Microsoft.Extensions.Configuration.Abstractions`，并自带源生成器（引用包时自动生效，无需额外配置）。应用需自备 `IServiceCollection`。

## 定义并注册模块

入口和依赖模块实现 `IModule.ConfigureServices`，用 `[InjectModule<T>]` 声明依赖，在构建容器之前注册入口模块：

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
    private readonly IClock _clock;
    public AppModule(IClock clock) => _clock = clock;   // 模块支持构造函数注入

    public void ConfigureServices(ServiceContext context) { }
}

public sealed class ClockModule : IModule
{
    public void ConfigureServices(ServiceContext context) { }
}
```

模块依赖树、循环依赖检查、服务发现全部在**编译期**完成：

- 循环依赖会在编译时报错 `MAOMI001`，而不是运行时抛异常。
- 服务按模块树程序集收集，同一程序集只注册一次，依赖模块先于入口模块初始化，顺序与 Maomi.Core 一致。
- 也可以使用 `services.AddModule(typeof(AppModule))` 动态注册入口模块。

## 自动注册服务

编译期扫描模块树程序集中带 `InjectOnAttribute` 的具体类（不扫描接口、抽象类、静态类；支持嵌套类，这一行为与 Maomi.Core 跳过公共嵌套类不同）。`InjectOnScoped`、`InjectOnSingleton`、`InjectOnTransient` 分别选择生命周期；`InjectScheme` 可选 `OnlyInterfaces`、`OnlyBaseClass`、`Any`、`Some`、`None`。

**注册语义（与 Maomi.Core 的差异）：** Maomi.Core 在 `OnlyInterfaces`、`OnlyBaseClass`、`Any` 模式下总是额外注册实现类自身；`Maomi.Core.Aot` 修正了这一行为——只有当**没有找到任何候选服务类型**或显式设置 `Own = true` 时，才会注册实现类自身。`Some` 只注册 `ServiceTypes` 列表（为空时编译警告 `MAOMI004`，`Own = true` 时额外注册自身）；`None` 只在 `Own = true` 时注册自身。

```csharp
public interface ICacheService { }

[InjectOnSingleton(InjectScheme.Any, Own = true)]
public sealed class CacheService : ICacheService { }
```

`ServiceKey` 支持键控服务，消费端构造函数参数使用 `[FromKeyedServices("key")]`。

`IDisposable`、`ICloneable`、`IComparable`、`object` 不会被注册为服务类型（与 Maomi.Core 的默认 `FilterServiceTypes` 一致，编译期固定，不可配置）。

## 编译期生成的注册方式

对每个服务类，源生成器分析其公共构造函数并生成**工厂委托**（如 `static sp => new MyService(sp.GetRequiredService<IClock>())`），运行时零反射、对 NativeAOT 和裁剪友好。构造函数参数支持：服务类型、`[FromKeyedServices]` 键控服务、`IServiceProvider`、`IEnumerable<T>`、带默认值的基础类型参数。

无法生成工厂委托时（多个公共构造函数、没有公共构造函数、不受支持的参数等），回退为 `typeof` 注册并由容器在运行时反射激活，同时给出提示诊断 `MAOMI003`；NativeAOT 发布时请确保这类类型可被反射构造（例如保留单一公共构造函数以消除回退）。

## 与 Maomi.Core 的功能差异

- 移除 `ModuleCore.TypeFilter` 类型过滤钩子（编译期扫描无此钩子）。
- 移除 `ModuleOptions`（`CustomAssembies`、`FilterServiceTypes`）：没有运行时扫描，扫描范围在编译期固定为模块树程序集；带 `InjectOn` 特性但所在程序集没有模块的类型不会被注册，编译期给出提示 `MAOMI002`。
- 修正 `OnlyInterfaces`、`OnlyBaseClass`、`Any` 模式总是自注册实现类的行为（见上文）。
- 模块类必须是非抽象、非静态、非泛型类（支持嵌套类）；`[InjectModule]` 依赖的模块必须可见且合法，否则编译报错 `MAOMI005`。
- 不支持开放式泛型服务注册（回退为 `typeof` 注册并提示 `MAOMI003`，NativeAOT 下解析开放式泛型有裁剪风险）。

## 生命周期与顺序

- 先按依赖顺序实例化所有模块并执行 `ConfigureServices`（先依赖模块、后入口模块），随后按模块顺序执行各程序集的服务注册；模块实现注册为 Singleton，支持构造函数注入。
- 模块构造函数注入使用的是 `AddModule` 时构建的临时容器，此时服务扫描尚未执行；模块的 `ConfigureServices` 中可以通过 `context.Services` 注册服务。
- 在调用 `BuildServiceProvider()` / `builder.Build()` 之前调用 `AddModule`。
- 框架内部会构建一个临时 `ServiceProvider` 用于实例化模块，`AddModule` 返回时释放；如需跨模块共享实例，请在最终容器解析。

## 常见问题

- 入口模块必须实现 `IModule`，否则抛出 `TypeLoadException`。
- 若提示源生成器没有生成注册计划：确认模块类定义在当前编译或可从源码模块/`AddModule` 调用点到达，并且程序集引用了 Maomi.Core.Aot 包；不要与其他 `Maomi.*` 模块框架包混用。
- 库项目中引用 Maomi.Core.Aot 时，生成器也会运行并生成该库自身的注册数据；应用端编译时会重新生成覆盖全依赖图的注册，两者数据一致。
- Scoped 服务不要注册为 Singleton；使用键控服务时，消费端必须按对应 key 解析。
