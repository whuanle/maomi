namespace Maomi.EventBus.Tests
{
	[Event]
	public class CancelCheckImageCodeEventHandler
	{
		private readonly EventStats _stats;
		public CancelCheckImageCodeEventHandler(EventStats eventStats)
		{
			_stats = eventStats;
		}

		[EventHandler]
		public void Check(CancelMyEvent @event)
		{
			_stats.Names.Add(typeof(CancelCheckImageCodeEventHandler).Name);
		}
	}
}