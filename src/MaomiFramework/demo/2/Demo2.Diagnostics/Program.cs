using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace Demo2.Diagnostics
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 获取所有 .NET 进程
            var processes = DiagnosticsClient.GetPublishedProcesses()
                .Select(Process.GetProcessById)
                .Where(process => process != null);
            Console.WriteLine("请输入进程 id");
            foreach (var item in processes)
            {
                Console.WriteLine($"{item.Id} ------ {item.ProcessName}");
            }

            var read = Console.ReadLine();
            ArgumentNullException.ThrowIfNullOrEmpty(read);
            var pid = int.Parse(read);

            var providers = new List<EventPipeProvider>()
            {
                new ("Microsoft-Windows-DotNETRuntime", EventLevel.Informational, (long)ClrTraceEventParser.Keywords.GC),
            };

            var client = new DiagnosticsClient(pid);

            using var session = client.StartEventPipeSession(providers: providers, requestRundown: false, circularBufferMB: 256);
            var source = new EventPipeEventSource(session.EventStream);

            // CLR 事件
            source.Clr.All += (TraceEvent obj) =>
            {
                Console.WriteLine(obj.ToString());
            };

            // 订阅 providers 中监听的所有事件
            // 如果想订阅全部事件，则应该则使用 Dynamic.All
            //source.AllEvents += (TraceEvent obj) =>
            //{
            //    Console.WriteLine(obj.ToString());
            //};

            // 内核事件
            //source.Kernel.All += (TraceEvent obj) =>
            //{
            //    Console.WriteLine(obj.ToString());
            //};

            // 动态处理所有事件
            //source.Dynamic.All += (TraceEvent obj) =>
            //{
            //    Console.WriteLine(obj.ToString());
            //};

            // 通常在 Debug 下使用，
            // 当一个事件没有被订阅处理时，将会使用此事件处理
            //source.UnhandledEvents += (TraceEvent obj) =>
            //{
            //    Console.WriteLine(obj.ToString());
            //};


            try
            {
                source.Process();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}