namespace Maomi.EventBus
{
    // 简化事件的实现，通过事件传递参数
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

        public Guid GetEventId() => _eventId;

        public void SetEventId(Guid eventId) => _eventId = eventId;

        public DateTime GetCreationTime() => _creationTime;

        public void SetCreationTime(DateTime creationTime) => _creationTime = creationTime;
    }

}
