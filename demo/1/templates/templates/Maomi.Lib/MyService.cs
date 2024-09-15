using Maomi.Module;
using Microsoft.Extensions.DependencyInjection;

namespace MaomiLib
{
	[InjectOn(ServiceLifetime.Scoped)]
	public class MyService : IMyService
	{
		public int Sum(int a, int b)
		{
			return a + b;
		}
	}
}
