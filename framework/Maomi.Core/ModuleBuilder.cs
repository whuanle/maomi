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
public partial class ModuleBuilder
{
    protected readonly ModuleBuilderParamters _modulerParamters;
    protected readonly IServiceCollection _services;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly DefaultServiceContext _serviceContext;

    // 已经初始化的模块
    protected readonly HashSet<Type> _initializedModules = new();

    // 已经初始化的程序集
    protected readonly HashSet<Assembly> _initializedAssemblys = new();

    // 所有模块类实例
    protected readonly HashSet<IModule> _moduleInstances = new();

    // ModuleCore 模块类实例
    protected readonly HashSet<ModuleCore> _moduleCoreInstances = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleBuilder"/> class.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="modulerParamters"></param>
    public ModuleBuilder(IServiceCollection services, ModuleBuilderParamters modulerParamters)
    {
        _services = services;
        _modulerParamters = modulerParamters;

        foreach (var module in _modulerParamters.ModuleTypes)
        {
            _services.AddSingleton(module);
        }

        // 初始化配置
        _serviceProvider = _services.BuildServiceProvider();
        _serviceContext = new DefaultServiceContext(_services, _serviceProvider.GetService<IConfiguration>()!);

        // 将程序集模块添加到上下文中
        foreach (var moduleType in _modulerParamters.ModuleTypes)
        {
            _serviceContext.AddModule(new ModuleRecord
            {
                Type = moduleType,
                Assembly = moduleType.Assembly
            });
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleBuilder"/> class.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <param name="rootModuleType"></param>
    public ModuleBuilder(IServiceCollection services, ModuleOptions options, Type rootModuleType)
        : this(services, new ModuleBuilderParamters(options, rootModuleType))
    {
    }

    /// <summary>
    /// 构建模块.
    /// </summary>
    public virtual void Build()
    {
        var moduleTypes = _modulerParamters.ModuleTypes;
        var customModuleTypes = _modulerParamters.CustomModuleTypes;
        var rootModuleNode = _modulerParamters.RootModuleNode;

        // 开始初始化模块依赖树
        InstantiationModuleTree(_modulerParamters.RootModuleNode);

        // 首先实例化程序集中的模块类
        foreach (var moduleType in customModuleTypes)
        {
            if (_initializedModules.Contains(moduleType))
            {
                continue;
            }

            InstantiationModule(moduleType);
            _initializedModules.Add(moduleType);
        }

        foreach (var moduleCore in _moduleInstances.OfType<ModuleCore>().ToHashSet())
        {
            _moduleCoreInstances.Add(moduleCore);
        }

        // 扫描模块树中的程序集
        ScanServiceFromModuleTree(rootModuleNode);

        // 首先扫描程序集中的类型
        foreach (var moduleType in customModuleTypes)
        {
            if (_initializedAssemblys.Contains(moduleType.Assembly))
            {
                continue;
            }

            ScanServiceFromAssembly(moduleType.Assembly);
            _initializedAssemblys.Add(moduleType.Assembly);
        }
    }

    /// <summary>
    /// 实例化模块树.
    /// </summary>
    /// <param name="currentNode"></param>
    protected virtual void InstantiationModuleTree(ModuleNode currentNode)
    {
        foreach (var childNode in currentNode.Childs)
        {
            if (_initializedModules.Contains(childNode.ModuleType))
            {
                continue;
            }

            InstantiationModuleTree(childNode);
        }

        InstantiationModule(currentNode.ModuleType);
    }

    /// <summary>
    /// 从模块树中扫描程序集.
    /// </summary>
    /// <param name="currentNode"></param>
    protected virtual void ScanServiceFromModuleTree(ModuleNode currentNode)
    {
        foreach (var childNode in currentNode.Childs)
        {
            ScanServiceFromModuleTree(childNode);
        }

        // 跳过重复扫描的程序集
        if (_initializedAssemblys.Contains(currentNode.ModuleType.Assembly))
        {
            return;
        }

        ScanServiceFromAssembly(currentNode.ModuleType.Assembly);
        _initializedAssemblys.Add(currentNode.ModuleType.Assembly);
    }

    /// <summary>
    /// 实例化模块类.
    /// </summary>
    /// <param name="moduleType"></param>
    protected virtual void InstantiationModule(Type moduleType)
    {
        // 实例化此模块
        var module = (IModule)_serviceProvider.GetRequiredService(moduleType);
        module.ConfigureServices(_serviceContext);
        _moduleInstances.Add(module);
        _initializedModules.Add(moduleType);
    }

    /// <summary>
    /// 扫描此模块（程序集）中需要依赖注入的服务.
    /// </summary>
    /// <param name="assembly"></param>
    protected virtual void ScanServiceFromAssembly(Assembly assembly)
    {
        // 只扫描可实例化的类，不扫描静态类、接口、抽象类、嵌套类、非公开类等
        foreach (var currentType in assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && !x.IsNestedPublic))
        {
            foreach (var filter in _moduleCoreInstances)
            {
                filter.TypeFilter(currentType);
            }

            if (currentType.IsAssignableTo(typeof(IModule)))
            {
                continue;
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
                var interfaces = currentType.GetInterfaces().Where(x => !_modulerParamters.Options.FilterServiceTypes.Contains(x)).ToList();

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
                if (baseType == typeof(object) || baseType == null || _modulerParamters.Options.FilterServiceTypes.Contains(baseType))
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

    /// <summary>
    /// 注册服务.
    /// </summary>
    /// <param name="injectOnAttribute"></param>
    /// <param name="serviceType"></param>
    /// <param name="implementationType"></param>
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

    /// <summary>
    /// 注册服务.
    /// </summary>
    /// <param name="lifetime"></param>
    /// <param name="serviceType"></param>
    /// <param name="implementationType"></param>
    protected virtual void AddService(ServiceLifetime lifetime, Type serviceType, Type implementationType)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient: _services.AddTransient(serviceType, implementationType); break;
            case ServiceLifetime.Scoped: _services.AddScoped(serviceType, implementationType); break;
            case ServiceLifetime.Singleton: _services.AddSingleton(serviceType, implementationType); break;
        }
    }

    /// <summary>
    /// 注册服务.
    /// </summary>
    /// <param name="lifetime"></param>
    /// <param name="key"></param>
    /// <param name="serviceType"></param>
    /// <param name="implementationType"></param>
    protected virtual void AddKeyedService(ServiceLifetime lifetime, object key, Type serviceType, Type implementationType)
    {
#if NET8_0_OR_GREATER
        switch (lifetime)
        {
            case ServiceLifetime.Transient: _services.AddKeyedTransient(serviceKey: key, serviceType: serviceType, implementationType: implementationType); break;
            case ServiceLifetime.Scoped: _services.AddKeyedScoped(serviceKey: key, serviceType: serviceType, implementationType: implementationType); break;
            case ServiceLifetime.Singleton: _services.AddKeyedSingleton(serviceKey: key, serviceType: serviceType, implementationType: implementationType); break;
        }
#endif
    }
}