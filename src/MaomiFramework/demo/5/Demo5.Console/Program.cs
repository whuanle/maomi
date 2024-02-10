using Demo5.Lib;
using Maomi.I18n;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;

public class Program
{
	static void Main()
	{
		var ioc = new ServiceCollection();
		ioc.AddI18n("zh-CN");
		ioc.AddI18nResource(options =>
		{
			options.AddJson<Program>("i18n");
		});

		ioc.AddLib();

		var services = ioc.BuildServiceProvider();

		// 手动设置当前请求语言
		using (var c = new CultureInfoScope("en-US"))
		{
			var l1 = services.GetRequiredService<IStringLocalizer<Program>>();
			var l2 = services.GetRequiredService<IStringLocalizer<Test>>();
			var s1 = l1["test"];
			var s2 = l2["test"];
			Console.WriteLine(s1);
			Console.WriteLine(s2);
		}
	}
}