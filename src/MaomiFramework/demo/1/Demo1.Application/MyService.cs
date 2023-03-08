using Maomi.Module;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo1.Application
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
