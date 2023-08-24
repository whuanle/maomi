using Maomi.I18n;
using Maomi.Module;

namespace Maomi.Web.Core
{
    public class MaomiWebModule : IModule
    {
        public void ConfigureServices(ServiceContext context)
        {
            // i18n 服务
            context.Services.AddI18n("zh-CN");
            context.Services.AddI18nResource(options =>
            {
                options.AddJson<MaomiWebModule>("i18n");
            });

            // 添加控制器
            context.Services.AddControllers()
                .AddI18nDataAnnotation();
        }
    }
}
