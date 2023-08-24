using Maomi.Module;
using Maomi.I18n;
using Maomi.Web.Core;

namespace Demo10.ApiDataAnnotations
{
    [InjectModule<MaomiWebModule>()]
    public class ApiModule : IModule
    {
        public void ConfigureServices(ServiceContext context)
        {
            context.Services.AddI18nResource(options =>
            {
                options.AddJson<ApiModule>("i18n");
            });
        }
    }
}
