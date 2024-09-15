using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Maomi.EventBus
{
	public partial class EventBus
	{
		#region static

		// 拦截器
		private static Type? Middleware;
		// 缓存所有事件执行器
		private static readonly Dictionary<Type, HashSet<EventInfo>> EventCache = new();
		// 调用链缓存
		private static readonly Dictionary<Type, ServiceEventHandlerDelegate> HandlerDelegateCache = new();

		// 设置拦截器
		public static void SetMiddleware(Type type)
		{
			Middleware = type;
		}

		// 给一个事件添加执行器
		public static void AddEventHandler(
			Type declaringType, // 执行器方法所在的类
			int order,
			Type eventType,     // 绑定了哪个事件 
			MethodInfo method)  // 执行器方法
		{
			if (!EventCache.TryGetValue(eventType, out var events))
			{
				events = new HashSet<EventInfo>();
				EventCache[eventType] = events;
			}
			var info = new EventInfo
			{
				DeclaringType = declaringType,
				EventType = eventType,
				MethodInfo = method,
				IsCancel = false,
				Order = order,
				// 封装方法为统一的格式
				TaskInvoke = InvokeBuilder.Build(method, declaringType)
			};
			events.Add(info);
			// 绑定对应的撤销器
			var cancelInfo = events.FirstOrDefault(x => x.EventType == eventType && x.Order == order && x.IsCancel == true);
			if (cancelInfo != null) info.CancelInfo = cancelInfo;
		}

		// 添加撤销事件执行器
		public static void AddCancelEventHandler(Type declaringType, int order, Type eventType, MethodInfo method)
		{
			if (!EventCache.TryGetValue(eventType, out var events))
			{
				events = new HashSet<EventInfo>();
				EventCache[eventType] = events;
			}
			var cancelInfo = new EventInfo
			{
				DeclaringType = declaringType,
				EventType = eventType,
				MethodInfo = method,
				IsCancel = true,
				Order = order,
				TaskInvoke = InvokeBuilder.Build(method, declaringType)
			};
			events.Add(cancelInfo);
			// 该撤销器绑定对应的执行器
			var info = events.FirstOrDefault(x => x.EventType == eventType && x.Order == order && x.IsCancel == false);
			if (info != null) info.CancelInfo = cancelInfo;
		}


		// 构建事件执行链
		private static ServiceEventHandlerDelegate BuildHandler<TEvent>() where TEvent : IEvent
		{
			if (HandlerDelegateCache.TryGetValue(typeof(TEvent), out var handler)) return handler;

			ServiceEventHandlerDelegate next = async (provider, @params) =>
			{
				var eventData = @params.OfType<Event>().FirstOrDefault();
				var cancel = @params.OfType<CancellationToken>().FirstOrDefault();

				var logger = provider.GetRequiredService<ILogger<EventBus>>();
				logger.LogDebug("开始执行事件: {0},{1}", typeof(TEvent).Name, @params[0]);

				if (!EventCache.TryGetValue(typeof(TEvent), out var eventInfos)) return;
				var infos = eventInfos.Where(x => x.IsCancel == false).OrderBy(x => x.Order).ToArray();

				Exception? exception = null;
				// 包装调用链和撤销链
				for (int i = 0; i < infos.Length; i++)
				{
					var info = infos[i];

					if (cancel.IsCancellationRequested)
					{
						logger.LogDebug("事件已被取消执行: {0},位置：{1}", typeof(TEvent).Name, info.MethodInfo.Name);
						return;
					}

					logger.LogDebug("事件: {0},=> {1}", typeof(TEvent).Name, info.MethodInfo.Name);

					// 构建执行链
					var currentService = provider.GetRequiredService(info.DeclaringType);
					try
					{
						await info.TaskInvoke(currentService, @params);
					}
					// 执行失败，开始回退
					catch (Exception ex)
					{
						exception = ex;

						logger.LogError(ex, "执行事件失败: {0},执行器:{1},{2}", typeof(TEvent).Name, info.MethodInfo.Name, @params[0]);
						for (int j = i; j >= 0; j--)
						{
							var backInfo = infos[j];
							if (backInfo.CancelInfo is not null)
							{
								await backInfo.CancelInfo.TaskInvoke(currentService, @params);
							}
						}
						break;
					}
				}

				// 如果出现了异常，在执行撤销链完成后，重新抛出异常
				if (exception != null) throw exception;
			};
			// 存到缓存
			HandlerDelegateCache[typeof(TEvent)] = next;
			return next;
		}

		#endregion

	}

	// 事件总线
	public partial class EventBus : IEventBus
	{

		private readonly IServiceProvider _provider;

		public EventBus(IServiceProvider serviceProvider)
		{
			_provider = serviceProvider;
		}

		// 发布事件
		public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
		{
			var handler = BuildHandler<TEvent>();

			if (Middleware != null)
			{
				var mid = _provider.GetRequiredService<IEventMiddleware<TEvent>>();
				EventHandlerDelegate next = async () =>
				{
					await handler(_provider, @event, cancellationToken);
				};
				await mid.HandleAsync(@event, next);
			}
			else
			{
				await handler(_provider, @event, cancellationToken);
			}
		}
	}
}
