// <copyright file="ModuleBuilderParamters.Static.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Reflection;

namespace Maomi;

/// <summary>
/// 模块构建器参数.
/// </summary>
public class ModuleBuilderParamters
{
    private readonly ModuleOptions _options;
    private readonly ModuleNode _rootModuleNode;
    private readonly HashSet<Type> _moduleTypes = new();
    private readonly HashSet<Type> _customModuleTypes = new();

    /// <summary>
    /// 初始化配置.
    /// </summary>
    public ModuleOptions Options => _options;

    /// <summary>
    /// 模块依赖树.
    /// </summary>
    public ModuleNode RootModuleNode => _rootModuleNode;

    /// <summary>
    /// 所有模块类.
    /// </summary>
    public HashSet<Type> ModuleTypes => _moduleTypes;

    /// <summary>
    /// 自定义的模块类.
    /// </summary>
    public HashSet<Type> CustomModuleTypes => _customModuleTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleBuilderParamters"/> class.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="rootModuleType"></param>
    public ModuleBuilderParamters(ModuleOptions options, Type rootModuleType)
    {
        _options = options;
        _rootModuleNode = new ModuleNode(rootModuleType);

        // 构建模块依赖树
        BuildModuleTree(_moduleTypes, _rootModuleNode);

        // 导入自定义模块
        ImportModulesFromAssemblies(_customModuleTypes, _options.CustomAssembies);

        foreach (var module in _moduleTypes)
        {
            if (_moduleTypes.Contains(module))
            {
                _customModuleTypes.Remove(module);
            }
        }

        foreach (var module in _customModuleTypes)
        {
            _moduleTypes.Add(module);
        }
    }

    /// <summary>
    /// 从程序集中导入模块类.
    /// </summary>
    /// <param name="moduleTypes"></param>
    /// <param name="assemblies"></param>
    public static void ImportModulesFromAssemblies(HashSet<Type> moduleTypes, IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            if (moduleTypes.Any(x => x.Assembly == assembly))
            {
                continue;
            }

            var moduleType = FindModuleFromModule(assembly);
            if (moduleType == null)
            {
                continue;
            }

            if (moduleTypes.Any(x => x == moduleType))
            {
                continue;
            }

            moduleTypes.Add(moduleType);
        }
    }

    /// <summary>
    /// 构建模块依赖树.
    /// </summary>
    /// <param name="moduleTypes">已记录的模块.</param>
    /// <param name="parentModuleNode">父节点.</param>
    public static void BuildModuleTree(HashSet<Type> moduleTypes, ModuleNode parentModuleNode)
    {
        if (moduleTypes.Contains(parentModuleNode.ModuleType))
        {
            return;
        }

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

    /// <summary>
    /// 从程序集中获取模块类.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns>模块类<see cref="IModule"/>.</returns>
    public static Type? FindModuleFromModule(Assembly assembly)
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
}