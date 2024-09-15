namespace Maomi.EventBus
{
    /// <summary>
    /// 标识类中有事件执行器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EventAttribute : Attribute { }

    /// <summary>
    /// 标识方法是一个事件执行器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class EventHandlerAttribute : Attribute
    {
        /// <summary>
        /// 事件排序
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// 是否为补偿事件
        /// </summary>
        public bool IsCancel { get; set; } = false;
    }
}
