using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

public class Program
{
	private static Serilog.ILogger GetJsonLogger()
	{
		IConfiguration configuration = new ConfigurationBuilder()
								 .SetBasePath(AppContext.BaseDirectory)
								 .AddJsonFile(path: "serilog.json", optional: true, reloadOnChange: true)
								 .Build();
		if (configuration == null)
		{
			throw new ArgumentNullException($"未能找到 serilog.json 日志配置文件");
		}
		var logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration)
			.CreateLogger();
		return logger;
	}

	private static Serilog.ILogger GetLogger()
	{
		const string LogTemplate = "{SourceContext} {Scope} {Timestamp:HH:mm} [{Level}] {Message:lj} {Properties:j} {NewLine}{Exception}";
		var logger = new LoggerConfiguration()
			.Enrich.WithMachineName()
			.Enrich.WithThreadId()
			.Enrich.FromLogContext()
#if DEBUG
			.MinimumLevel.Debug()
#else
		                .MinimumLevel.Information()
#endif
			.WriteTo.Console(outputTemplate: LogTemplate)
			.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, outputTemplate: LogTemplate)
			.CreateLogger();
		return logger;
	}

	private static Microsoft.Extensions.Logging.ILogger InjectLogger()
	{
		var logger = GetJsonLogger();
		var ioc = new ServiceCollection();
		ioc.AddLogging(builder => builder.AddSerilog(logger: logger, dispose: true));
		var loggerProvider = ioc.BuildServiceProvider().GetRequiredService<ILoggerProvider>();
		return loggerProvider.CreateLogger("Program");
	}

	static void Main()
	{
		var log1 = GetLogger();
		log1.Debug("溪源More、痴者工良");
		var log2 = GetJsonLogger();
		log2.Debug("溪源More、痴者工良");
		var log3 = InjectLogger();
		log3.LogDebug("溪源More、痴者工良");
	}
}