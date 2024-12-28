namespace Maomi.Core.Tests.Module2;

// C=>B=>A=>C
[InjectModule<CLoopModule>]
	public class ALoopModule : IModule
	{
		public void ConfigureServices(ServiceContext context)
		{
		}
	}

	[InjectModule<ALoopModule>]
	public class BLoopModule : IModule
	{
		public void ConfigureServices(ServiceContext context)
		{
		}
	}
	[InjectModule<BLoopModule>]
	public class CLoopModule : IModule
	{
		public void ConfigureServices(ServiceContext context)
		{
		}
	}
