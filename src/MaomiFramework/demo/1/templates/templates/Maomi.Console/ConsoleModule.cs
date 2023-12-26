using Maomi.Module;
using MaomiLib;

[InjectModule<LibModule>()]
public class ConsoleModule : IModule
{
	public void ConfigureServices(ServiceContext context)
	{
	}
}