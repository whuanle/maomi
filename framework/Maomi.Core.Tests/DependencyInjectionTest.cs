using Maomi.Core.Tests.Module3;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maomi.Core.Tests
{
	public class DependencyInjectionTest
	{
		[Fact]
		public void Default_Inject()
		{
			var ioc = new ServiceCollection();
			ioc.AddModule<ServiceModule>();
			var services = ioc.BuildServiceProvider();
			var s1 = services.GetRequiredService<ParentService>();
			Assert.NotEqual(typeof(Service_Interfaces), s1.GetType());
			var sa = services.GetRequiredService<IA>();
			var sb = services.GetRequiredService<IB>();
			var sc = services.GetRequiredService<IC>();
			Assert.Equal(typeof(Service_Interfaces), sa.GetType());
			Assert.Equal(typeof(Service_Interfaces), sb.GetType());
			Assert.Equal(typeof(Service_Interfaces), sc.GetType());
		}

		[Fact]
		public void Inject_Scoped()
		{
			var ioc = new ServiceCollection();
			ioc.AddModule<ServiceModule>();

			var provider1 = ioc.BuildServiceProvider();
			var s1 = provider1.GetService<IScoped>();

			var provider2 = provider1.CreateScope().ServiceProvider;
			var s2 = provider2.GetService<IScoped>();
			var s3 = provider2.GetService<IScoped>();

			Assert.NotEqual(s1, s2);
			Assert.NotEqual(s1, s3);
			Assert.Equal(s2, s3);
		}

		[Fact]
		public void Inject_Singleton()
		{
			var ioc = new ServiceCollection();
			ioc.AddModule<ServiceModule>();

			var provider1 = ioc.BuildServiceProvider();
			var s1 = provider1.GetService<ISingleton>();

			var provider2 = provider1.CreateScope().ServiceProvider;
			var s2 = provider2.GetService<ISingleton>();

			var provider3 = provider1.CreateScope().ServiceProvider;
			var s3 = provider3.GetService<ISingleton>();


			Assert.Equal(s1, s2);
			Assert.Equal(s1, s3);
			Assert.Equal(s2, s3);
		}

		[Fact]
		public void Inject_OnlyBaseClass()
		{
			var ioc = new ServiceCollection();
			ioc.AddModule<ServiceModule>();

			var services = ioc.BuildServiceProvider();
			var s1 = services.GetRequiredService<ParentService>();

			Assert.Equal(typeof(Service_OnlyBaseClass), s1.GetType());

			var s2 = services.GetRequiredService<IA>();
			var s3 = services.GetRequiredService<IB>();

			Assert.NotEqual(typeof(Service_OnlyBaseClass), s2.GetType());
			Assert.NotEqual(typeof(Service_OnlyBaseClass), s3.GetType());
		}

		[Fact]
		public void Inject_Any()
		{
			var ioc = new ServiceCollection();
			ioc.AddModule<ServiceModule>();

			var services = ioc.BuildServiceProvider();
			var s1 = services.GetRequiredService<Any>();
			var s2 = services.GetRequiredService<AnyA>();
			var s3 = services.GetRequiredService<AnyB>();
			var s4 = services.GetRequiredService<AnyC>();

			Assert.Equal(typeof(Service_Any), s1.GetType());
			Assert.Equal(typeof(Service_Any), s2.GetType());
			Assert.Equal(typeof(Service_Any), s3.GetType());
			Assert.Equal(typeof(Service_Any), s4.GetType());
		}

		[Fact]
		public void Inject_Some()
		{
			var ioc = new ServiceCollection();
			ioc.AddModule<ServiceModule>();

			var services = ioc.BuildServiceProvider();
			var s1 = services.GetService<Some>();
			var s2 = services.GetService<SomeA>();
			var s3 = services.GetService<SomeB>();
			var s4 = services.GetService<SomeC>();

			Assert.Null(s1);
			Assert.Null(s2);
			Assert.NotNull(s3);
			Assert.Null(s4);
		}

		[Fact]
		public void Inject_Own()
		{
			var ioc = new ServiceCollection();
			ioc.AddModule<ServiceModule>();

			var services = ioc.BuildServiceProvider();
			var s1 = services.GetRequiredService<ParentService>();
			var s2 = services.GetRequiredService<IA>();
			var s3 = services.GetRequiredService<IB>();
			var s4 = services.GetRequiredService<IC>();
			var s5 = services.GetRequiredService<Service_Own>();

			Assert.NotEqual(typeof(Service_Own), s1.GetType());
			Assert.NotEqual(typeof(Service_Own), s2.GetType());
			Assert.NotEqual(typeof(Service_Own), s3.GetType());
			Assert.NotEqual(typeof(Service_Own), s4.GetType());
			Assert.Equal(typeof(Service_Own), s5.GetType());
		}
	}
}
