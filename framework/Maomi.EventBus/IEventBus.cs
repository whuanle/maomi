namespace Maomi.EventBus;

// 事件总线服务
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    where TEvent : IEvent;
}
