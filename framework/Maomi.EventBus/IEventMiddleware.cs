namespace Maomi.EventBus;

// 定义事件委托，用于构建执行链
public delegate Task EventHandlerDelegate();

// 带依赖注入的事件委托，用于构建执行链
internal delegate Task ServiceEventHandlerDelegate(IServiceProvider provider, params object?[] parameters);

// 事件执行中间件，即执行事件时的拦截器
public interface IEventMiddleware<in TEvent>
where TEvent : IEvent
{
    // @event: 事件
    // next: 下一个要执行的函数
    Task HandleAsync(TEvent @event, EventHandlerDelegate next);
}
