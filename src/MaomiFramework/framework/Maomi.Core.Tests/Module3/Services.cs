using Maomi.Module;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maomi.Core.Tests.Module3
{
	public interface IA { }
	public interface IB { }
	public interface IC { }


	public class ParentService { }

	[InjectOn]
	public class Service_Interfaces : ParentService, IA, IB, IC { }


	public interface IScoped { }
	[InjectOn(lifetime: ServiceLifetime.Scoped)]
	public class Service_Scoped : IScoped { }

	public interface ISingleton { }
	[InjectOn(lifetime: ServiceLifetime.Singleton)]
	public class Service_Singleton : ISingleton { }


	[InjectOn(scheme: InjectScheme.OnlyBaseClass)]
	public class Service_OnlyBaseClass : ParentService, IA, IB, IC { }


	public class Any { }
	public interface AnyA { }
	public interface AnyB { }
	public interface AnyC { }
	[InjectOn(scheme: InjectScheme.Any)]
	public class Service_Any : Any, AnyA, AnyB, AnyC { }


	public class Some { }
	public interface SomeA { }
	public interface SomeB { }
	public interface SomeC { }

	[InjectOn(scheme: InjectScheme.Some, ServicesType = new Type[] { typeof(SomeB) })]
	public class Service_Some : Some, SomeA, SomeB, SomeC { }


	[InjectOn(scheme: InjectScheme.None, Own = true)]
	public class Service_Own : ParentService, IA, IB, IC { }
}
