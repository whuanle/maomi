using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Xml.Linq;

namespace Demo2.ES
{
    internal class Program
    {
        private static readonly MyEventSource EventSource = new MyEventSource();
        public static void Main(string[] args)
        {
            int number = 0;
            while (true)
            {
                number++;
                EventSource.LogEvent("测试", number);
                Thread.Sleep(1000);
            }
        }
    }

    [EventSource(Name = "MyEvent")]
    public class MyEventSource : EventSource
    {
        // 计数器
        private readonly IncrementingEventCounter _incrementingEventCounter;
        public MyEventSource()
        {
            _incrementingEventCounter = new IncrementingEventCounter("MyEvent", this);
        }

        [Event(eventId: 1)]
        public void LogEvent(string message, int favoriteNumber)
        {
            _incrementingEventCounter.Increment();
            WriteEvent(1, message, favoriteNumber);
        }
    }
}