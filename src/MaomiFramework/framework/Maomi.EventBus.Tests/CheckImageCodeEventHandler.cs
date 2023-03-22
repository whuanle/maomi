namespace Maomi.EventBus.Tests
{
	[Event]
	public class CheckImageCodeEventHandler
	{
		private readonly EventStats _stats;
		public CheckImageCodeEventHandler(EventStats eventStats)
		{
			_stats = eventStats;
		}

		[EventHandler]
		public void Check(MyEvent @event)
		{
			_stats.Names.Add(typeof(CheckImageCodeEventHandler).Name);
		}
	}
}