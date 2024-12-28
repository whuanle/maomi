namespace Maomi.EventBus;

/// <summary>
/// 事件模型类，作为事件的参数使用
/// </summary>
public abstract record Event : IEvent
{
    private Guid _eventId;
    private DateTime _creationTime;

    protected Event() : this(Guid.NewGuid(), DateTime.UtcNow) { }

    protected Event(Guid eventId, DateTime creationTime)
    {
        _eventId = eventId;
        _creationTime = creationTime;
    }

    /// <summary>
    /// 事件 id
    /// </summary>
    /// <returns></returns>
    public Guid GetEventId() => _eventId;

    /// <summary>
    /// 设置事件 id
    /// </summary>
    /// <param name="eventId"></param>
    public void SetEventId(Guid eventId) => _eventId = eventId;

    /// <summary>
    /// 事件创建时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetCreationTime() => _creationTime;

    /// <summary>
    /// 设置时间创建时间
    /// </summary>
    /// <param name="creationTime"></param>
    public void SetCreationTime(DateTime creationTime) => _creationTime = creationTime;
}
