// <copyright file="MaomiAnalyzer.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace Maomi.Core.Aot.Generators;

/// <summary>
/// 符号级分析：从类型符号中提取服务模型、模块模型，并生成构造函数工厂委托代码.
/// </summary>
internal static class MaomiAnalyzer
{
    /// <summary>
    /// 提取一个标记了 InjectOn 特性的服务类模型；不满足条件时返回 null.
    /// </summary>
    public static ServiceModel? TryCreateServiceModel(MaomiSymbols symbols, INamedTypeSymbol type)
    {
        if (!IsServiceCandidate(type))
        {
            return null;
        }

        var injectOnAttribute = FindInjectOnAttribute(symbols, type);
        if (injectOnAttribute == null)
        {
            return null;
        }

        // 与原版一致：模块类不作为服务处理
        if (symbols.IsModuleType(type))
        {
            return null;
        }

        if (!TryParseInjectOn(symbols, injectOnAttribute, out var lifetime, out var scheme, out var own, out var serviceKeyLiteral, out var serviceTypes))
        {
            return null;
        }

        bool warnEmptySome = false;
        var registrations = ComputeRegistrations(symbols, type, scheme, own, serviceTypes, out warnEmptySome);

        return new ServiceModel(
            typeName: Emit.TypeName(type),
            metadataName: Emit.MetadataName(type),
            displayName: type.ToDisplayString(),
            lifetime: lifetime,
            scheme: scheme,
            own: own,
            serviceKeyLiteral: serviceKeyLiteral,
            registrations: registrations,
            warnEmptySome: warnEmptySome,
            factory: TryCreateFactory(type),
            location: SourceSpanInfo.Create(type.Locations.FirstOrDefault()));
    }

    /// <summary>
    /// 提取一个实现了 IModule 的模块类模型；不满足条件（非类、抽象、静态）时返回 null.
    /// </summary>
    public static ModuleModel? TryCreateModuleModel(MaomiSymbols symbols, INamedTypeSymbol type)
    {
        if (!symbols.IsModuleType(type))
        {
            return null;
        }

        // 模块类必须是非抽象、非静态的类（支持嵌套类）
        if (type.TypeKind != TypeKind.Class
            || type.IsAbstract
            || type.IsStatic
            || type.IsImplicitlyDeclared)
        {
            return null;
        }

        var dependencies = ImmutableArray.CreateBuilder<string>();
        foreach (var attribute in type.GetAttributes())
        {
            if (attribute.AttributeClass == null || !symbols.IsInjectModuleAttribute(attribute.AttributeClass))
            {
                continue;
            }

            // [InjectModule(typeof(X))]：依赖类型在构造函数参数中
            if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Value is ITypeSymbol moduleType)
            {
                dependencies.Add(Emit.MetadataName(moduleType));
                continue;
            }

            // [InjectModule<X>]：依赖类型在特性的泛型类型实参中（构造函数体内传给基类的参数不会出现在 AttributeData 中）
            if (attribute.AttributeClass.IsGenericType && attribute.AttributeClass.TypeArguments.Length > 0
                && attribute.AttributeClass.TypeArguments[0] is ITypeSymbol genericModuleType)
            {
                dependencies.Add(Emit.MetadataName(genericModuleType));
            }
        }

