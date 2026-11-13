# Maomi.EventBus 使用指南

`Maomi.EventBus` 是进程内异步事件总线：扫描带特性的处理器，按顺序执行，可插入通用中间件，并可为失败处理器配置补偿处理器。它不是消息队列，不提供跨进程传输或持久化。

- NuGet：`dotnet add package Maomi.EventBus --version 2.4.0`
- 目标框架：`net8.0`、`net9.0`、`net10.0`
- 命名空间：`Maomi.EventBus`

应用需有 Microsoft DI 与日志服务（`EventBus` 使用 `ILogger<EventBus>`）。事件需实现 `IEvent`；通常继承 `Event`，它会自动生成事件 ID 和 UTC 创建时间。

## 定义、发现、发布

给含处理方法的类标 `[Event]`，给公开实例方法标 `[EventHandler]`。在处理器程序集加载后调用 `AddEventBus()`；它扫描当时已加载的程序集。再从 DI scope 解析 `IEventBus` 并发布事件：

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

也可直接写 `PublishAsync(event)`；其 `CancellationToken` 参数可取消尚未开始执行的后续处理器。处理器可返回 `void` 或 `Task`，第一个参数必须是事件；若方法还有参数，需与总线传入的 `CancellationToken` 匹配。处理器类按 Scoped 注册。

## 顺序、中间件与错误

- `Order` 数值升序执行；为同一事件的不同处理器指定不同顺序值，避免依赖相同顺序下的未定义先后。
- 可为同一事件类型和顺序配置补偿处理器：在处理方法上加 `[EventHandler(Order = 10, IsCancel = true)]`。处理器抛出异常后，总线尝试从失败位置向前逆序执行补偿器，然后重新抛出异常。
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

## 接入注意事项与常见问题

- `[Event]` 标在处理器类，而不是事件类型；处理方法必须是公开实例方法。
- 注册 `AddEventBus()` 前确保处理器程序集已加载。无效签名（无参数、首参不是事件、返回类型不是 `void` / `Task`）会在扫描或构建时失败。
- 取消 token 会在每个处理器开始前检查并停止后续处理器，不会触发补偿器；正在执行的处理器需自行使用收到的 token 配合取消。补偿器仅用于处理器抛异常的回退路径。
- `[EventHandler(IsCancel = true)]` 必须与对应处理器的事件类型和 `Order` 一致。补偿器本身失败时异常会直接传播。
- 当前回退逻辑使用失败处理器实例调用回退方法；需要补偿的处理器和补偿方法应放在同一个处理器类中，避免类型不匹配。
- 中间件类型和事件处理器缓存是进程级静态配置；不要在同一进程为多个容器分别设置不同 `AddEventBus` 中间件，也不要把此总线当作具有持久化、重试队列或跨实例投递能力的消息系统。
