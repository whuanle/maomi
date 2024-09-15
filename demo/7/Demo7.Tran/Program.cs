using Maomi.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Demo7.Tran
{
	public class TranMiddleware<TEvent> : IEventMiddleware<TEvent> where TEvent : IEvent
	{
		private readonly MyContext _context;

		public TranMiddleware(MyContext context)
		{
			_context = context;
		}

		public async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
		{
			Console.WriteLine("----- Handling command {0} ({1})", @event.GetType().Name, @event.ToString());
			using var tran = await _context.Database.BeginTransactionAsync();
			try
			{
				await next();
				await tran.CommitAsync();
			}
			catch (Exception)
			{
				await tran.RollbackAsync();
				throw;
			}
		}
	}

	public record class MyEvent : Event
	{
		public string Name { get; set; }
		public string EMail { get; set; }
		public override string ToString()
		{
			return $"用户名:	{Name} ，邮箱:	{EMail}";
		}
	}

	[Event]
	public class UserRegisterEventHandler
	{
		private readonly MyContext _context;

		public UserRegisterEventHandler(MyContext context)
		{
			_context = context;
		}

		[EventHandler(Order = 0)]
		public async Task InsertDb(MyEvent @event)
		{
			var state = new Random().Next(0, 2);
			if (state == 1)
			{
				await _context.Account.AddAsync(new AccountEntity
				{
					Name = @event.Name,
					EMail = @event.EMail,
				});
				await _context.SaveChangesAsync();
				Console.WriteLine("√ 用户信息已添加到数据库");
			}
			else throw new Exception("× 写入用户信息到数据库失败");
		}
	}


	[Event]
	public class VerifyEMailEventHandler
	{
		private readonly MyContext _context;

		public VerifyEMailEventHandler(MyContext context)
		{
			_context = context;
		}

		[EventHandler(Order = 1)]
		public async Task VerifyEmaill(MyEvent @event)
		{
			var state = new Random().Next(0, 2);
			if (state == 1)
			{
				var account = await _context.Account.FirstOrDefaultAsync(x=>x.EMail == @event.EMail);
				if (account != null)
				{
					account.VerifyEMail = true;
					_context.Account.Update(account);
					await _context.SaveChangesAsync();
					Console.WriteLine("√ 已验证邮箱");
					return;
				}
			}
			throw new Exception("× 验证邮箱失败");
		}
	}


	internal class Program
	{
		static async Task Main(string[] args)
		{
			var ioc = new ServiceCollection();
			ioc.AddDbContext<MyContext>();
			ioc.AddLogging(build => build.AddConsole());
			ioc.AddEventBus(typeof(TranMiddleware<>));

			var services = ioc.BuildServiceProvider();
			var eventBus = services.GetRequiredService<IEventBus>();
			await eventBus.PublishAsync(new MyEvent()
			{
				Name = "工良",
				EMail = "工良@maomi.com"
			});
		}
	}
}