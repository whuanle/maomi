using Maomi.Module;
using Maomi.Web.Core;

namespace Demo10.ExceptionFilter
{
    [InjectModule<MaomiWebModule>()]
    public class ApiModule : IModule
    {
        public void ConfigureServices(ServiceContext context)
        {
        }
    }
}
