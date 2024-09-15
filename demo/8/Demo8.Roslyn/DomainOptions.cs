using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

public class DomainOptions
{
    /// <summary>
    /// 当前要编译的程序集是何种类型的项目
    /// </summary>
    public OutputKind OutputKind { get; set; } = OutputKind.DynamicallyLinkedLibrary;

    /// <summary>
    /// Debug 还是 Release
    /// </summary>
    public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Release;

    /// <summary>
    /// 是否允许使用不安全代码
    /// </summary>
    public bool AllowUnsafe { get; set; } = false;

    /// <summary>
    /// 生成目标平台
    /// </summary>
    public Platform Platform { get; set; } = Platform.AnyCpu;

    /// <summary>
    /// 是否检查语法
    /// </summary>
    public bool CheckOverflow { get; set; } = false;

    /// <summary>
    /// 语言版本
    /// </summary>
    public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp7_3;

    /// <summary>
    /// 环境
    /// </summary>
    public HashSet<string> Environments { get; } = new HashSet<string>();
}