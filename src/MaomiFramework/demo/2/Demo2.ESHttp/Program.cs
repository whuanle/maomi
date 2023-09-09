using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace Demo2.ESTrace
{
    public static class Program
    {
        private static readonly HttpClient Http = new();
        public static async Task Main(string[] args)
        {
            // 由 CLR 自动调用
            HttpClientEventListener listener = new ();

            Console.WriteLine("活动ID ---- 事件名称 ---- 请求地址 ---- 协议");
            while (true)
            {
                await GetAsync();
                await Task.Delay(1000);
            }
        }

        static async Task GetAsync()
        {
            await Http.GetAsync("https://www.baidu.com");
        }
    }

    sealed class HttpClientEventListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            switch (eventSource.Name)
            {
                case "System.Net.Http":
                    EnableEvents(eventSource, EventLevel.Informational, EventKeywords.All);
                    break;
            }

            base.OnEventSourceCreated(eventSource);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            // RequestStart 事件
            if (eventData.EventId == 1)
            {
                var scheme = (string)eventData.Payload[0];
                var host = (string)eventData.Payload[1];
                var port = (int)eventData.Payload[2];
                var pathAndQuery = (string)eventData.Payload[3];
                var versionMajor = (byte)eventData.Payload[4];
                var versionMinor = (byte)eventData.Payload[5];
                var policy = (HttpVersionPolicy)eventData.Payload[6];

                Console.WriteLine($"{eventData.ActivityId} {eventData.EventName} {scheme}://{host}:{port}{pathAndQuery} HTTP/{versionMajor}.{versionMinor}");
            }
            // RequestStop 事件
            else if (eventData.EventId == 2)
            {
                Console.WriteLine($"{eventData.ActivityId} {eventData.EventName} 状态码：{eventData.Payload[0]}");
            }
        }
    }
}