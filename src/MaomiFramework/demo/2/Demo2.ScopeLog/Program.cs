using Serilog;
using Microsoft.Extensions.Logging;

public class Program
{
    static void Main()
    {
        var logger = GetLogger();
        using (logger.BeginScope("Checking mail"))
        {
            // Scope is "Checking mail"
            logger.LogInformation("Opening SMTP connection");

            using (logger.BeginScope("Downloading messages"))
            {
                // Scope is "Checking mail" -> "Downloading messages"
                logger.LogError("Connection interrupted");
            }
        }
    }

    private static Microsoft.Extensions.Logging.ILogger GetLogger()
	{
		const string LogTemplate = "{SourceContext} {Timestamp:HH:mm} [{Level}] (ThreadId:{ThreadId}) {Message}{NewLine}{Exception} {Scope}";
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
            .CreateLogger();

		// Serilog.ILogger 转换为 Microsoft.Extensions.Logging.ILogger
		ILoggerFactory factory = new LoggerFactory().AddSerilog(logger);
		return factory.CreateLogger<Program>();
    }
}