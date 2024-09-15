using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Maomi.EventBus
{
	public static class EventBusExtensions
	{
		// 添加事件总线扩展
		public static void AddEventBus(this IServiceCollection services, Type? middleware = null)
		{
			services.AddScoped<IEventBus, EventBus>();
			if (middleware is not null)
			{
				EventBus.SetMiddleware(middleware);
				services.TryAddEnumerable(new ServiceDescriptor(typeof(IEventMiddleware<>), middleware, lifetime: ServiceLifetime.Transient));
			}

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				foreach (var type in assembly.GetTypes())
				{
					if (type.CustomAttributes.Any(x => x.AttributeType == typeof(EventAttribute)))
					{
						GetEventHandler(services, type);
					}
				}
			}
		}

		// 扫描类中的执行器
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void GetEventHandler(IServiceCollection services, Type type)
		{
			services.AddScoped(type);

			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
			foreach (var method in methods)
			{
				var attr = method.GetCustomAttribute<EventHandlerAttribute>();
				if (attr == null) return;

				var parameters = method.GetParameters();
				if (parameters.Length == 0) throw new Exception($"{method.Name} 的定义不正确，至少包含一个参数");
				var eventType = parameters[0].ParameterType;
				if (!(eventType.IsSubclassOf(typeof(Event)) || eventType.GetInterface(typeof(IEvent).Name) != null)) 
					throw new Exception($"{method.Name} 的定义不正确，第一个参数必须为事件");

				if (!attr.IsCancel) EventBus.AddEventHandler(type, attr.Order, eventType, method);
				else EventBus.AddCancelEventHandler(type, attr.Order, eventType, method);
			}
		}
	}
}
