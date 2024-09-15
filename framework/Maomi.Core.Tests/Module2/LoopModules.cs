using Maomi.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maomi.Core.Tests.Module2
{
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
}
