using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Demo3.Options
{
    public class TestOptions
    {
        public string Title { get; set; }
        public string Name { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("test.json", optional: true, reloadOnChange: true)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            services.AddOptions<TestOptions>().Bind(configuration);
            // services.Configure<TestOptions>(name: "", configuration);
            // 或者使用 Microsoft.Extensions.Options.ConfigurationExtensions 包
            // services.Configure<TestOptions>(configuration);

            var ioc = services.BuildServiceProvider();
            var to1 = ioc.GetRequiredService<IOptions<TestOptions>>();
            var to2 = ioc.GetRequiredService<IOptionsSnapshot<TestOptions>>();
            var to3 = ioc.GetRequiredService<IOptionsMonitor<TestOptions>>();
            to3.OnChange(s =>
            {
                Console.WriteLine($"变更之前的值: {s.Name}");
            });
            while (true)
            {
                Console.WriteLine($"IOptions: {to1.Value.Name}");
                Console.WriteLine($"IOptionsSnapshot: {to2.Value.Name}");
                Console.WriteLine($"IOptionsMonitor: {to3.CurrentValue.Name}");
                Thread.Sleep(1000);
            }
        }
    }
}