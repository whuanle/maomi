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
                    Name = "�������ھ���",
                    Book = "Hive ���ŵ�����"
                });
            }
            catch (Exception ex)
            {
                Assert.Equal("�� д���û���Ϣ�����ݿ�ʧ��", ex.Message);
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
                    Name = "�������ھ���",
                    Book = "Hive ���ŵ�����"
                });
            }
            catch (Exception ex)
            {
                Assert.Equal("�� ��ʼ���û�����ʧ��", ex.Message);
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
                    Name = "�������ھ���",
                    Book = "Hive ���ŵ�����"
                });
            }
            catch (Exception ex)
            {
                Assert.Equal("�� ������֤�ʼ�ʧ��", ex.Message);
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
                Name = "�������ھ���",
                Book = "Hive ���ŵ�����"
            });

            // ȡ��֮ǰ��ִ�к������� 4
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
                Name = "�������ھ���",
                Book = "Hive ���ŵ�����"
            }, source.Token);

            // ȡ��֮��ִ�к������� < 4
            Assert.NotEqual(4, eventStats.Names.Count);
        }
    }
}