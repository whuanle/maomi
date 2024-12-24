// <copyright file="ModuleBuilder.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

#pragma warning disable CS1591
#pragma warning disable SA1401
#pragma warning disable SA1600

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Maomi;

/// <summary>
/// 模块构建服务.
/// </summary>
public class ModuleBuilder
{
    protected readonly IServiceCollection _services;
    protected readonly InitOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleBuilder"/> class.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    public ModuleBuilder(IServiceCollection services, InitOptions options)
    {
        _services = services;
        _options = options;
    }

    /*
     分为两部分，一份是依赖树，一份是顺序的
     */
    public virtual void Start(Type rootModuleType)
    {
        // 所有模块类
        HashSet<Type> moduleTypes = new();

        // 自定义程序集模块类
        HashSet<Type> customModuleTypes = new();

        // 所有模块类实例
        HashSet<IModule> moduleInstances = new HashSet<IModule>();

        // 构建模块依赖树
        ModuleNode rootNode = new ModuleNode(rootModuleType);
        BuildModuleTree(moduleTypes, rootNode);

        // 处理自定义模块
        foreach (var assembly in _options.CustomAssembies)
        {
            var moduleType = GetModuleTypeFromAssembly(assembly);
            if (moduleType == null)
            {
                continue;
            }

            if (moduleTypes.Any(x => x == moduleType))
            {
                continue;
            }

            customModuleTypes.Add(moduleType);
            moduleTypes.Add(moduleType);
            _services.AddTransient(moduleType);
        }

        // 初始化配置
        var contextServicePriovider = _services.BuildServiceProvider();
        var context = new DefaultServiceContext(_services, contextServicePriovider.GetService<IConfiguration>()!);

        foreach (var moduleType in moduleTypes)
        {
            context.AddModule(new ModuleRecord
            {
                Type = moduleType,
                Assembly = moduleType.Assembly
            });
        }

        // 已经初始化的模块
        HashSet<Type> initializedModules = new();
        HashSet<Assembly> initializedAssemblys = new();

        // 首先实例化程序集中的模块类
        foreach (var moduleType in customModuleTypes)
        {
            RegisterModule(contextServicePriovider, context, moduleInstances, moduleType);
            initializedModules.Add(moduleType);
        }

        // 开始初始化模块依赖树
        InitModuleTree(contextServicePriovider, context, initializedModules, moduleInstances, rootNode);

        var moduleCores = moduleInstances.OfType<ModuleCore>().ToArray();

        // 首先扫描程序集中的类型
        foreach (var moduleType in customModuleTypes)
        {
            RegisterAssembly(contextServicePriovider, moduleCores, moduleType.Assembly);
            initializedAssemblys.Add(moduleType.Assembly);
        }

        // 扫描模块树中的程序集
        InitModuleTreeAssembly(contextServicePriovider, context, initializedModules, moduleCores, rootNode);
    }

    /// <summary>
    /// 构建模块依赖树.
    /// </summary>
    /// <param name="moduleTypes">已记录的模块.</param>
    /// <param name="parentModuleNode">父节点.</param>
    protected virtual void BuildModuleTree(HashSet<Type> moduleTypes, ModuleNode parentModuleNode)
    {
        _services.AddTransient(parentModuleNode.ModuleType);
        moduleTypes.Add(parentModuleNode.ModuleType);

        var moduleDependences = parentModuleNode.ModuleType.GetCustomAttributes(false)
            .Where(x => x.GetType().IsSubclassOf(typeof(InjectModuleAttribute)))
            .OfType<InjectModuleAttribute>();

        foreach (var module in moduleDependences)
        {
            ModuleNode moduleNode = new ModuleNode(module.ModuleType, parentModuleNode);
            parentModuleNode.Childs.Add(moduleNode);

            // 循环依赖检测
            // 检查当前模块(parentTree)依赖的模块(childTree)是否在之前出现过，如果是，则说明是循环依赖
            var isLoop = parentModuleNode.ContainsTree(moduleNode);
            if (isLoop)
            {
                throw new InvalidOperationException($"Loop dependent reference or duplicate reference detected.{module.ModuleType.Name} -> {parentModuleNode.ModuleType.Name} -> {module.ModuleType.Name}.");
            }

            BuildModuleTree(moduleTypes, moduleNode);
        }
    }

    protected virtual void InitModuleTree(IServiceProvider serviceProvider, ServiceContext context, HashSet<Type> initializedModules, HashSet<IModule> modules, ModuleNode currentNode)
    {
        foreach (var childNode in currentNode.Childs)
        {
            if (initializedModules.Contains(childNode.ModuleType))
            {
                continue;
            }

            InitModuleTree(serviceProvider, context, initializedModules, modules, childNode);
        }

        RegisterModule(serviceProvider, context, modules, currentNode.ModuleType);
    }

