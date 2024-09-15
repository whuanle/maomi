using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;

public partial class Program
{
    static void Main()
    {
        const string code =
"""
using System;
namespace MySpace
{
	public class Test
	{
		public int Sum(int a, int b)
		{
			return a + b;
		}
	}
}
""";

        // 编译选项
        // 编译选项可以不配置
        DomainOptionBuilder option = new DomainOptionBuilder()
            .WithPlatform(Platform.AnyCpu)                     // 生成可移植程序集
            .WithDebug(false)                                  // 使用 Release 编译
            .WithKind(OutputKind.DynamicallyLinkedLibrary)     // 生成动态库
            .WithLanguageVersion(LanguageVersion.CSharp7_3);   // 使用 C# 7.3

        // 编译代码
        var isSuccess = CompilationBuilder.CreateDomain(code,
           assemblyPath: "./",
           assemblyName: "test.dll",
           option: option,
           out var messages);

        // 编译失败，输出错误信息
        if (!isSuccess)
        {
            foreach (var item in messages)
            {
                Console.WriteLine(
        $"""
        ID:{item.Id}
        严重程度:{item.Severity}     
        位置：{item.Location.SourceSpan.Start}~{item.Location.SourceSpan.End}
        消息:{item.Descriptor.Title}   {item}
        """);
            }
            return;
        }

        // 编译成功，反射调用程序集代码
        var curPath = Directory.GetParent(typeof(Program).Assembly.Location).FullName;
        var assembly = Assembly.LoadFile($"{curPath}/test.dll");
        var type = assembly.GetType("MySpace.Test");
        var method = type.GetMethod("Sum");
        object obj = Activator.CreateInstance(type);
        int result = (int)method.Invoke(obj, new object[] { 1, 2 });
        Console.WriteLine(result);
    }
}
