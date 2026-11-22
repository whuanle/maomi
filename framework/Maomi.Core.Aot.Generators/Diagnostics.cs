// <copyright file="Diagnostics.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.CodeAnalysis;

namespace Maomi.Core.Aot.Generators;

/// <summary>
/// 源生成器诊断描述.
/// </summary>
internal static class Diagnostics
{
    /// <summary>
    /// MAOMI001：模块之间存在循环依赖.
    /// </summary>
    public static readonly DiagnosticDescriptor LoopDependency = new(
        id: "MAOMI001",
        title: "模块存在循环依赖",
        messageFormat: "检测到循环依赖：{0}",
        category: "Maomi.Core.Aot",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "模块之间通过 [InjectModule] 声明的依赖不能形成环。",
        helpLinkUri: "https://github.com/whuanle/maomi");

    /// <summary>
    /// MAOMI002：类型标记了 InjectOn 特性，但其所在程序集没有任何模块，不会被注册.
    /// </summary>
    public static readonly DiagnosticDescriptor ServiceWithoutModule = new(
        id: "MAOMI002",
        title: "类型不会被注册到容器",
        messageFormat: "{0} 标记了 InjectOn 特性，但其所在程序集 '{1}' 中没有定义任何模块，该类型不会被注册到容器（与 Maomi.Core 的扫描范围语义一致）",
        category: "Maomi.Core.Aot",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "只有模块所在程序集中的服务才会被扫描注册，其他程序集需要先定义模块。",
        helpLinkUri: "https://github.com/whuanle/maomi");

    /// <summary>
    /// MAOMI003：无法在编译期生成构造函数工厂，回退为 typeof 注册.
    /// </summary>
    public static readonly DiagnosticDescriptor ReflectionFallback = new(
        id: "MAOMI003",
        title: "回退为反射注册",
        messageFormat: "{0} 无法在编译期生成构造函数工厂委托（{1}），已回退为 typeof 注册，由容器在运行时反射激活；在 NativeAOT/裁剪发布下请确保该类型可被反射构造",
        category: "Maomi.Core.Aot",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "存在多个公共构造函数、没有公共构造函数或包含不受支持的参数类型时，无法生成工厂委托。",
        helpLinkUri: "https://github.com/whuanle/maomi");

    /// <summary>
    /// MAOMI004：Some 模式下 ServiceTypes 为空.
    /// </summary>
    public static readonly DiagnosticDescriptor EmptySomeServiceTypes = new(
        id: "MAOMI004",
        title: "InjectScheme.Some 的 ServiceTypes 为空",
        messageFormat: "{0} 使用 InjectScheme.Some 但 ServiceTypes 为空，除了 Own = true 之外不会注册任何服务",
        category: "Maomi.Core.Aot",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "InjectScheme.Some 只注册 ServiceTypes 列表中的类型。",
        helpLinkUri: "https://github.com/whuanle/maomi");

    /// <summary>
    /// MAOMI005：依赖的模块无法解析为有效的模块类.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidDependencyModule = new(
        id: "MAOMI005",
        title: "依赖的模块无效",
        messageFormat: "{0} 依赖的模块 '{1}' 无法解析为可实例化的模块类（模块类必须是非抽象、非泛型的类，且当前编译或引用程序集可见）",
        category: "Maomi.Core.Aot",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "[InjectModule] 声明的依赖模块必须是一个可实例化的模块类。",
        helpLinkUri: "https://github.com/whuanle/maomi");
}
