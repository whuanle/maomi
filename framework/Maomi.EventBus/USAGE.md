# Maomi.EventBus 使用指南

## 用途与环境

`Maomi.EventBus` 提供进程内异步事件发布、按顺序执行处理器以及可选中间件。支持 `net8.0`、`net9.0`、`net10.0`。

```bash
dotnet add package Maomi.EventBus --version 2.3.0
```

应用需有 Microsoft DI 与日志服务（`EventBus` 使用 `ILogger<EventBus>`）。事件需实现 `IEvent`；通常继承 `Event`，它会自动生成事件 ID 和 UTC 创建时间。

## 定义、发现、发布

给含处理方法的类标 `[Event]`，给公开实例方法标 `[EventHandler]`。调用 `AddEventBus()` 注册总线并扫描当前已加载程序集，然后从作用域解析 `IEventBus`：

```csharp
using Maomi.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
var services = new ServiceCollection();
services.AddLogging();
services.AddEventBus();
using var provider = services.BuildServiceProvider();
await using var scope = provider.CreateAsyncScope();
await scope.ServiceProvider.GetRequiredService<IEventBus>()
    .PublishAsync(new UserCreated(Guid.NewGuid()));

public sealed record UserCreated(Guid UserId) : Event;

[Event]
public sealed class WelcomeHandler
{
    [EventHandler(Order = 10)]
    public Task Handle(UserCreated @event, CancellationToken cancellationToken)
    {
        Console.WriteLine($"欢迎用户 {@event.UserId}");
        return Task.CompletedTask;
    }
}
```

也可直接写 `PublishAsync(event)`；其 `CancellationToken` 参数可取消尚未开始执行的后续处理器。处理器可返回 `void` 或 `Task`，第一个参数必须是事件，后续参数由发布时传入（当前总线传入 `CancellationToken`）。处理器类按 Scoped 注册。

## 顺序、中间件与错误

- `Order` 数值升序执行；不同处理器应使用不同顺序值以确保确定顺序。
- 可为同一事件类型和顺序配置补偿处理器：在处理方法上加 `[EventHandler(Order = 10, IsCancel = true)]`。处理器抛出异常后，总线尝试从失败位置向前逆序执行补偿器，然后重新抛出异常。当前回退实现会以失败处理器实例调用补偿方法；为避免跨类目标类型不匹配，同一事件需要回退的处理器和补偿器应放在同一处理器类中。
- `AddEventBus(typeof(TraceMiddleware<>))` 可选注册通用中间件；传入类型需是实现 `IEventMiddleware<TEvent>` 的开放泛型类。中间件可在调用 `next()` 前后执行逻辑；若不调用 `next()`，事件处理链不会运行。

```csharp
public sealed class TraceMiddleware<TEvent> : IEventMiddleware<TEvent>
    where TEvent : IEvent
{
    public async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
    {
        await next();
    }
}
```

```csharp
services.AddEventBus(typeof(TraceMiddleware<>));
```

## 常见问题

- `[Event]` 标在处理器类，而不是事件类型；处理方法必须是公开实例方法。
- 注册 `AddEventBus()` 前确保处理器程序集已加载。无效签名（无参数、首参不是事件、返回类型不是 `void` / `Task`）会在扫描或构建时失败。
- 取消 token 会在每个处理器开始前检查并停止后续处理器，不会触发补偿器；正在执行的处理器需自行使用收到的 token 配合取消。补偿器仅用于处理器抛异常的回退路径。
- `[EventHandler(IsCancel = true)]` 必须与对应处理器的事件类型和 `Order` 一致。补偿器本身失败时异常会直接传播。
- 中间件类型保存在静态配置中；在一个进程内使用不同 `AddEventBus` 中间件配置会相互影响。
