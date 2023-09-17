using Microsoft.Diagnostics.NETCore.Client;
using System.Diagnostics;

public class Program
{
    static async Task Main()
    {
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

        var client = new DiagnosticsClient(pid);
        await client.WriteDumpAsync(
            dumpType: DumpType.Full,
            dumpPath: $"D:/{pid}_{DateTime.Now.Ticks}.dmp",
            logDumpGeneration: true,
            token: CancellationToken.None
        );
    }
}