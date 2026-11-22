// <copyright file="MaomiAotRegistry.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace Maomi;

/// <summary>
/// 源生成器生成的注册表.
/// <para>
/// 源生成器会在编译期分析模块依赖图和带 <c>InjectOn</c> 特性的服务，并为每个程序集生成一个模块初始化器，
/// 在模块初始化器中调用 <see cref="RegisterModule"/> 填充注册数据：模块依赖图、模块构造函数工厂委托、模块所在程序集的服务注册代码块.
/// </para>
/// <para>
/// 一般无需手动调用 <see cref="RegisterModule"/>，它由源生成器生成的代码调用.
/// 若当前编译未被源生成器处理（例如项目中没有定义或引用任何模块），注册数据将为空，
/// 此时调用 <c>AddModule</c> 会得到明确的错误提示.
/// </para>
/// </summary>
public static class MaomiAotRegistry
{
    /// <summary>
    /// 注册数据锁.
    /// </summary>
    private static readonly object _lock = new();

    /// <summary>
    /// 模块依赖图：模块类 -> 依赖的模块类列表（按特性声明顺序）.
    /// </summary>
    private static readonly Dictionary<Type, Type[]> _moduleDependencies = new();

    /// <summary>
    /// 模块构造函数工厂委托：模块类 -> (IServiceProvider -> 模块实例).
    /// </summary>
    private static readonly Dictionary<Type, Func<IServiceProvider, object>> _moduleFactories = new();

    /// <summary>
    /// 模块所在程序集的服务注册代码块：模块类 -> (IServiceCollection -> 注册该程序集中所有标记了 InjectOn 特性的服务).
    /// </summary>
    private static readonly Dictionary<Type, Action<IServiceCollection>> _assemblyChunks = new();

    /// <summary>
    /// 是否已收到生成器注册的数据.
    /// </summary>
    private static bool _hasGeneratedCode;

    /// <summary>
    /// 获取一个值，该值表示是否存在由源生成器生成的注册数据.
    /// </summary>
    internal static bool HasGeneratedCode => _hasGeneratedCode;

    /// <summary>
    /// 注册一个模块（由源生成器生成的代码调用，同一模块重复注册时以第一次注册为准）.
    /// </summary>
    /// <param name="moduleType">模块类.</param>
    /// <param name="dependencies">依赖的模块类列表（按特性声明顺序）.</param>
    /// <param name="moduleFactory">模块构造函数工厂委托；源生成器无法生成时为 <c>null</c>，将回退为类型注册.</param>
    /// <param name="assemblyChunk">模块所在程序集的服务注册代码块；该程序集没有可注册的服务时为 <c>null</c>.</param>
    public static void RegisterModule(
        Type moduleType,
        Type[] dependencies,
        Func<IServiceProvider, object>? moduleFactory,
        Action<IServiceCollection>? assemblyChunk)
    {
        ArgumentNullException.ThrowIfNull(moduleType, nameof(moduleType));

        lock (_lock)
        {
            _hasGeneratedCode = true;
            _moduleDependencies.TryAdd(moduleType, dependencies ?? Array.Empty<Type>());
            if (moduleFactory != null)
            {
                _moduleFactories.TryAdd(moduleType, moduleFactory);
            }

            if (assemblyChunk != null)
            {
                _assemblyChunks.TryAdd(moduleType, assemblyChunk);
            }
        }
    }

    /// <summary>
    /// 获取模块的依赖模块列表.
    /// </summary>
    /// <param name="moduleType">模块类.</param>
    /// <param name="dependencies">依赖的模块类列表.</param>
    /// <returns>存在该模块的注册数据时返回 true.</returns>
    internal static bool TryGetDependencies(Type moduleType, out Type[] dependencies)
    {
        lock (_lock)
        {
            return _moduleDependencies.TryGetValue(moduleType, out dependencies!);
        }
    }

    /// <summary>
    /// 获取模块的构造函数工厂委托.
    /// </summary>
    /// <param name="moduleType">模块类.</param>
    /// <param name="moduleFactory">模块构造函数工厂委托.</param>
    /// <returns>存在该模块的工厂委托时返回 true.</returns>
    internal static bool TryGetFactory(Type moduleType, out Func<IServiceProvider, object> moduleFactory)
    {
        lock (_lock)
        {
            return _moduleFactories.TryGetValue(moduleType, out moduleFactory!);
        }
    }

    /// <summary>
    /// 获取模块所在程序集的服务注册代码块.
    /// </summary>
    /// <param name="moduleType">模块类.</param>
    /// <param name="assemblyChunk">服务注册代码块.</param>
    /// <returns>存在该模块的注册代码块时返回 true.</returns>
    internal static bool TryGetChunk(Type moduleType, out Action<IServiceCollection> assemblyChunk)
    {
        lock (_lock)
        {
            return _assemblyChunks.TryGetValue(moduleType, out assemblyChunk!);
        }
    }

    /// <summary>
    /// 生成已注册模块类型名称列表文本（用于错误提示）.
    /// </summary>
    /// <returns>已注册模块类型名称列表文本.</returns>
    internal static string DescribeKnownModules()
    {
        lock (_lock)
        {
            var builder = new System.Text.StringBuilder();
            var first = true;
            foreach (var moduleType in _moduleDependencies.Keys)
            {
                if (!first)
                {
                    builder.Append(", ");
                }

                builder.Append(moduleType.FullName);
                first = false;
            }

            return builder.ToString();
        }
    }
}