        return new ModuleModel(
            typeName: Emit.TypeName(type),
            metadataName: Emit.MetadataName(type),
            displayName: type.ToDisplayString(),
            assemblyName: type.ContainingAssembly.Name,
            dependencies: dependencies.ToImmutable(),
            factory: TryCreateFactory(type),
            location: SourceSpanInfo.Create(type.Locations.FirstOrDefault()));
    }

    /// <summary>
    /// 从 AddModule&lt;TModule&gt; / AddModule(typeof(TModule)) 调用点提取模块.
    /// </summary>
    public static ModuleModel? TryCreateModuleFromCallSite(MaomiSymbols symbols, GeneratorSyntaxContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        var method = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
        if (method == null || method.Name != "AddModule")
        {
            return null;
        }

        INamedTypeSymbol? moduleType = null;
        if (method.TypeArguments.Length == 1)
        {
            moduleType = method.TypeArguments[0] as INamedTypeSymbol;
        }
        else
        {
            var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
            if (argument?.Expression is TypeOfExpressionSyntax typeOfExpression)
            {
                moduleType = context.SemanticModel.GetTypeInfo(typeOfExpression.Type).Type as INamedTypeSymbol;
            }
        }

        if (moduleType == null)
        {
            return null;
        }

        return TryCreateModuleModel(symbols, moduleType);
    }

    /// <summary>
    /// 服务候选：非抽象、非静态的类（支持嵌套类，排除编译器生成的类型）.
    /// </summary>
    private static bool IsServiceCandidate(INamedTypeSymbol type)
    {
        return type.TypeKind == TypeKind.Class
            && !type.IsAbstract
            && !type.IsStatic
            && !type.IsImplicitlyDeclared;
    }

    private static AttributeData? FindInjectOnAttribute(MaomiSymbols symbols, INamedTypeSymbol type)
    {
        foreach (var attribute in type.GetAttributes())
        {
            if (attribute.AttributeClass != null && symbols.IsInjectOnAttribute(attribute.AttributeClass))
            {
                return attribute;
            }
        }

        return null;
    }

    private static bool TryParseInjectOn(
        MaomiSymbols symbols,
        AttributeData attribute,
        out MaomiLifetime lifetime,
        out MaomiScheme scheme,
        out bool own,
        out string? serviceKeyLiteral,
        out ImmutableArray<ITypeSymbol> serviceTypes)
    {
        lifetime = MaomiLifetime.Scoped;
        scheme = MaomiScheme.OnlyInterfaces;
        own = false;
        serviceKeyLiteral = null;
        serviceTypes = ImmutableArray<ITypeSymbol>.Empty;

        // 快捷特性（InjectOnScoped/Singleton/Transient）的生命周期在基类构造函数中设置，
        // 不会出现在 AttributeData 中，必须按特性类型推导
        var attributeClass = attribute.AttributeClass;
        if (attributeClass != null)
        {
            if (SymbolEqualityComparer.Default.Equals(attributeClass, symbols.InjectOnScopedAttribute))
            {
                lifetime = MaomiLifetime.Scoped;
            }
            else if (SymbolEqualityComparer.Default.Equals(attributeClass, symbols.InjectOnSingletonAttribute))
            {
                lifetime = MaomiLifetime.Singleton;
            }
            else if (SymbolEqualityComparer.Default.Equals(attributeClass, symbols.InjectOnTransientAttribute))
            {
                lifetime = MaomiLifetime.Transient;
            }
        }

        // 按实际应用的构造函数的参数名解析位置参数（InjectOn 基类是 (lifetime, scheme)，三个快捷特性只有 (scheme)）
        var constructor = attributeClass?.Constructors
            .FirstOrDefault(c => c.Parameters.Length == attribute.ConstructorArguments.Length);
        if (constructor != null)
        {
            for (int i = 0; i < attribute.ConstructorArguments.Length; i++)
            {
                switch (constructor.Parameters[i].Name)
                {
                    case "lifetime":
                        lifetime = ToLifetime(attribute.ConstructorArguments[i].Value);
                        break;
                    case "scheme":
                        scheme = ToScheme(attribute.ConstructorArguments[i].Value);
                        break;
                }
            }
        }

        var serviceTypeValues = ImmutableArray.CreateBuilder<ITypeSymbol>();
        foreach (var namedArgument in attribute.NamedArguments)
        {
            switch (namedArgument.Key)
            {
                case "Lifetime":
                    lifetime = ToLifetime(namedArgument.Value.Value);
                    break;
                case "Scheme":
                    scheme = ToScheme(namedArgument.Value.Value);
                    break;
                case "Own":
                    own = namedArgument.Value.Value is bool ownValue && ownValue;
                    break;
                case "ServiceKey":
                    serviceKeyLiteral = Emit.TypedConstantToLiteral(namedArgument.Value);
                    break;
                case "ServiceTypes":
                    if (namedArgument.Value.Kind == TypedConstantKind.Array)
                    {
                        foreach (var element in namedArgument.Value.Values)
                        {
                            if (element.Value is ITypeSymbol typeSymbol && typeSymbol.TypeKind != TypeKind.Error)
                            {
                                serviceTypeValues.Add(typeSymbol);
                            }
                        }
                    }

                    break;
            }
        }

        serviceTypes = serviceTypeValues.ToImmutable();

        // 无法识别的取值按默认处理
        if (lifetime < 0 || scheme < 0)
        {
            return false;
        }

        return true;
    }

    private static MaomiLifetime ToLifetime(object? value)
    {
        int intValue = value switch
        {
            int i => i,
            Enum e => Convert.ToInt32(e),
            _ => -1
        };

        if (intValue < 0 || intValue > (int)MaomiLifetime.Singleton)
        {
            return (MaomiLifetime)(-1);
        }

        return (MaomiLifetime)intValue;
    }

    private static MaomiScheme ToScheme(object? value)
    {
        int intValue = value switch
        {
            int i => i,
            Enum e => Convert.ToInt32(e),
            _ => -1
        };

        if (intValue < 0 || intValue > (int)MaomiScheme.None)
        {
            return (MaomiScheme)(-1);
        }

        return (MaomiScheme)intValue;
    }

    /// <summary>
    /// 依据 InjectScheme 语义解析要注册的服务类型列表（修正版语义：仅当没有任何候选或 Own = true 时才注册实现类自身）.
    /// </summary>
    private static ImmutableArray<string> ComputeRegistrations(
        MaomiSymbols symbols,
        INamedTypeSymbol type,
        MaomiScheme scheme,
        bool own,
        ImmutableArray<ITypeSymbol> serviceTypes,
        out bool warnEmptySome)
    {
        warnEmptySome = false;
        var registrations = ImmutableArray.CreateBuilder<string>();
        var filteredTypes = GetFilteredTypes(symbols);
        var selfType = Emit.TypeName(type);

        switch (scheme)
        {
            case MaomiScheme.None:
            {
                if (own)
                {
                    registrations.Add(selfType);
                }

                break;
            }

            case MaomiScheme.Some:
            {
                foreach (var serviceType in serviceTypes)
                {
                    registrations.Add(Emit.TypeName(serviceType));
                }

                if (registrations.Count == 0)
                {
                    warnEmptySome = true;
                }

                if (own && !registrations.Contains(selfType))
                {
                    registrations.Add(selfType);
                }

                break;
            }

            case MaomiScheme.OnlyInterfaces:
            case MaomiScheme.OnlyBaseClass:
            case MaomiScheme.Any:
            {
                if (scheme == MaomiScheme.OnlyInterfaces || scheme == MaomiScheme.Any)
                {
                    foreach (var interfaceType in type.AllInterfaces)
                    {
                        if (filteredTypes.Contains(interfaceType, SymbolEqualityComparer.Default))
                        {
                            continue;
                        }

                        registrations.Add(Emit.TypeName(interfaceType));
                    }
                }

                if (scheme == MaomiScheme.OnlyBaseClass || scheme == MaomiScheme.Any)
                {
                    var baseType = type.BaseType;
                    if (baseType != null
                        && baseType.SpecialType != SpecialType.System_Object
                        && !filteredTypes.Contains(baseType, SymbolEqualityComparer.Default))
                    {
                        registrations.Add(Emit.TypeName(baseType));
                    }
                }

                // 修正版语义：没有任何候选、或 Own = true 时注册实现类自身
                if ((registrations.Count == 0 || own) && !registrations.Contains(selfType))
                {
                    registrations.Add(selfType);
                }

                break;
            }
        }

        return registrations.ToImmutable();
    }

    private static ImmutableArray<INamedTypeSymbol> GetFilteredTypes(MaomiSymbols symbols)
    {
        return symbols.FilteredTypes;
    }

    /// <summary>
    /// 分析构造函数并生成工厂委托；无法生成时记录回退原因.
    /// </summary>
    private static FactoryInfo TryCreateFactory(INamedTypeSymbol type)
    {
        // 开放式泛型：无法生成 new T<>(); 由容器在运行时处理
        if (type.IsGenericType && type.IsDefinition)
        {
            return FactoryInfo.CreateFallback("开放式泛型类型");
        }

        var constructors = type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor
                && m.DeclaredAccessibility == Accessibility.Public
                && !m.IsStatic
                && (m.Parameters.Length == 0 || !m.IsImplicitlyDeclared))
            .ToList();

        if (constructors.Count == 0)
        {
            return FactoryInfo.CreateFallback("没有公共构造函数");
        }

        if (constructors.Count > 1)
        {
            return FactoryInfo.CreateFallback("存在多个公共构造函数");
        }

        var constructor = constructors[0];
        var parameters = new string[constructor.Parameters.Length];
        for (int i = 0; i < constructor.Parameters.Length; i++)
        {
            var parameter = constructor.Parameters[i];
            var expression = TryCreateParameterExpression(parameter);
            if (expression == null)
            {
                return FactoryInfo.CreateFallback($"构造函数参数 '{parameter.Name}' 不受支持");
            }

            parameters[i] = expression;
        }

        return FactoryInfo.CreateFactory($"new {Emit.TypeName(type)}({string.Join(", ", parameters)})");
    }

    /// <summary>
    /// 为构造函数参数生成取值表达式；不受支持时返回 null.
    /// </summary>
    private static string? TryCreateParameterExpression(IParameterSymbol parameter)
    {
        var parameterType = parameter.Type;
        if (parameterType.TypeKind == TypeKind.Error || parameterType is not INamedTypeSymbol namedType)
        {
            return null;
        }

        // [FromKeyedServices] 键控参数
        foreach (var attribute in parameter.GetAttributes())
        {
            if (attribute.AttributeClass == null)
            {
                continue;
            }

            if (string.Equals(
                attribute.AttributeClass.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                MaomiSymbols.FromKeyedServicesAttributeName,
                System.StringComparison.Ordinal)
                && attribute.ConstructorArguments.Length > 0)
            {
                var keyLiteral = Emit.TypedConstantToLiteral(attribute.ConstructorArguments[0]);
                if (keyLiteral != null)
                {
                    return $"sp.GetRequiredKeyedService<{Emit.TypeName(parameterType)}>({keyLiteral})";
                }
            }
        }

        // 基础类型参数：只能使用默认值
        if (IsPrimitiveLike(parameterType))
        {
            if (parameter.HasExplicitDefaultValue)
            {
                return Emit.ValueToLiteral(parameter.ExplicitDefaultValue, parameterType);
            }

            return null;
        }

        // IServiceProvider 参数直接传递
        if (namedType.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) == "System.IServiceProvider")
        {
            return "sp";
        }

        return $"sp.GetRequiredService<{Emit.TypeName(parameterType)}>()";
    }

    private static bool IsPrimitiveLike(ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Enum || type.TypeKind == TypeKind.Array)
        {
            return true;
        }

        return type.SpecialType switch
        {
            SpecialType.System_Boolean
                or SpecialType.System_Char
                or SpecialType.System_SByte
                or SpecialType.System_Byte
                or SpecialType.System_Int16
                or SpecialType.System_UInt16
                or SpecialType.System_Int32
                or SpecialType.System_UInt32
                or SpecialType.System_Int64
                or SpecialType.System_UInt64
                or SpecialType.System_Single
                or SpecialType.System_Double
                or SpecialType.System_Decimal
                or SpecialType.System_String => true,
            _ => false
        };
    }
}
