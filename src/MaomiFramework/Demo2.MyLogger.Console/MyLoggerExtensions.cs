using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Demo2.MyLogger.Console
{
	public static class MyLoggerExtensions
	{
		public static ILoggingBuilder AddMyConsoleLogger(
			this ILoggingBuilder builder, Action<MyLoggerOptions> action)
		{
			MyLoggerOptions options = new();
			if (action != null)
			{
				action.Invoke(options);
			}

			builder.AddConfiguration();
			builder.Services.TryAddEnumerable(
				ServiceDescriptor.Singleton<ILoggerProvider>(new MyLoggerProvider(options)));
			return builder;
		}
	}
}