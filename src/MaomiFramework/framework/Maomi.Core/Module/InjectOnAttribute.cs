using Microsoft.Extensions.DependencyInjection;

namespace Maomi.Module
{
    /// <summary>
    /// 依赖注入标记
    /// </summary>
    /// <remarks>注意，程序启动时先注册模块类以及实例化，请勿在模块类中使用自动依赖注入的服务类</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InjectOnAttribute : Attribute
    {
        /// <summary>
        /// 要注入的服务
        /// </summary>
        public Type[]? ServicesType { get; set; }

        /// <summary>
        /// 生命周期
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }

        /// <summary>
        /// 注入模式
        /// </summary>
        public InjectScheme Scheme { get; set; }

        /// <summary>
        /// 是否注入自己
        /// </summary>
        public bool Own { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lifetime"></param>
        /// <param name="scheme"></param>
        public InjectOnAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient, InjectScheme scheme = InjectScheme.OnlyInterfaces)
        {
            Lifetime = lifetime;
            Scheme = scheme;
        }
    }

    /// <summary>
    /// 依赖注入标记
    /// </summary>
    /// <remarks>注意，程序启动时先注册模块类以及实例化，请勿在模块类中使用自动依赖注入的服务类</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InjectOnTransientAttribute : InjectOnAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheme"></param>
        public InjectOnTransientAttribute(InjectScheme scheme = InjectScheme.OnlyInterfaces) : base(ServiceLifetime.Transient, scheme)
        {
        }
    }

    /// <summary>
    /// 依赖注入标记
    /// </summary>
    /// <remarks>注意，程序启动时先注册模块类以及实例化，请勿在模块类中使用自动依赖注入的服务类</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InjectOnScopedAttribute : InjectOnAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheme"></param>
        public InjectOnScopedAttribute(InjectScheme scheme = InjectScheme.OnlyInterfaces) : base(ServiceLifetime.Scoped, scheme)
        {
        }
    }

    /// <summary>
    /// 依赖注入标记
    /// </summary>
    /// <remarks>注意，程序启动时先注册模块类以及实例化，请勿在模块类中使用自动依赖注入的服务类</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InjectOnSingletonAttribute : InjectOnAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheme"></param>
        public InjectOnSingletonAttribute(InjectScheme scheme = InjectScheme.OnlyInterfaces) : base(ServiceLifetime.Singleton, scheme)
        {
        }
    }
}
