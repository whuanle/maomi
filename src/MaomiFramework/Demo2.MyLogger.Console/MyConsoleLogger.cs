using Microsoft.Extensions.Logging;

namespace Demo2.MyLogger.Console
{
	/// <summary>
	/// 自定义的日志记录器
	/// </summary>
	public class MyConsoleLogger : ILogger
	{
		// 日志名称
		private readonly string _name;
		private readonly MyLoggerOptions _options;

		public MyConsoleLogger(string name, MyLoggerOptions options)
		{
			_name = name;
			_options = options;
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

		// 判断是否启用日志等级
		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel >= _options.DefaultLevel;
		}

		// 记录日志，formatter 由框架提供的字符串格式化器
		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}
			if (logLevel == LogLevel.Critical)
			{
				System.Console.ForegroundColor = System.ConsoleColor.Red;
				System.Console.WriteLine($"[{logLevel}] {_name} {formatter(state, exception)} {exception}");
				System.Console.ResetColor();
			}
			else if (logLevel == LogLevel.Error)
			{
				System.Console.ForegroundColor = System.ConsoleColor.DarkRed;
				System.Console.WriteLine($"[{logLevel}] {_name} {formatter(state, exception)} {exception}");
				System.Console.ResetColor();
			}
			else
			{
				System.Console.WriteLine($"[{logLevel}] {_name} {formatter(state, exception)} {exception}");
			}
		}
	}
}