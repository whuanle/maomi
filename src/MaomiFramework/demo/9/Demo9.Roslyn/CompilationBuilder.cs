using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;

public partial class Program
{
    /// <summary>
    /// 程序集编译构建器
    /// </summary>
    public class CompilationBuilder
    {
        /// <summary>
        /// 通过代码生成程序集
        /// </summary>
        /// <param name="code">代码</param>
        /// <param name="assemblyPath">程序集路径</param>
        /// <param name="assemblyName">程序集名称</param>
        /// <param name="option">程序集配置</param>
        /// <param name="messages">编译时的消息</param>
        /// <returns></returns>
        public static bool CreateDomain(string code,
            string assemblyPath,
            string assemblyName, 
            DomainOptionBuilder option,
            out ImmutableArray<Diagnostic> messages)
        {
            HashSet<PortableExecutableReference> references = new HashSet<PortableExecutableReference>();

            // 设置依赖的程序集列表，这里使用跟 Demo9.Roslyn 一样的依赖
            // 读者可以根据自己的需求添加
            var refAssemblys = AppDomain.CurrentDomain.GetAssemblies()
               .Where(i => !i.IsDynamic && !string.IsNullOrWhiteSpace(i.Location))
               .Distinct()
               .Select(i => MetadataReference.CreateFromFile(i.Location)).ToList();
            foreach(var item in refAssemblys)
            {
                references.Add(item);
            }

            CSharpCompilationOptions options = (option ?? new DomainOptionBuilder()).Build();

            var syntaxTree = ParseToSyntaxTree(code, option);
            var result = BuildCompilation(assemblyPath, assemblyName, new SyntaxTree[] { syntaxTree }, references.ToArray(), options);
            messages = result.Diagnostics;
            return result.Success;
        }

        /// <summary>
        /// 将代码转为语法树
        /// </summary>
        /// <param name="code"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SyntaxTree ParseToSyntaxTree(string code, DomainOptionBuilder option)
        {
            var parseOptions = new CSharpParseOptions(option.LanguageVersion, preprocessorSymbols: option.Environments);

            return CSharpSyntaxTree.ParseText(code, parseOptions);
        }


        /// <summary>
        /// 编译代码
        /// </summary>
        /// <param name="path">程序集位置</param>
        /// <param name="assemblyName">程序集名称</param>
        /// <param name="syntaxTrees">代码语法树</param>
        /// <param name="references">依赖</param>
        /// <param name="options">编译配置</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static EmitResult BuildCompilation(
            string path,
            string assemblyName,
            SyntaxTree[] syntaxTrees,
            PortableExecutableReference[] references,
            CSharpCompilationOptions options)
        {
            var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, options);
            var result = compilation.Emit(Path.Combine(path, assemblyName));
            return result;
        }
    }
}
