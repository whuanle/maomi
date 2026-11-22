// <copyright file="GeneratorTestRunner.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Maomi.Core.Aot.Generator.Tests;

/// <summary>
/// 生成器测试辅助：在内存中编译源码并驱动源生成器.
/// </summary>
internal static class GeneratorTestRunner
{
    /// <summary>
    /// 生成器输出的单个源文件.
    /// </summary>
    public sealed record GeneratedSource(string HintName, string SourceText);

    /// <summary>
    /// 运行源生成器.
    /// </summary>
    /// <param name="userSource">用户源码.</param>
    /// <param name="extraReferences">额外的程序集引用（用于跨程序集测试）.</param>
    /// <returns>生成器诊断与输出文件.</returns>
    public static (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<GeneratedSource> Sources) RunGenerator(
        string userSource,
        IEnumerable<MetadataReference>? extraReferences = null)
    {
        var references = LoadDefaultReferences();
        if (extraReferences != null)
        {
            references = references.AddRange(extraReferences);
        }

        var compilation = CSharpCompilation.Create(
            assemblyName: "Maomi.Core.Aot.Generator.Tests.TestAssembly",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(userSource) },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        var driver = CSharpGeneratorDriver.Create(new Maomi.Core.Aot.Generators.MaomiAotGenerator().AsSourceGenerator());
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        var generated = outputCompilation.SyntaxTrees
            .Where(tree => tree.FilePath.EndsWith(".g.cs", StringComparison.Ordinal))
            .Select(tree => new GeneratedSource(tree.FilePath, tree.ToString()))
            .ToImmutableArray();

        return (generatorDiagnostics, generated);
    }

    /// <summary>
    /// 编译一个库并返回其元数据引用（用于跨程序集测试）.
    /// </summary>
    /// <param name="source">库源码.</param>
    /// <returns>元数据引用.</returns>
    public static MetadataReference CompileLibrary(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "Maomi.Core.Aot.Generator.Tests.Library",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source) },
            references: LoadDefaultReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        var diagnostics = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);
        if (diagnostics.Any())
        {
            throw new InvalidOperationException("库源码编译失败: " + string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString())));
        }

        return compilation.ToMetadataReference();
    }

    /// <summary>
    /// 断言编译无错误并返回生成器诊断.
    /// </summary>
    public static ImmutableArray<Diagnostic> AssertNoCompilationErrors(ImmutableArray<Diagnostic> diagnostics)
    {
        var compilationErrors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error && d.Id.StartsWith("CS", StringComparison.Ordinal)).ToList();
        if (compilationErrors.Any())
        {
            throw new InvalidOperationException("源码存在编译错误: " + string.Join(Environment.NewLine, compilationErrors.Select(d => d.ToString())));
        }

        return diagnostics;
    }

    /// <summary>
    /// 加载默认引用：运行时核心程序集 + 测试输出目录中的依赖程序集.
    /// </summary>
    private static ImmutableArray<MetadataReference> LoadDefaultReferences()
    {
        var references = new Dictionary<string, MetadataReference>(StringComparer.OrdinalIgnoreCase);

        // 运行时核心程序集（System.Runtime、System.Linq 等）
        var runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        AddReferencesFromDirectory(references, runtimeDirectory);

        // 测试输出目录中的依赖（Maomi.Core.Aot、Microsoft.Extensions.*）
        AddReferencesFromDirectory(references, AppContext.BaseDirectory);

        return references.Values.ToImmutableArray();
    }

    private static readonly string[] NativeDllMarkers =
    {
        "coreclr", "clrjit", "clretwrc", "clrgc", "clrsynth", "hostpolicy", "hostfxr", "hostdx",
        "nethost", "mscord", "msquic", "createdump", "Native", "api-ms-win"
    };

    private static void AddReferencesFromDirectory(Dictionary<string, MetadataReference> references, string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*.dll"))
        {
            var fileName = Path.GetFileName(file);
            if (references.ContainsKey(fileName))
            {
                continue;
            }

            // 跳过原生 DLL（coreclr.dll、msquic.dll、*Native*.dll 等）
            if (NativeDllMarkers.Any(marker => fileName.Contains(marker, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            try
            {
                references.Add(fileName, MetadataReference.CreateFromFile(file));
            }
            catch (BadImageFormatException)
            {
                // 跳过非托管程序集
            }
        }
    }
}
