// <copyright file="MaomiSymbols.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Maomi.Core.Aot.Generators;

/// <summary>
/// 编译期符号缓存：Maomi.Core.Aot 运行时库中的关键类型.
/// </summary>
internal sealed class MaomiSymbols
{
    public const string ModuleInterfaceName = "Maomi.IModule";
    public const string InjectOnAttributeName = "Maomi.InjectOnAttribute";
    public const string InjectModuleAttributeName = "Maomi.InjectModuleAttribute";
    public const string FromKeyedServicesAttributeName = "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute";

    /// <summary>
    /// Maomi.IModule.
    /// </summary>
    public INamedTypeSymbol ModuleInterface { get; }

    /// <summary>
    /// Maomi.InjectOnAttribute.
    /// </summary>
    public INamedTypeSymbol InjectOnAttribute { get; }

    /// <summary>
    /// Maomi.InjectModuleAttribute.
    /// </summary>
    public INamedTypeSymbol InjectModuleAttribute { get; }

    /// <summary>
    /// Maomi.InjectOnScopedAttribute.
    /// </summary>
    public INamedTypeSymbol? InjectOnScopedAttribute { get; }

    /// <summary>
    /// Maomi.InjectOnSingletonAttribute.
    /// </summary>
    public INamedTypeSymbol? InjectOnSingletonAttribute { get; }

    /// <summary>
    /// Maomi.InjectOnTransientAttribute.
    /// </summary>
    public INamedTypeSymbol? InjectOnTransientAttribute { get; }

    /// <summary>
    /// Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute.
    /// </summary>
    public INamedTypeSymbol? FromKeyedServicesAttribute { get; }

    /// <summary>
    /// 服务类型过滤集（与原版 ModuleOptions.FilterServiceTypes 的默认值一致）.
    /// </summary>
    public ImmutableArray<INamedTypeSymbol> FilteredTypes { get; }

    private MaomiSymbols(
        INamedTypeSymbol moduleInterface,
        INamedTypeSymbol injectOnAttribute,
        INamedTypeSymbol injectModuleAttribute,
        INamedTypeSymbol? injectOnScopedAttribute,
        INamedTypeSymbol? injectOnSingletonAttribute,
        INamedTypeSymbol? injectOnTransientAttribute,
        INamedTypeSymbol? fromKeyedServicesAttribute,
        ImmutableArray<INamedTypeSymbol> filteredTypes)
    {
        ModuleInterface = moduleInterface;
        InjectOnAttribute = injectOnAttribute;
        InjectModuleAttribute = injectModuleAttribute;
        InjectOnScopedAttribute = injectOnScopedAttribute;
        InjectOnSingletonAttribute = injectOnSingletonAttribute;
        InjectOnTransientAttribute = injectOnTransientAttribute;
        FromKeyedServicesAttribute = fromKeyedServicesAttribute;
        FilteredTypes = filteredTypes;
    }

    /// <summary>
    /// 从编译中解析 Maomi.Core.Aot 的关键类型；未引用运行时库时返回 null.
    /// </summary>
    public static MaomiSymbols? Create(Compilation compilation)
    {
        var moduleInterface = compilation.GetTypeByMetadataName(ModuleInterfaceName);
        var injectOnAttribute = compilation.GetTypeByMetadataName(InjectOnAttributeName);
        var injectModuleAttribute = compilation.GetTypeByMetadataName(InjectModuleAttributeName);
        if (moduleInterface == null || injectOnAttribute == null || injectModuleAttribute == null)
        {
            return null;
        }

        var filteredTypes = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        foreach (var filteredTypeName in FilteredTypeNames)
        {
            var filteredType = compilation.GetTypeByMetadataName(filteredTypeName);
            if (filteredType != null)
            {
                filteredTypes.Add(filteredType);
            }
        }

        return new MaomiSymbols(
            moduleInterface,
            injectOnAttribute,
            injectModuleAttribute,
            compilation.GetTypeByMetadataName("Maomi.InjectOnScopedAttribute"),
            compilation.GetTypeByMetadataName("Maomi.InjectOnSingletonAttribute"),
            compilation.GetTypeByMetadataName("Maomi.InjectOnTransientAttribute"),
            compilation.GetTypeByMetadataName(FromKeyedServicesAttributeName),
            filteredTypes.ToImmutable());
    }

    /// <summary>
    /// 原版 ModuleOptions.FilterServiceTypes 的默认过滤集.
    /// </summary>
    private static readonly string[] FilteredTypeNames =
    {
        "System.IDisposable",
        "System.ICloneable",
        "System.IComparable"
    };

    /// <summary>
    /// 类型是否实现了 Maomi.IModule.
    /// </summary>
    public bool IsModuleType(ITypeSymbol type)
    {
        return type.AllInterfaces.Contains(ModuleInterface, SymbolEqualityComparer.Default);
    }

    /// <summary>
    /// 特性类型是否继承自 Maomi.InjectOnAttribute.
    /// </summary>
    public bool IsInjectOnAttribute(ITypeSymbol type)
    {
        return InheritsFrom(type, InjectOnAttribute);
    }

    /// <summary>
    /// 特性类型是否继承自 Maomi.InjectModuleAttribute.
    /// </summary>
    public bool IsInjectModuleAttribute(ITypeSymbol type)
    {
        return InheritsFrom(type, InjectModuleAttribute);
    }

    private static bool InheritsFrom(ITypeSymbol type, ITypeSymbol baseType)
    {
        var current = type;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }
}
