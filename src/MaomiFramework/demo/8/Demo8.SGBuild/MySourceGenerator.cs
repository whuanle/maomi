using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Demo8.SGBuild
{
    /// <summary>
    /// 
    /// </summary>
    [Generator]
    public class MySourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // 查找 Main 方法
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);
            // 生成新的代码
            string source = $@"
using System;

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
    public class Test : ITest
    {{
        public int Sum(int a, int b)
        {{
            return a + b;
        }}
    }}
}}
";

            // 生成新的代码到文件
            context.AddSource($"MyAOP.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
