namespace Maomi.EventBus.Tests
{
	public record class MyEvent : Event
	{
		public string Name { get; set; }
		public string Book { get; set; }
		public override string ToString()
		{
			return $"{Name} ，书名：{Book}";
		}
	}

	public record class CancelMyEvent : Event
	{
		public string Name { get; set; }
		public string Book { get; set; }
		public override string ToString()
		{
			return $"{Name} ，书名：{Book}";
		}
	}
}