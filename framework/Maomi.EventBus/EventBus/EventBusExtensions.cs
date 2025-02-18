using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Maomi.EventBus;

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
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    // 使用 LINQ 简化属性检查
                    if (type.GetCustomAttributes(typeof(EventAttribute), inherit: false).Length != 0)
                    {
                        GetEventHandler(services, type);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.Error.WriteLine("无法加载某些类型: " + ex.Message);
            }
        }
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
            // 提前检查 EventHandlerAttribute 是否存在
            var attr = method.GetCustomAttribute<EventHandlerAttribute>();
            if (attr == null)
            {
                continue;
            }

            // 检查方法参数数量
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                throw new ArgumentException($"{method.Name} 的定义不正确，至少包含一个参数");
            }

            // 检查第一个参数是否为事件类型
            var eventType = parameters[0].ParameterType;
            bool isValidEventType = eventType.IsSubclassOf(typeof(Event)) ||
                                    eventType.GetInterfaces().Any(i => i == typeof(IEvent));
            if (!isValidEventType)
            {
                throw new ArgumentException($"{method.Name} 的定义不正确，第一个参数必须为事件");
            }

            // 根据 IsCancel 属性决定如何注册事件处理器
            if (!attr.IsCancel)
            {
                EventBus.AddEventHandler(type, attr.Order, eventType, method);
            }
            else
            {
                EventBus.AddCancelEventHandler(type, attr.Order, eventType, method);
            }
        }
    }
}
