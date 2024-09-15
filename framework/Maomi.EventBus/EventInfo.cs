using System.Reflection;

namespace Maomi.EventBus
{
	// 用来记录一个 Handler 
	internal class EventInfo
	{
		// 执行器所在的类
		public Type DeclaringType { get; set; }
		// 执行序号
		public int Order { get; set; }
		// 事件
		public Type EventType { get; set; }
		// 执行器方法
		public MethodInfo MethodInfo { get; set; }
		// 委托封装的执行器方法
		public TaskInvokeDelegate TaskInvoke { get; set; }
        // 撤销时执行
        public bool IsCancel { get; set; }
		// 撤销执行器对应的信息
        public EventInfo? CancelInfo { get; set; }

		public override int GetHashCode()
		{
			return MethodInfo.GetHashCode();
		}
		public override bool Equals(object? obj)
		{
			if (obj is not EventInfo info) return false;
			return this.GetHashCode() == info.GetHashCode();
		}
	}
}
