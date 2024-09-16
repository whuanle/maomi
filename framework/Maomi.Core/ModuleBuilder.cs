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
    protected bool _initialized = false;

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

    public virtual void Start(Type rootModuleType)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        // 构建模块依赖树
        HashSet<Type> moduleTypes = new();
        ModuleNode rootNode = new ModuleNode(rootModuleType);
        BuildModuleTree(moduleTypes, rootNode);

        // 初始化配置
        var contextServicePriovider = _services.BuildServiceProvider();
        var context = new DefaultServiceContext(_services, contextServicePriovider.GetService<IConfiguration>()!);

        foreach (var item in moduleTypes)
        {
            context.AddModule(new ModuleRecord
            {
                Type = item,
                Assembly = item.Assembly
            });
        }

        // 初始化模块树
        HashSet<ModuleNode> initializedModules = new();
        InitModuleTree(contextServicePriovider, context, initializedModules, rootNode);

        // 处理自定义模块
        foreach (var assembly in _options.CustomAssembies)
        {
            var moduleType = GetModuleTypeFromAssembly(assembly);
            if (moduleType == null)
            {
                continue;
            }

            if (initializedModules.Any(x => x.ModuleType == moduleType))
            {
                continue;
            }

            RegisterAssembly(contextServicePriovider, assembly);
            RegisterModule(contextServicePriovider, context, moduleType);
        }
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

    protected virtual void InitModuleTree(IServiceProvider serviceProvider, ServiceContext context, HashSet<ModuleNode> initializedModules, ModuleNode currentNode)
    {
        foreach (var childNode in currentNode.Childs)
        {
            if (initializedModules.Contains(childNode))
            {
                continue;
            }

            InitModuleTree(serviceProvider, context, initializedModules, childNode);
        }

        RegisterAssembly(serviceProvider, currentNode.ModuleType.Assembly);
        RegisterModule(serviceProvider, context, currentNode.ModuleType);
    }

    protected Type? GetModuleTypeFromAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.GetInterface(nameof(IModule)) != null)
            {
                return type;
            }
        }

        return default!;
    }

    protected virtual void RegisterModule(IServiceProvider serviceProvider, ServiceContext serviceContext, Type moduleType)
    {
        // 实例化此模块
        var module = (IModule)serviceProvider.GetRequiredService(moduleType);
        module.ConfigureServices(serviceContext);
    }

    // 扫描此模块（程序集）中需要依赖注入的服务
    protected virtual void RegisterAssembly(IServiceProvider serviceProvider, Assembly assembly)
    {
        // 只扫描可实例化的类，不扫描静态类、接口、抽象类、嵌套类、非公开类等
        foreach (var currentType in assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && !x.IsNestedPublic))
        {
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
