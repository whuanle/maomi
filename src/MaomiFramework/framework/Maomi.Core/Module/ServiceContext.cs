using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Maomi.Module
{
    /// <summary>
    /// 模块上下文
    /// </summary>
    public class ServiceContext
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;


        internal ServiceContext(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            _serviceCollection = serviceCollection;
            _configuration = configuration;
        }

        /// <summary>
        /// 依赖注入服务
        /// </summary>
        public IServiceCollection Services => _serviceCollection;

        /// <summary>
        /// 配置
        /// </summary>
        public IConfiguration Configuration => _configuration;
    }
}
