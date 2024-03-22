using Maomi.Module;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Maomi
{
    /// <summary>
    /// 模块处理
    /// </summary>
    public static class ModuleExtensions
    {
        /// <summary>
        /// 注册模块化服务
        /// </summary>
        /// <typeparam name="TModule">入口模块</typeparam>
        /// <param name="services"></param>
        public static void AddModule<TModule>(this IServiceCollection services)
            where TModule : IModule
        {
            AddModule(services, typeof(TModule));
        }


        /// <summary>
        /// 注册模块化服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="startupModule">入口模块</param>
        public static void AddModule(this IServiceCollection services, Type startupModule)
        {
            if (startupModule?.GetInterface(nameof(IModule)) == null)
            {
                throw new TypeLoadException($"{startupModule?.Name} 不是有效的模块类");
            }

            IServiceProvider scope = BuildModule(services, startupModule);
        }


        #region 自动依赖注入

        /// <summary>
        /// 自动依赖注入
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <param name="injectTypes">已被注入的服务</param>
        private static void InitInjectService(IServiceCollection services, Assembly assembly, HashSet<Type> injectTypes)
        {
            // 只扫描可实例化的类，不扫描静态类、接口、抽象类、嵌套类、非公开类等
            foreach (var item in assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && !x.IsNestedPublic))
            {
                var inject = item.GetCustomAttributes().OfType<InjectOnAttribute>().FirstOrDefault();
                if (inject == null) continue;

                if (injectTypes.Contains(item)) continue;
                injectTypes.Add(item);

                // 如果需要注入自身
                if (inject.Own)
                {
                    switch (inject.Lifetime)
                    {
                        case ServiceLifetime.Transient: services.AddTransient(item); break;
                        case ServiceLifetime.Scoped: services.AddScoped(item); break;
                        case ServiceLifetime.Singleton: services.AddSingleton(item); break;
                    }
                }

                if (inject.Scheme == InjectScheme.None) continue;

                // 注入所有接口
                if (inject.Scheme == InjectScheme.OnlyInterfaces || inject.Scheme == InjectScheme.Any)
                {
                    var interfaces = item.GetInterfaces().Where(x => x != typeof(IDisposable)).ToList();
                    if (interfaces.Count() == 0) continue;
                    switch (inject.Lifetime)
                    {
                        case ServiceLifetime.Transient: interfaces.ForEach(x => services.AddTransient(x, item)); break;
                        case ServiceLifetime.Scoped: interfaces.ForEach(x => services.AddScoped(x, item)); break;
                        case ServiceLifetime.Singleton: interfaces.ForEach(x => services.AddSingleton(x, item)); break;
                    }
                }

                // 注入父类
                if (inject.Scheme == InjectScheme.OnlyBaseClass || inject.Scheme == InjectScheme.Any)
                {
                    var baseType = item.BaseType;
                    if (baseType == null) throw new ArgumentException($"{item.Name} 注入模式 {nameof(inject.Scheme)} 未找到父类！");
                    switch (inject.Lifetime)
                    {
                        case ServiceLifetime.Transient: services.AddTransient(baseType, item); break;
                        case ServiceLifetime.Scoped: services.AddScoped(baseType, item); break;
                        case ServiceLifetime.Singleton: services.AddSingleton(baseType, item); break;
                    }
                }
                if (inject.Scheme == InjectScheme.Some)
                {
                    var types = inject.ServicesType;
                    if (types == null) throw new ArgumentException($"{item.Name} 注入模式 {nameof(inject.Scheme)} 未找到服务！");
                    switch (inject.Lifetime)
                    {
                        case ServiceLifetime.Transient: types.ToList().ForEach(x => services.AddTransient(x, item)); break;
                        case ServiceLifetime.Scoped: types.ToList().ForEach(x => services.AddScoped(x, item)); break;
                        case ServiceLifetime.Singleton: types.ToList().ForEach(x => services.AddSingleton(x, item)); break;
                    }
                }
            }
        }

        #endregion


        #region 模块注册

        /// <summary>
        /// 构建模块依赖树并初始化模块
        /// </summary>
        /// <param name="services"></param>
        /// <param name="startupModule"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static IServiceProvider BuildModule(IServiceCollection services, Type startupModule)
        {
            // 生成根模块
            ModuleNode rootTree = new ModuleNode()
            {
                ModuleType = startupModule,
                Childs = new HashSet<ModuleNode>()
            };

            // 根模块依赖的其他模块
            // IModule => InjectModuleAttribute
            var rootDependencies = startupModule.GetCustomAttributes(false)
                .Where(x => x.GetType().IsSubclassOf(typeof(InjectModuleAttribute)))
                .OfType<InjectModuleAttribute>();

            // 构建模块依赖树
            BuildTree(services, rootTree, rootDependencies);

            // 构建一个 Ioc 实例，以便初始化模块类
            var scope = services.BuildServiceProvider();

            // 初始化所有模块类
            var serviceContext = new ServiceContext(services, scope.GetService<IConfiguration>()!);

            // 记录已经处理的程序集、模块和服务，以免重复处理
            HashSet<Assembly> moduleAssemblies = new HashSet<Assembly> { startupModule.Assembly };
            HashSet<Type> moduleTypes = new HashSet<Type>();
            HashSet<Type> injectTypes = new HashSet<Type>();

            InitModuleTree(scope, serviceContext, moduleAssemblies, moduleTypes, injectTypes, rootTree);

            return scope;
        }

        /// <summary>
        /// 构建模块依赖树
        /// </summary>
        /// <param name="services"></param>
        /// <param name="currentNode"></param>
        /// <param name="injectModules">其依赖的模块</param>
        private static void BuildTree(IServiceCollection services, ModuleNode currentNode, IEnumerable<InjectModuleAttribute> injectModules)
        {
            services.AddTransient(currentNode.ModuleType);
            if (injectModules == null || injectModules.Count() == 0) return;
            foreach (var childModule in injectModules)
            {
                var childTree = new ModuleNode
                {
                    ModuleType = childModule.ModuleType,
                    ParentModule = currentNode
                };

                // 循环依赖检测
                // 检查当前模块(parentTree)依赖的模块(childTree)是否在之前出现过，如果是，则说明是循环依赖
                var isLoop = currentNode.ContainsTree(childTree);
                if (isLoop)
                {
                    throw new OverflowException($"检测到循环依赖引用或重复引用！{currentNode.ModuleType.Name} 依赖的 {childModule.ModuleType.Name} 模块在其父模块中出现过！");
                }

                if (currentNode.Childs == null)
                {
                    currentNode.Childs = new HashSet<ModuleNode>();
                }

                currentNode.Childs.Add(childTree);
                // 子模块依赖的其他模块
                var childDependencies = childModule.ModuleType.GetCustomAttributes(inherit: false)
                    .Where(x => x.GetType().IsSubclassOf(typeof(InjectModuleAttribute))).OfType<InjectModuleAttribute>().ToHashSet();
                // 子模块也依赖其他模块
                BuildTree(services, childTree, childDependencies);
            }
        }


        /// <summary>
        /// 从模块树中遍历
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="context"></param>
        /// <param name="moduleTypes">已经被注册到容器中的模块类</param>
        /// <param name="moduleAssemblies">模块类所在的程序集</param>'
        /// <param name="injectTypes">已被注册到容器的服务</param>
        /// <param name="moduleNode">模块节点</param>
        private static void InitModuleTree(IServiceProvider serviceProvider,
            ServiceContext context,
            HashSet<Assembly> moduleAssemblies,
            HashSet<Type> moduleTypes,
            HashSet<Type> injectTypes,
            ModuleNode moduleNode)
        {
            if (moduleNode.Childs != null)
            {
                foreach (var item in moduleNode.Childs)
                {
                    InitModuleTree(serviceProvider, context, moduleAssemblies, moduleTypes, injectTypes, item);
                }
            }

            // 如果模块没有处理过
            if (!moduleTypes.Contains(moduleNode.ModuleType))
            {
                moduleTypes.Add(moduleNode.ModuleType);

                // 实例化此模块
                // 扫描此模块（程序集）中需要依赖注入的服务
                var module = (IModule)serviceProvider.GetRequiredService(moduleNode.ModuleType);
                module.ConfigureServices(context);
                InitInjectService(context.Services, moduleNode.ModuleType.Assembly, injectTypes);
                moduleAssemblies.Add(moduleNode.ModuleType.Assembly);
            }
        }

        #endregion
    }
}
