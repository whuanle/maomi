using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

public class Program
{
	static void Main()
    {
        IConfiguration configuration = new ConfigurationBuilder()
                                 .SetBasePath(AppContext.BaseDirectory)
                                 .AddJsonFile(path: "serilog.json", optional: true, reloadOnChange: true)
                                 .Build();
        if (configuration == null)
        {
            throw new ArgumentNullException($"未能找到 serilog.json 日志配置文件");
        }

        var loggerBuilder = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Console(new CompactJsonFormatter())
            .CreateLogger();

        var services = new ServiceCollection();
		services.AddLogging(s =>
		{
			s.AddSerilog(loggerBuilder);
		});

        var ioc = services.BuildServiceProvider();

        var logger = ioc.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Test log {@Message}",new Dictionary<string, string>() { { "A","1"},{ "B","2"} });
	}
}