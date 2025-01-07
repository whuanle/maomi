using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net.Http;
using System.Xml;

public class Program
{
    static async Task Main(string[] args)
    {
        var filters = new string[] { "bin", "obj", "Properties" };

        var i18nDic = new Dictionary<string, string>();

        var slnPath = "";
        var jsonDir = "";

        var dllPath = typeof(Program).Assembly.Location;

        // 如果运行路径在 bin\Debug\net8.0
        if (Directory.GetParent(dllPath)!.FullName.Contains("bin\\Debug"))
        {
            slnPath = Directory.GetParent(dllPath)!.Parent!.Parent!.Parent!.Parent!.Parent!.FullName;
            jsonDir = Directory.GetParent(dllPath)!.Parent!.Parent!.Parent!.FullName;
        }
        else
        {
            slnPath = Directory.GetParent(dllPath)!.Parent!.Parent!.FullName;
            jsonDir = Directory.GetParent(dllPath)!.FullName;
        }

        // 所有项目所在目录都在 src 下面
        var projPath = slnPath;

        // 所有项目目录
        var projects = Directory.GetDirectories(projPath);

        // 使用队列逐个目录搜索，不要一次性都加载进去
        foreach (string project in projects)
        {
            // 子目录列表
            Queue<string> itemDirs = new();

            itemDirs.Enqueue(project);

            while (itemDirs.Count > 0)
            {
                var curDir = itemDirs.Dequeue();
                var csFiles = Directory.GetFiles(curDir, "*.cs", SearchOption.TopDirectoryOnly);
                foreach (var csFile in csFiles)
                {
                    Console.WriteLine(csFile);
                    string fileContent = await File.ReadAllTextAsync(csFile);

                    // 读取文件解析语法树
                    SyntaxTree tree = CSharpSyntaxTree.ParseText(fileContent);
                    var root = tree.GetRoot();

                    // 查找所有 new BusinessException 语句，new BsiRpcException 语句
                    var objectCreations = root.DescendantNodes()
                                             .OfType<ObjectCreationExpressionSyntax>()
                                             .Where(node => node.Type.ToString() == "BusinessException");

                    foreach (var objectCreation in objectCreations)
                    {
                        if (objectCreation.ArgumentList == null)
                        {
                            continue;
                        }

                        // 提取 objectCreation 的参数列表中的字符串
                        var arguments = objectCreation.ArgumentList.Arguments;

                        foreach (var argument in arguments)
                        {
                            if (argument.Expression is LiteralExpressionSyntax literal &&
                                literal.IsKind(SyntaxKind.StringLiteralExpression))
                            {
                                string str = literal.Token.ValueText;
                                if (!i18nDic.ContainsKey(str))
                                {
                                    i18nDic[str] = str;
                                }
                            }
                        }
                    }


                    // 查找所有 WithMessage 方法
                    var invocationExpressions = root.DescendantNodes()
                                                    .OfType<InvocationExpressionSyntax>()
                                                    .Where(node => node.Expression is MemberAccessExpressionSyntax memberAccess &&
                                                                   memberAccess.Name.ToString() == "WithMessage");

                    foreach (var invocation in invocationExpressions)
                    {
                        var arguments = invocation.ArgumentList.Arguments;

                        foreach (var argument in arguments)
                        {
                            if (argument.Expression is LiteralExpressionSyntax literal &&
                                literal.IsKind(SyntaxKind.StringLiteralExpression))
                            {
                                string str = literal.Token.ValueText;
                                if (!i18nDic.ContainsKey(str))
                                {
                                    i18nDic[str] = str;
                                }
                            }
                        }
                    }
                }

                var newDirs = Directory.GetDirectories(curDir).Where(x => !filters.Contains(x)).ToArray();
                foreach (var itemDir in newDirs)
                {
                    itemDirs.Enqueue(itemDir);
                }
            }
        }

        // Serialize the dictionary to a JSON file
        string jsonOutput = System.Text.Json.JsonSerializer.Serialize(i18nDic, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        File.WriteAllText(Path.Combine(jsonDir, "zh-CN.json"), jsonOutput);

        Console.WriteLine("已经自动生成i18n文件，祝你生活愉快");
    }
}