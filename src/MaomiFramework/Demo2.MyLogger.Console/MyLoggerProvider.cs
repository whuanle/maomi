using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Demo2.MyLogger.Console
{
	[ProviderAlias("MyConsole")]
	public sealed class MyLoggerProvider : ILoggerProvider
	{
		private MyLoggerOptions _options;
		private readonly ConcurrentDictionary<string, MyConsoleLogger> _loggers =
			new(StringComparer.OrdinalIgnoreCase);

		public MyLoggerProvider(MyLoggerOptions options)
		{
			_options = options;
		}

		public ILogger CreateLogger(string categoryName) =>
			_loggers.GetOrAdd(categoryName, name => new MyConsoleLogger(name, _options));

		public void Dispose()
		{
			_loggers.Clear();
		}
	}
}