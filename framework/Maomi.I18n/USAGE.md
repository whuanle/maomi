# Maomi.I18n 使用指南

## 用途与环境

`Maomi.I18n` 提供基于文化名称的 JSON 字符串资源、`IStringLocalizer` 适配和默认语言回退。支持 `net8.0`、`net9.0`、`net10.0`。

```bash
dotnet add package Maomi.I18n --version 2.3.0
```

依赖 Microsoft DI、Configuration Abstractions 和 `Microsoft.Extensions.Localization.Abstractions`。在容器中先调用 `AddI18n`，再注册资源；资源目录需由应用复制到运行输出目录。

## 注册 JSON 资源并使用

JSON 文件名（不含扩展名）就是文化名；嵌套对象用冒号连接成查找键：

```json
{
  "Cart": {
    "ProductName": "商品名称",
    "Items": "共 {0} 件"
  }
}
```

将此内容分别保存为 `i18n/zh-CN.json`、`i18n/en-US.json` 等。示例应用需把文件复制到输出目录：

```xml
<ItemGroup>
  <Content Include="i18n\*.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

```csharp
using Maomi.I18n;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.IO;

var services = new ServiceCollection();
services.AddI18n(defaultLanguage: "zh-CN");
services.AddI18nResource(factory =>
    factory.AddJsonDirectory<AppResources>(Path.Combine(AppContext.BaseDirectory, "i18n")));
services.AddTransient<CatalogService>();
using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
scope.ServiceProvider.GetRequiredService<CatalogService>().Show();

public sealed class AppResources { }

public sealed class CatalogService(IStringLocalizer<CatalogService> localizer)
{
    public void Show() => Console.WriteLine(localizer["Cart:ProductName"]);
}
```

`AddJsonDirectory<T>` 将资源限定到 `T` 所在程序集；因此 `T` 与 `IStringLocalizer<TLocalizer>` 的类型应位于同一程序集。也可用 `AddJsonFile(language, path)` 注册单个文件，或使用 `AddJsonDirectory(path)` 注册不按程序集区分的资源并通过非泛型 `IStringLocalizer` 访问。

## 查找与文化

注入 `IStringLocalizer<T>` 后可用 `localizer["Cart:ProductName"]` 或格式化形式 `localizer["Cart:Items", count]`。当前文化来自 `I18nContext.Culture`；未显式设置时取 `CultureInfo.CurrentCulture`。资源按文化名精确匹配，不自动尝试父文化。查找当前文化未命中时，代码会尝试从 DI 服务资源中查找 `AddI18n(defaultLanguage)` 指定的语言；`AddJsonDirectory` 注册的是静态资源，不参与这一步回退，因此要为每种文化准备所需键。仍未找到时返回键名并标记为未找到。

需要服务资源参与默认语言回退时，可将实现了 `I18nResource` 的资源类型注册到 DI，并在 `AddI18nResource(factory => factory.AddServiceType(typeof(MyResource)))` 中登记该类型。

在非 Web 应用中可用 `using var culture = new I18nScope("en-US");` 临时设置 `CurrentCulture`，作用域结束后自动还原。Web 请求文化由 `Maomi.I18n.AspNetCore` 负责。

## 常见问题

- 必须先 `AddI18n()` 再调用 `AddI18nResource()`；后者从服务集合构建 provider 并取得资源工厂。
- 文件名必须是有效文化名（例如 `en-US.json`）；资源键使用 `:` 分隔嵌套 JSON 字段，不是点号。
- JSON 资源在注册时从磁盘读取；确认路径指向运行时目录且文件已复制。
- 泛型本地化器按资源类型所在程序集筛选资源；把 marker 类型放在错误程序集会导致一直找不到条目。
- `I18nScope` 只改变 `CurrentCulture`，不是请求文化选择器，也不负责切换 WPF 资源字典。
