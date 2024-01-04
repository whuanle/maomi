using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace Demo2.MyLogger.Console
{
	internal class Program
	{
		static void Main(string[] args)
		{
			using ILoggerFactory factory = LoggerFactory.Create(builder =>
			{
				builder.AddConsole();
				builder.AddMyConsoleLogger(options =>
				{
					options.DefaultLevel = LogLevel.Debug;
				});
			});
			ILogger logger1 = factory.CreateLogger("Program");
			ILogger logger2 = factory.CreateLogger<Program>();

			logger1.LogError(new Exception("报错了"), message: "Hello World! Logging is {Description}.", args: "error");
			logger2.LogError(new Exception("报错了"), message: "Hello World! Logging is {Description}.", args: "error");
		}
	}
}