    protected virtual void InitModuleTreeAssembly(IServiceProvider serviceProvider, ServiceContext context, HashSet<Type> initializedModules, ModuleCore[] moduleInstances, ModuleNode currentNode)
    {
        foreach (var childNode in currentNode.Childs)
        {
            if (initializedModules.Contains(childNode.ModuleType))
            {
                continue;
            }

            InitModuleTreeAssembly(serviceProvider, context, initializedModules, moduleInstances, childNode);
        }

        RegisterAssembly(serviceProvider, moduleInstances, currentNode.ModuleType.Assembly);
    }

    // 从程序集中获取模块类.
    protected virtual Type? GetModuleTypeFromAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
            {
                continue;
            }

            if (type.GetInterface(nameof(IModule)) != null)
            {
                return type;
            }
        }

        return default!;
    }

    protected virtual void RegisterModule(IServiceProvider serviceProvider, ServiceContext serviceContext, HashSet<IModule> modules, Type moduleType)
    {
        // 实例化此模块
        var module = (IModule)serviceProvider.GetRequiredService(moduleType);
        module.ConfigureServices(serviceContext);
        modules.Add(module);
    }

    // 扫描此模块（程序集）中需要依赖注入的服务
    protected virtual void RegisterAssembly(IServiceProvider serviceProvider, ModuleCore[] modules, Assembly assembly)
    {
        // 只扫描可实例化的类，不扫描静态类、接口、抽象类、嵌套类、非公开类等
        foreach (var currentType in assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && !x.IsNestedPublic))
        {
            if (currentType.IsAssignableTo(typeof(IModule)))
            {
                continue;
            }

            foreach (var filter in modules)
            {
                filter.TypeFilter(currentType);
            }

            var inject = currentType.GetCustomAttributes().OfType<InjectOnAttribute>().FirstOrDefault();
            if (inject == null)
            {
                continue;
            }

            // 将自身注册到容器中
            bool hasInterface = false;
            bool hasBaseClass = false;

            // 不注册任何服务
            if (inject.Scheme == InjectScheme.None)
            {
                if (inject.Own)
                {
                    RegisterService(inject, currentType, currentType);
                }

                continue;
            }

            // 注册接口
            if (inject.Scheme == InjectScheme.OnlyInterfaces || inject.Scheme == InjectScheme.Any)
            {
                var interfaces = currentType.GetInterfaces().Where(x => !_options.FilterServiceTypes.Contains(x)).ToList();

                if (interfaces.Count == 0)
                {
                    hasInterface = false;
                }
                else
                {
                    foreach (var interfaceType in interfaces)
                    {
                        RegisterService(inject, interfaceType, currentType);
                    }
                }
            }

            // 注册父类
            if (inject.Scheme == InjectScheme.OnlyBaseClass || inject.Scheme == InjectScheme.Any)
            {
                var baseType = currentType.BaseType;
                if (baseType == typeof(object) || baseType == null || _options.FilterServiceTypes.Contains(baseType))
                {
                    hasBaseClass = false;
                }
                else
                {
                    RegisterService(inject, baseType, currentType);
                }
            }

            // 自定义注册时，其它规则失效
            if (inject.Scheme == InjectScheme.Some)
            {
                if (inject.ServiceTypes == null)
                {
                    continue;
                }

                foreach (var interfaceType in inject.ServiceTypes)
                {
                    RegisterService(inject, interfaceType, currentType);
                }

                continue;
            }

            // 当没有找到任何父类或者可用接口时，将自身注册到容器中
            if (inject.Own || (!hasBaseClass && !hasInterface))
            {
                RegisterService(inject, currentType, currentType);
            }
        }
    }

    // 注册服务
    protected virtual void RegisterService(InjectOnAttribute injectOnAttribute, Type serviceType, Type implementationType)
    {
        if (injectOnAttribute.ServiceKey != null)
        {
            AddKeyedService(injectOnAttribute.Lifetime, injectOnAttribute.ServiceKey, serviceType, implementationType);
        }
        else
        {
            AddService(injectOnAttribute.Lifetime, serviceType, implementationType);
        }
    }

    protected virtual void AddService(ServiceLifetime lifetime, Type serviceType, Type implementationType)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient: _services.AddTransient(serviceType, implementationType); break;
            case ServiceLifetime.Scoped: _services.AddScoped(serviceType, implementationType); break;
            case ServiceLifetime.Singleton: _services.AddSingleton(serviceType, implementationType); break;
        }
    }

    protected virtual void AddKeyedService(ServiceLifetime lifetime, object key, Type serviceType, Type implementationType)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient: _services.AddKeyedTransient(serviceKey: key, serviceType: serviceType, implementationType: implementationType); break;
            case ServiceLifetime.Scoped: _services.AddKeyedScoped(serviceKey: key, serviceType: serviceType, implementationType: implementationType); break;
            case ServiceLifetime.Singleton: _services.AddKeyedSingleton(serviceKey: key, serviceType: serviceType, implementationType: implementationType); break;
        }
    }
}
