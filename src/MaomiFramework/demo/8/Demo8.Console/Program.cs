using Maomi.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Program
{
    public record class MyEvent : Event
    {
        public string Name { get; set; }
        public string EMail { get; set; }
        public override string ToString()
        {
            return $"用户名:	{Name} ，邮箱:	{EMail}";
        }
    }

    public class LoggingMiddleware<TEvent> : IEventMiddleware<TEvent> where TEvent : IEvent
    {
        public async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
        {
            Console.WriteLine("----- Handling command {0} ({1})", @event.GetType().Name, @event.ToString());
            await next();
        }
    }

    [Event]
    public class CheckImageCodeEventHandler
    {
        [EventHandler(Order = 0)]
        public void Check(MyEvent @event)
        {
            Console.WriteLine(@event.ToString());
        }
    }

    [Event]
    public class UserRegisterEventHandler
    {
        [EventHandler(Order = 1)]
        public void InsertDb(MyEvent @event)
        {
            var state = new Random().Next(0, 2);
            if (state == 0)
                Console.WriteLine("√ 用户信息已添加到数据库");
            else throw new Exception("× 写入用户信息到数据库失败");
        }

        [EventHandler(Order = 1, IsCancel = true)]
        public void CancelInsertDb(MyEvent @event)
        {
            Console.WriteLine("注册失败，刷新验证码");
        }

        [EventHandler(Order = 2)]
        public void InitUser(MyEvent @event)
        {
            var state = new Random().Next(0, 2);
            if (state == 0)
                Console.WriteLine("√ 初始化用户数据，系统生成默认用户权限、数据");
            else throw new Exception("× 初始化用户数据失败");
        }

        [EventHandler(Order = 2, IsCancel = true)]
        public void CancelInitUser(MyEvent @event)
        {
            Console.WriteLine("撤销用户注册信息");
        }

        [EventHandler(Order = 3)]
        public void SendEmail(MyEvent @event)
        {
            var state = new Random().Next(0, 2);
            if (state == 0)
                Console.WriteLine("√ 发送验证邮件成功");
            else throw new Exception("× 发送验证邮件失败");
        }

        [EventHandler(Order = 3, IsCancel = true)]
        public void CancelSendEmail(MyEvent @event)
        {
            Console.WriteLine("× 撤销初始化用户数据");
        }
    }
    static async Task Main()
    {
        var ioc = new ServiceCollection();
        ioc.AddEventBus(typeof(LoggingMiddleware<>));
        ioc.AddLogging(build => build.AddConsole());
        var services = ioc.BuildServiceProvider();
        var eventBus = services.GetRequiredService<IEventBus>();
        await eventBus.PublishAsync(new MyEvent()
        {
            Name = "工良",
            EMail = "工良@maomi.com"
        });
    }
}
