// <copyright file="MaomiAotGenerator.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Maomi.Core.Aot.Generators;

/// <summary>
/// Maomi.Core.Aot 源生成器入口：增量管道编排.
/// </summary>
[Generator]
public sealed class MaomiAotGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 服务候选：带特性的类
        var serviceCandidates = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is ClassDeclarationSyntax classDeclaration && classDeclaration.AttributeLists.Count > 0,
            transform: static (syntaxContext, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node, cancellationToken) is not INamedTypeSymbol typeSymbol)
                {
                    return null;
                }

                var symbols = MaomiSymbols.Create(syntaxContext.SemanticModel.Compilation);
                if (symbols == null)
                {
                    return null;
                }

                return MaomiAnalyzer.TryCreateServiceModel(symbols, typeSymbol);
            });

        // 模块候选：实现了 IModule 的类
        var moduleCandidates = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is ClassDeclarationSyntax classDeclaration && classDeclaration.BaseList != null,
            transform: static (syntaxContext, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node, cancellationToken) is not INamedTypeSymbol typeSymbol)
                {
                    return null;
                }

                var symbols = MaomiSymbols.Create(syntaxContext.SemanticModel.Compilation);
                if (symbols == null)
                {
                    return null;
                }

                return MaomiAnalyzer.TryCreateModuleModel(symbols, typeSymbol);
            });

        // AddModule<TModule>() / AddModule(typeof(TModule)) 调用点
        var addModuleCallSites = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is InvocationExpressionSyntax invocation
                && ((invocation.Expression is GenericNameSyntax genericName && genericName.Identifier.ValueText == "AddModule")
                    || (invocation.Expression is IdentifierNameSyntax identifierName && identifierName.Identifier.ValueText == "AddModule")),
            transform: static (syntaxContext, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var symbols = MaomiSymbols.Create(syntaxContext.SemanticModel.Compilation);
                if (symbols == null)
                {
                    return null;
                }

                return MaomiAnalyzer.TryCreateModuleFromCallSite(symbols, syntaxContext);
            });

        var combined = serviceCandidates.Collect()
            .Combine(moduleCandidates.Collect())
            .Combine(addModuleCallSites.Collect())
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combined, static (productionContext, source) =>
        {
            var (((services, modules), callSites), compilation) = source;
            GenerateRegistry(productionContext, compilation, services, modules, callSites);
        });
    }

    private static void GenerateRegistry(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<ServiceModel?> services,
        ImmutableArray<ModuleModel?> sourceModules,
        ImmutableArray<ModuleModel?> callSiteModules)
    {
        var symbols = MaomiSymbols.Create(compilation);
        if (symbols == null)
        {
            return;
        }

        // 1. 建立模块图（源模块与 AddModule 调用点模块）
        var moduleMap = new Dictionary<string, ModuleModel>(StringComparer.Ordinal);
        foreach (var module in sourceModules)
        {
            AddModuleUnique(moduleMap, module);
        }

        foreach (var module in callSiteModules)
        {
            AddModuleUnique(moduleMap, module);
        }

        if (moduleMap.Count == 0)
        {
            // 没有任何模块时，源服务不会被注册（与原版扫描范围语义一致）
            foreach (var service in services)
            {
                if (service != null)
                {
                    Report(context, Diagnostics.ServiceWithoutModule, service.Location, service.DisplayName, compilation.Assembly.Name);
                }
            }

            return;
        }

        // 2. 通过 [InjectModule] 依赖闭包展开模块图（依赖模块可能在引用程序集中）
        var hasInvalidDependency = false;
        var pendingModules = new Queue<string>(moduleMap.Keys);
        while (pendingModules.Count > 0)
        {
            var moduleName = pendingModules.Dequeue();
            var module = moduleMap[moduleName];
            foreach (var dependencyName in module.Dependencies)
            {
                if (moduleMap.ContainsKey(dependencyName))
                {
                    continue;
                }

                var dependencySymbol = compilation.GetTypeByMetadataName(dependencyName);
                var dependencyModel = dependencySymbol == null ? null : MaomiAnalyzer.TryCreateModuleModel(symbols, dependencySymbol);
                if (dependencyModel == null)
                {
                    Report(context, Diagnostics.InvalidDependencyModule, module.Location, module.DisplayName, dependencyName);
                    hasInvalidDependency = true;
                    continue;
                }

                moduleMap.Add(dependencyName, dependencyModel);
                pendingModules.Enqueue(dependencyName);
            }
        }

        if (hasInvalidDependency)
        {
            // 存在无效的依赖模块：报错并跳过生成
            return;
        }

        // 3. 循环依赖检测（编译期报错）
        if (TryFindLoop(moduleMap, out var loopPath))
        {
            var loopNames = FormatLoopPath(moduleMap, loopPath);
            var firstLocation = moduleMap.TryGetValue(loopPath[0], out var firstLoopModule) ? firstLoopModule.Location : null;
            Report(context, Diagnostics.LoopDependency, firstLocation, loopNames);
            return;
        }

        // 4. 划分每个模块程序集的服务注册代码块
        var chunkAssemblies = new List<IAssemblySymbol>();
        var seenAssemblies = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
        foreach (var moduleName in moduleMap.Keys.OrderBy(name => name, StringComparer.Ordinal))
        {
            var moduleSymbol = compilation.GetTypeByMetadataName(moduleName);
            if (moduleSymbol?.ContainingAssembly == null)
            {
                continue;
            }

            if (seenAssemblies.Add(moduleSymbol.ContainingAssembly))
            {
                chunkAssemblies.Add(moduleSymbol.ContainingAssembly);
            }
        }

        var usedChunkMethodNames = new HashSet<string>(StringComparer.Ordinal);
        var chunkMethodNames = new Dictionary<IAssemblySymbol, string>(SymbolEqualityComparer.Default);
        foreach (var assembly in chunkAssemblies)
        {
            chunkMethodNames[assembly] = MakeChunkMethodName(assembly.Name, usedChunkMethodNames);
        }

        var chunkServices = new List<(string MethodName, List<ServiceModel> Services)>();
        foreach (var assembly in chunkAssemblies)
        {
            var servicesInAssembly = new List<ServiceModel>();
            if (SymbolEqualityComparer.Default.Equals(assembly, compilation.Assembly))
            {
                // 源程序集：直接使用语法候选结果
                foreach (var service in services)
                {
                    if (service != null)
                    {
                        servicesInAssembly.Add(service);
                    }
                }
            }
            else
            {
                // 引用程序集：遍历程序集中声明的类型
                var assemblyTypes = new List<INamedTypeSymbol>();
                CollectAssemblyTypes(assembly.GlobalNamespace, assemblyTypes);
                foreach (var type in assemblyTypes)
                {
                    var service = MaomiAnalyzer.TryCreateServiceModel(symbols, type);
                    if (service != null)
                    {
                        servicesInAssembly.Add(service);
                    }
                }
            }

            servicesInAssembly.Sort((a, b) => StringComparer.Ordinal.Compare(a.MetadataName, b.MetadataName));
            chunkServices.Add((chunkMethodNames[assembly], servicesInAssembly));
        }

        // 源程序集没有任何模块时，源服务不会被注册
        if (!seenAssemblies.Contains(compilation.Assembly))
        {
            foreach (var service in services)
            {
                if (service != null)
                {
                    Report(context, Diagnostics.ServiceWithoutModule, service.Location, service.DisplayName, compilation.Assembly.Name);
                }
            }
        }

        // 5. 回退与告警诊断
        foreach (var module in moduleMap.Values.OrderBy(module => module.MetadataName, StringComparer.Ordinal))
        {
            if (!module.Factory.HasFactory)
            {
                Report(context, Diagnostics.ReflectionFallback, module.Location, module.DisplayName, module.Factory.FallbackReason);
            }
        }

        foreach (var (_, servicesInAssembly) in chunkServices)
        {
            foreach (var service in servicesInAssembly)
            {
                if (!service.Factory.HasFactory)
                {
                    Report(context, Diagnostics.ReflectionFallback, service.Location, service.DisplayName, service.Factory.FallbackReason);
                }

                if (service.WarnEmptySome)
                {
                    Report(context, Diagnostics.EmptySomeServiceTypes, service.Location, service.DisplayName);
                }
            }
        }

        // 6. 生成注册表代码
        var orderedModules = moduleMap.Values
            .OrderBy(module => module.MetadataName, StringComparer.Ordinal)
            .ToList();
        var chunkMethodNamesByAssemblyName = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var assembly in chunkAssemblies)
        {
            chunkMethodNamesByAssemblyName[assembly.Name] = chunkMethodNames[assembly];
        }

        var code = RegistryEmitter.Emit(orderedModules, chunkServices, chunkMethodNamesByAssemblyName);
        context.AddSource("MaomiAotRegistry.g.cs", code);
    }

    private static void AddModuleUnique(Dictionary<string, ModuleModel> moduleMap, ModuleModel? module)
    {
        if (module != null && !moduleMap.ContainsKey(module.MetadataName))
        {
            moduleMap.Add(module.MetadataName, module);
        }
    }

    /// <summary>
    /// 检测模块依赖图中的环（深度优先，路径栈实现）.
    /// </summary>
    private static bool TryFindLoop(Dictionary<string, ModuleModel> moduleMap, out List<string> loopPath)
    {
        var visiting = new List<string>();
        var done = new HashSet<string>(StringComparer.Ordinal);
        var foundPath = new List<string>();
        var found = false;

        foreach (var name in moduleMap.Keys.OrderBy(name => name, StringComparer.Ordinal))
        {
            if (Visit(name))
            {
                found = true;
                break;
            }
        }

        loopPath = foundPath;
        return found;

        bool Visit(string name)
        {
            if (done.Contains(name))
            {
                return false;
            }

            var index = visiting.IndexOf(name);
            if (index >= 0)
            {
                foundPath = visiting.Skip(index).Concat(new[] { name }).ToList();
                return true;
            }

            visiting.Add(name);
            foreach (var dependency in moduleMap[name].Dependencies)
            {
                if (moduleMap.ContainsKey(dependency) && Visit(dependency))
                {
                    return true;
                }
            }

            visiting.RemoveAt(visiting.Count - 1);
            done.Add(name);
            return false;
        }
    }

    /// <summary>
    /// 将环路径格式化为可读文本.
    /// </summary>
    private static string FormatLoopPath(Dictionary<string, ModuleModel> moduleMap, List<string> loopPath)
    {
        var builder = new StringBuilder();
        var first = true;
        foreach (var name in loopPath)
        {
            if (!first)
            {
                builder.Append(" -> ");
            }

            if (moduleMap.TryGetValue(name, out var loopModule))
            {
                builder.Append(loopModule.DisplayName);
            }
            else
            {
                builder.Append(name);
            }

            first = false;
        }

        return builder.ToString();
    }

    /// <summary>
    /// 生成程序集服务注册代码块的方法名（同名字段冲突时追加序号）.
    /// </summary>
    private static string MakeChunkMethodName(string assemblyName, HashSet<string> usedNames)
    {
        var builder = new StringBuilder("Register__");
        foreach (var c in assemblyName)
        {
            builder.Append(char.IsLetterOrDigit(c) ? c : '_');
        }

        var methodName = builder.ToString();
        if (string.IsNullOrEmpty(assemblyName))
        {
            methodName = "Register__Assembly";
        }

        var suffix = 0;
        while (!usedNames.Add(methodName))
        {
            suffix++;
            methodName = string.Concat(builder.ToString(), "_", suffix.ToString(CultureInfo.InvariantCulture));
        }

        return methodName;
    }

    private static void CollectAssemblyTypes(INamespaceOrTypeSymbol container, List<INamedTypeSymbol> results)
    {
        foreach (var member in container.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol namespaceSymbol:
                    CollectAssemblyTypes(namespaceSymbol, results);
                    break;
                case INamedTypeSymbol typeSymbol:
                    results.Add(typeSymbol);
                    CollectAssemblyTypes(typeSymbol, results);
                    break;
            }
        }
    }

    private static void Report(SourceProductionContext context, DiagnosticDescriptor descriptor, SourceSpanInfo? location, params object?[] messageArgs)
    {
        Location? roslynLocation = null;
        if (location.HasValue)
        {
            try
            {
                roslynLocation = location.Value.ToLocation();
            }
            catch
            {
                roslynLocation = null;
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(descriptor, roslynLocation ?? Location.None, messageArgs));
    }
}