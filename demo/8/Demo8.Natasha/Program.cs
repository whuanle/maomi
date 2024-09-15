using System;
using System.Linq;
using System.Reflection;

public class Program
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

        //初始化 Natasha 编译组件及环境
        NatashaInitializer.Preheating();
        //创建编译单元,并指定程序集名
        AssemblyCSharpBuilder oop = new AssemblyCSharpBuilder("myAssembly");
        //编译单元使用从域管理分配出来的随机域
        oop.Domain = DomainManagement.Random();
        //增加代码到编译单元中
        oop.Add(code);
        // 生成程序集
        Assembly assembly = oop.GetAssembly();

        var type = assembly.GetTypes().FirstOrDefault(x => x.Name == "Test");
        var result = type.GetMethod("Sum", BindingFlags.Instance | BindingFlags.Public).Invoke(Activator.CreateInstance(type), new object[] { 1, 2 });
        Console.Write(result);
    }
}