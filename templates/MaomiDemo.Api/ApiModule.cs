using Maomi.Module;
using Maomi.Web.Core;

namespace MaomiDemo.Api
{
	[InjectModule<MaomiWebModule>]
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
