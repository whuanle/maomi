namespace Maomi.EventBus.Tests
{
	[Event]
	public class UserRegisterEventHandler
	{
		private readonly EventStats _stats;
		private readonly SetException _setException;
		public UserRegisterEventHandler(EventStats eventStats, SetException setException)
		{
			_stats = eventStats;
			_setException = setException;
		}

		[EventHandler(Order = 1)]
		public void InsertDb(MyEvent @event)
		{
			_stats.Names.Add(nameof(UserRegisterEventHandler.InsertDb));
			if (_setException.Node == 1)
				throw new Exception("× 写入用户信息到数据库失败");
			else Console.WriteLine("√ 用户信息已添加到数据库");

		}

		[EventHandler(Order = 1, IsCancel = true)]
		public void CancelInsertDb(MyEvent @event)
		{
			_stats.Names.Add(nameof(UserRegisterEventHandler.CancelInsertDb));
			Console.WriteLine("注册失败，刷新验证码");
		}

		[EventHandler(Order = 2)]
		public void InitUser(MyEvent @event)
		{
			_stats.Names.Add(nameof(UserRegisterEventHandler.InitUser));
			if (_setException.Node == 2)
				throw new Exception("× 初始化用户数据失败");
			else Console.WriteLine("√ 初始化用户数据，系统生成默认用户权限、数据");

		}

		[EventHandler(Order = 2, IsCancel = true)]
		public void CancelInitUser(MyEvent @event)
		{
			_stats.Names.Add(nameof(UserRegisterEventHandler.CancelInitUser));
			Console.WriteLine("撤销用户注册信息");
		}

		[EventHandler(Order = 3)]
		public void SendEmail(MyEvent @event)
		{
			_stats.Names.Add(nameof(UserRegisterEventHandler.SendEmail));
			if (_setException.Node == 3)
				throw new Exception("× 发送验证邮件失败");
			else Console.WriteLine("√ 发送验证邮件成功");
		}

		[EventHandler(Order = 3, IsCancel = true)]
		public void CancelSendEmail(MyEvent @event)
		{
			_stats.Names.Add(nameof(UserRegisterEventHandler.CancelSendEmail));
			Console.WriteLine("× 撤销初始化用户数据");
		}
	}
}