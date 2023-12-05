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

        // 因为没有中间件请求，所以手动设置
        CultureInfo.CurrentCulture = new CultureInfo("zh-CN");
        // todo:这里有问题需要重新处理
		var context = services.GetRequiredService<I18nContext>();

        var l1 = services.GetRequiredService<IStringLocalizer<Program>>();
        var l2 = services.GetRequiredService<IStringLocalizer<Test>>();

        var s1 = l1["test"];
        var s2 = l2["test"];
    }
}