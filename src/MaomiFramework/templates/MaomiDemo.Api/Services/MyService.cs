using Maomi.Module;

namespace MaomiDemo.Api.Services
{
	[InjectOn(ServiceLifetime.Scoped, Own = true)]
	public class MyService : IMyService
	{
		public int Sum(int a, int b)
		{
			return a + b;
		}
	}
}
