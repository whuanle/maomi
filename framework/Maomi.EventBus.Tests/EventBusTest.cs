using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Maomi.EventBus.Tests
{
    public class EventBusTest
    {
        private readonly IServiceCollection _ioc;
        public EventBusTest()
        {
            var ioc = new ServiceCollection();
            ioc.AddEventBus();
            ioc.AddLogging(builder => builder.AddConsole());
            ioc.AddScoped<SetException>();
            ioc.AddScoped<EventStats>(s => new EventStats { Names = new() });
            _ioc = ioc;
        }

        [Fact]
        public async Task PublishEvent()
        {
            var provider = _ioc.BuildServiceProvider();
            var eventBus = provider.GetRequiredService<IEventBus>();
            var setException = provider.GetRequiredService<SetException>();
            var eventStats = provider.GetRequiredService<EventStats>();
            setException.Node = 1;
            try
            {
                await eventBus.PublishAsync(new MyEvent()
                {
                    Name = "工良又在卷了",
                    Book = "Hive 入门到放弃"
                });
            }
            catch (Exception ex)
            {
                Assert.Equal("× 写入用户信息到数据库失败", ex.Message);
            }


            Assert.Equal(nameof(UserRegisterEventHandler.InsertDb), eventStats.Names[1]);
            Assert.Equal(nameof(UserRegisterEventHandler.CancelInsertDb), eventStats.Names[2]);

            provider = _ioc.BuildServiceProvider();
            eventBus = provider.GetRequiredService<IEventBus>();
            setException = provider.GetRequiredService<SetException>();
            eventStats = provider.GetRequiredService<EventStats>();
            setException.Node = 2;

            try
            {
                await eventBus.PublishAsync(new MyEvent()
                {
                    Name = "工良又在卷了",
                    Book = "Hive 入门到放弃"
                });
            }
            catch (Exception ex)
            {
                Assert.Equal("× 初始化用户数据失败", ex.Message);
            }

            Assert.Equal(nameof(UserRegisterEventHandler.InsertDb), eventStats.Names[1]);
            Assert.Equal(nameof(UserRegisterEventHandler.InitUser), eventStats.Names[2]);
            Assert.Equal(nameof(UserRegisterEventHandler.CancelInitUser), eventStats.Names[3]);
            Assert.Equal(nameof(UserRegisterEventHandler.CancelInsertDb), eventStats.Names[4]);

            provider = _ioc.BuildServiceProvider();
            eventBus = provider.GetRequiredService<IEventBus>();
            setException = provider.GetRequiredService<SetException>();
            eventStats = provider.GetRequiredService<EventStats>();
            setException.Node = 3;

            try
            {
                await eventBus.PublishAsync(new MyEvent()
                {
                    Name = "工良又在卷了",
                    Book = "Hive 入门到放弃"
                });
            }
            catch (Exception ex)
            {
                Assert.Equal("× 发送验证邮件失败", ex.Message);
            }

            Assert.Equal(nameof(UserRegisterEventHandler.InsertDb), eventStats.Names[1]);
            Assert.Equal(nameof(UserRegisterEventHandler.InitUser), eventStats.Names[2]);
            Assert.Equal(nameof(UserRegisterEventHandler.SendEmail), eventStats.Names[3]);
            Assert.Equal(nameof(UserRegisterEventHandler.CancelSendEmail), eventStats.Names[4]);
            Assert.Equal(nameof(UserRegisterEventHandler.CancelInitUser), eventStats.Names[5]);
            Assert.Equal(nameof(UserRegisterEventHandler.CancelInsertDb), eventStats.Names[6]);

        }


        [Fact]
        public async Task CancelPublishEvent()
        {
            var provider = _ioc.BuildServiceProvider();
            var eventBus = provider.GetRequiredService<IEventBus>();
            var setException = provider.GetRequiredService<SetException>();
            var eventStats = provider.GetRequiredService<EventStats>();

            await eventBus.PublishAsync(new CancelMyEvent()
            {
                Name = "工良又在卷了",
                Book = "Hive 入门到放弃"
            });

            // 取消之前，执行函数数量 4
            Assert.Equal(4, eventStats.Names.Count);
            Assert.Equal(nameof(UserRegisterEventHandler.InsertDb), eventStats.Names[1]);
            Assert.Equal(nameof(UserRegisterEventHandler.SendEmail), eventStats.Names[3]);

            provider = _ioc.BuildServiceProvider();
            eventBus = provider.GetRequiredService<IEventBus>();
            setException = provider.GetRequiredService<SetException>();
            eventStats = provider.GetRequiredService<EventStats>();

            CancellationTokenSource source = new CancellationTokenSource();
            new Thread(() =>
            {
                Thread.Sleep(600);
                source.Cancel();
            }).Start();
            await eventBus.PublishAsync(new CancelMyEvent()
            {
                Name = "工良又在卷了",
                Book = "Hive 入门到放弃"
            }, source.Token);

            // 取消之后，执行函数数量 < 4
            Assert.NotEqual(4, eventStats.Names.Count);
        }
    }
}