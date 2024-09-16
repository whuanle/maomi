using Demo1.Application;
using Maomi;

namespace Demo1.Api
{
    [InjectModule<ApplicationModule>]
    public class ApiModule : IModule
    {
        private readonly IConfiguration _configuration;
        public ApiModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(ServiceContext context)
        {
            var configuration = context.Configuration;
            context.Services.AddCors();
        }
    }
}
