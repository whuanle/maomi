// <copyright file="Models.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Maomi.Core.Aot.Generators;

/// <summary>
/// 服务生命周期（与 Microsoft.Extensions.DependencyInjection.ServiceLifetime 的取值一致）.
/// </summary>
internal enum MaomiLifetime
{
    Transient = 0,
    Scoped = 1,
    Singleton = 2
}

/// <summary>
/// 服务注册模式（与 Maomi.InjectScheme 的取值一致）.
/// </summary>
internal enum MaomiScheme
{
    Any = 0,
    Some = 1,
    OnlyBaseClass = 2,
    OnlyInterfaces = 3,
    None = 4
}

/// <summary>
/// 源码位置的可比较快照，用于在增量管道中传递诊断位置.
/// </summary>
internal readonly struct SourceSpanInfo : IEquatable<SourceSpanInfo>
{
    public string FilePath { get; }
    public TextSpan Span { get; }
    public LinePositionSpan LineSpan { get; }

    private SourceSpanInfo(string filePath, TextSpan span, LinePositionSpan lineSpan)
    {
        FilePath = filePath;
        Span = span;
        LineSpan = lineSpan;
    }

    public static SourceSpanInfo? Create(Location? location)
    {
        if (location == null || !location.IsInSource)
        {
            return null;
        }

        var lineSpan = location.GetLineSpan();
        return new SourceSpanInfo(lineSpan.Path, location.SourceSpan, new LinePositionSpan(lineSpan.StartLinePosition, lineSpan.EndLinePosition));
    }

    public Location ToLocation()
    {
        return Location.Create(FilePath, Span, LineSpan);
    }

    public bool Equals(SourceSpanInfo other)
    {
        return FilePath == other.FilePath && Span.Equals(other.Span) && LineSpan.Equals(other.LineSpan);
    }

    public override bool Equals(object? obj)
    {
        return obj is SourceSpanInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = FilePath.GetHashCode();
            hashCode = (hashCode * 397) ^ Span.GetHashCode();
            hashCode = (hashCode * 397) ^ LineSpan.GetHashCode();
            return hashCode;
        }
    }
}

/// <summary>
/// 构造函数工厂信息：能生成工厂委托时 <see cref="HasFactory"/> 为 true，否则记录回退原因.
/// </summary>
internal sealed class FactoryInfo : IEquatable<FactoryInfo>
{
    /// <summary>
    /// 是否生成了工厂委托.
    /// </summary>
    public bool HasFactory { get; }

    /// <summary>
    /// 工厂委托表达式：new Type(param1, param2).
    /// </summary>
    public string FactoryBody { get; }

    /// <summary>
    /// 无法生成工厂委托时的原因.
    /// </summary>
    public string FallbackReason { get; }

    private FactoryInfo(bool hasFactory, string factoryBody, string fallbackReason)
    {
        HasFactory = hasFactory;
        FactoryBody = factoryBody;
        FallbackReason = fallbackReason;
    }

    public static FactoryInfo CreateFactory(string factoryBody)
    {
        return new FactoryInfo(true, factoryBody, string.Empty);
    }

    public static FactoryInfo CreateFallback(string reason)
    {
        return new FactoryInfo(false, string.Empty, reason);
    }

    public bool Equals(FactoryInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        return HasFactory == other.HasFactory
            && FactoryBody == other.FactoryBody
            && FallbackReason == other.FallbackReason;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as FactoryInfo);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = HasFactory.GetHashCode();
            hashCode = (hashCode * 397) ^ FactoryBody.GetHashCode();
            hashCode = (hashCode * 397) ^ FallbackReason.GetHashCode();
            return hashCode;
        }
    }
}

/// <summary>
/// 一个标记了 InjectOn 特性的服务的完整分析结果.
/// </summary>
internal sealed class ServiceModel : IEquatable<ServiceModel>
{
    /// <summary>
    /// 实现类型的发射字符串（global:: 前缀）.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// 实现类型的元数据名（用于排序和去重）.
    /// </summary>
    public string MetadataName { get; }

    /// <summary>
    /// 诊断显示名.
    /// </summary>
    public string DisplayName { get; }

    public MaomiLifetime Lifetime { get; }

    public MaomiScheme Scheme { get; }

    public bool Own { get; }

    /// <summary>
    /// ServiceKey 字面量；null 表示非键控服务.
    /// </summary>
    public string? ServiceKeyLiteral { get; }

