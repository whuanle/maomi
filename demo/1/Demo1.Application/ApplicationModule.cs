using Maomi;
using Microsoft.Extensions.Configuration;

namespace Demo1.Application
{
    public class ApplicationModule : IModule
    {
        // 模块类中可以使用依赖注入
        private readonly IConfiguration _configuration;
        public ApplicationModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(ServiceContext services)
        {
            // services.Services.AddScoped<IMyService, MyService>();
        }
    }
}
