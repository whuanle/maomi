using Maomi;
using MaomiLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
	static void Main()
	{
		var services = new ServiceCollection();
		services.AddModule<ConsoleModule>();
		var ioc = services.BuildServiceProvider();

		var service = ioc.GetRequiredService<IMyService>();
		Console.WriteLine(service.Sum(1, 2));
	}
}