    /// <summary>
    /// 依据 InjectScheme 语义解析出的服务类型列表（发射字符串，可能包含实现类型自身）.
    /// </summary>
    public ImmutableArray<string> Registrations { get; }

    /// <summary>
    /// Some 模式下 ServiceTypes 为空.
    /// </summary>
    public bool WarnEmptySome { get; }

    public FactoryInfo Factory { get; }

    public SourceSpanInfo? Location { get; }

    public ServiceModel(
        string typeName,
        string metadataName,
        string displayName,
        MaomiLifetime lifetime,
        MaomiScheme scheme,
        bool own,
        string? serviceKeyLiteral,
        ImmutableArray<string> registrations,
        bool warnEmptySome,
        FactoryInfo factory,
        SourceSpanInfo? location)
    {
        TypeName = typeName;
        MetadataName = metadataName;
        DisplayName = displayName;
        Lifetime = lifetime;
        Scheme = scheme;
        Own = own;
        ServiceKeyLiteral = serviceKeyLiteral;
        Registrations = registrations;
        WarnEmptySome = warnEmptySome;
        Factory = factory;
        Location = location;
    }

    public bool Equals(ServiceModel? other)
    {
        if (other is null)
        {
            return false;
        }

        return TypeName == other.TypeName
            && MetadataName == other.MetadataName
            && DisplayName == other.DisplayName
            && Lifetime == other.Lifetime
            && Scheme == other.Scheme
            && Own == other.Own
            && ServiceKeyLiteral == other.ServiceKeyLiteral
            && Registrations.SequenceEqual(other.Registrations)
            && WarnEmptySome == other.WarnEmptySome
            && Factory.Equals(other.Factory)
            && Nullable.Equals(Location, other.Location);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ServiceModel);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = TypeName.GetHashCode();
            hashCode = (hashCode * 397) ^ MetadataName.GetHashCode();
            hashCode = (hashCode * 397) ^ DisplayName.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)Lifetime;
            hashCode = (hashCode * 397) ^ (int)Scheme;
            hashCode = (hashCode * 397) ^ Own.GetHashCode();
            hashCode = (hashCode * 397) ^ (ServiceKeyLiteral?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ Registrations.Length;
            hashCode = (hashCode * 397) ^ WarnEmptySome.GetHashCode();
            hashCode = (hashCode * 397) ^ Factory.GetHashCode();
            hashCode = (hashCode * 397) ^ (Location?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
}

/// <summary>
/// 一个实现了 Maomi.IModule 的模块类的完整分析结果.
/// </summary>
internal sealed class ModuleModel : IEquatable<ModuleModel>
{
    /// <summary>
    /// 模块类型的发射字符串（global:: 前缀）.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// 模块类型的元数据名（模块图 key）.
    /// </summary>
    public string MetadataName { get; }

    /// <summary>
    /// 诊断显示名.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 模块所在程序集名（用于划分服务注册代码块）.
    /// </summary>
    public string AssemblyName { get; }

    /// <summary>
    /// 依赖的模块元数据名列表（按 [InjectModule] 特性声明顺序）.
    /// </summary>
    public ImmutableArray<string> Dependencies { get; }

    public FactoryInfo Factory { get; }

    public SourceSpanInfo? Location { get; }

    public ModuleModel(
        string typeName,
        string metadataName,
        string displayName,
        string assemblyName,
        ImmutableArray<string> dependencies,
        FactoryInfo factory,
        SourceSpanInfo? location)
    {
        TypeName = typeName;
        MetadataName = metadataName;
        DisplayName = displayName;
        AssemblyName = assemblyName;
        Dependencies = dependencies;
        Factory = factory;
        Location = location;
    }

    public bool Equals(ModuleModel? other)
    {
        if (other is null)
        {
            return false;
        }

        return TypeName == other.TypeName
            && MetadataName == other.MetadataName
            && DisplayName == other.DisplayName
            && AssemblyName == other.AssemblyName
            && Dependencies.SequenceEqual(other.Dependencies)
            && Factory.Equals(other.Factory)
            && Nullable.Equals(Location, other.Location);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ModuleModel);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = TypeName.GetHashCode();
            hashCode = (hashCode * 397) ^ MetadataName.GetHashCode();
            hashCode = (hashCode * 397) ^ DisplayName.GetHashCode();
            hashCode = (hashCode * 397) ^ AssemblyName.GetHashCode();
            hashCode = (hashCode * 397) ^ Dependencies.Length;
            hashCode = (hashCode * 397) ^ Factory.GetHashCode();
            hashCode = (hashCode * 397) ^ (Location?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
}

/// <summary>
/// 字面量与类型名的发射工具.
/// </summary>
internal static class Emit
{
    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    /// <summary>
    /// 生成可用于 typeof / 泛型参数的完全限定类型名（global:: 前缀），开放式泛型输出形如 Foo&lt;&gt;.
    /// </summary>
    public static string TypeName(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType && !namedType.IsUnboundGenericType && namedType.IsDefinition)
        {
            return namedType.ConstructUnboundGenericType().ToDisplayString(FullyQualifiedFormat);
        }

        return type.ToDisplayString(FullyQualifiedFormat);
    }

    /// <summary>
    /// 生成模块图使用的元数据名（与 <see cref="Compilation.GetTypeByMetadataName(string)"/> 兼容：嵌套类型用 + 连接，泛型带参数个数）.
    /// </summary>
    public static string MetadataName(ITypeSymbol type)
    {
        var nestedParts = new List<string>();
        var current = type;
        while (current is INamedTypeSymbol namedType)
        {
            nestedParts.Insert(0, namedType.OriginalDefinition.MetadataName);
            current = namedType.ContainingType;
        }

        var namespaceDisplay = string.Empty;
        if (type.ContainingNamespace != null && !type.ContainingNamespace.IsGlobalNamespace)
        {
            namespaceDisplay = type.ContainingNamespace.ToDisplayString();
        }

        var nestedName = string.Join("+", nestedParts);
        return namespaceDisplay.Length == 0 ? nestedName : namespaceDisplay + "." + nestedName;
    }

    /// <summary>
    /// 将 TypedConstant 转为 C# 字面量（ServiceKey 等场景）.
    /// </summary>
    public static string? TypedConstantToLiteral(TypedConstant constant)
    {
        switch (constant.Kind)
        {
            case TypedConstantKind.Primitive:
                return ValueToLiteral(constant.Value, constant.Type);
            case TypedConstantKind.Enum:
                var enumValue = Convert.ToInt64(constant.Value, CultureInfo.InvariantCulture);
                return $"({TypeName(constant.Type!)}){enumValue}";
            case TypedConstantKind.Type:
                if (constant.Value is ITypeSymbol typeSymbol)
                {
                    return $"typeof({TypeName(typeSymbol)})";
                }

                return null;
            default:
                return null;
        }
    }

    /// <summary>
    /// 将装箱值转为 C# 字面量（构造函数默认值等场景）.
    /// </summary>
    public static string ValueToLiteral(object? value, ITypeSymbol? targetType)
    {
        if (value == null)
        {
            return "default";
        }

        switch (value)
        {
            case string s:
                return QuoteString(s);
            case char c:
                return $"'{EscapeChar(c)}'";
            case bool b:
                return b ? "true" : "false";
            case float f:
                return FormattableString.Invariant($"{f}f");
            case double d:
                return FormattableString.Invariant($"{d}");
            case decimal m:
                return FormattableString.Invariant($"{m}m");
            case ulong ul:
                return FormattableString.Invariant($"{ul}UL");
            case Enum enumValue:
                if (targetType != null)
                {
                    return $"({TypeName(targetType)}){Convert.ToInt64(enumValue, CultureInfo.InvariantCulture)}";
                }

                return Convert.ToInt64(enumValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        }

        var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "default";
        switch (targetType?.SpecialType)
        {
            case SpecialType.System_Single:
                return FormattableString.Invariant($"{text}f");
            case SpecialType.System_Decimal:
                return FormattableString.Invariant($"{text}m");
            case SpecialType.System_UInt64:
                return FormattableString.Invariant($"{text}UL");
        }

        return text;
    }

    public static string QuoteString(string value)
    {
        var builder = new StringBuilder(value.Length + 2);
        builder.Append('"');
        foreach (var c in value)
        {
            switch (c)
            {
                case '"': builder.Append("\\\""); break;
                case '\\': builder.Append("\\\\"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                default:
                    if (char.IsControl(c))
                    {
                        builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(c);
                    }

                    break;
            }
        }

        builder.Append('"');
        return builder.ToString();
    }

    private static string EscapeChar(char c)
    {
        return c switch
        {
            '\'' => "\\'",
            '\\' => "\\\\",
            '\n' => "\\n",
            '\r' => "\\r",
            '\t' => "\\t",
            _ => c.ToString()
        };
    }
}
