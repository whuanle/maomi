namespace Maomi.EventBus
{
    // 标识类中有事件执行器
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EventAttribute : Attribute { }

    // 标识方法是一个事件执行器
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class EventHandlerAttribute : Attribute
    {
        // 事件排序
        public int Order { get; set; } = 0;

        // 是否为撤销事件
        public bool IsCancel { get; set; } = false;
    }
}
