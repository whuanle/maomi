using Microsoft.Extensions.Logging;

namespace Demo2.MyLogger.Console
{
	public class MyLoggerOptions
	{
		/// <summary>
		/// 最小日志等级
		/// </summary>
		public LogLevel DefaultLevel { get; set; } = LogLevel.Debug;
	}
}