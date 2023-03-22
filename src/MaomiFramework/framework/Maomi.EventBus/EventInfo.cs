using System.Reflection;

namespace Maomi.EventBus
{
	// 用来记录一个 Handler 
	internal class EventInfo
	{
		public Type DeclaringType { get; set; }
		public int Order { get; set; }
		public bool IsCancel { get; set; }
		public Type EventType { get; set; }
		public MethodInfo MethodInfo { get; set; }

		public TaskInvokeDelegate TaskInvoke { get; set; }

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
