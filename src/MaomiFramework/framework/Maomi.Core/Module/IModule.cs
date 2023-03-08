namespace Maomi.Module
{
    /// <summary>
    /// 模块接口
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// 模块中的依赖注入
        /// </summary>
        /// <param name="services">模块服务上下文</param>
        void ConfigureServices(ServiceContext context);
    }
}
