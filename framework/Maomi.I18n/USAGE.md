# Maomi.I18n 使用指南

`Maomi.I18n` 提供 JSON / 自定义资源、`IStringLocalizer` 适配和默认语言回退，可用于控制台、Web 和 WPF 应用。

- NuGet：`dotnet add package Maomi.I18n --version 2.4.0`
- 目标框架：`net8.0`、`net9.0`、`net10.0`
- 命名空间：`Maomi.I18n`
- 官方教程：[i18n 多语言支持](https://docs.whuanle.cn/zh/maomi_framework/start/2.i18n)
- ASP.NET Core 请求语言：见 [`Maomi.I18n.AspNetCore`](../Maomi.I18n.AspNetCore/USAGE.md)

## 最小可运行示例

在应用项目中创建 `i18n/zh-CN.json` 和 `i18n/en-US.json`。JSON 嵌套对象会展开为冒号分隔的 key：

```json
{
  "Cart": {
    "ProductName": "商品名称",
    "Items": "共 {0} 件"
  }
}
```

把文件复制到运行目录，然后注册 i18n、资源和消费服务：

```xml
<ItemGroup>
  <Content Include="i18n\*.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

```csharp
using Maomi.I18n;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

var services = new ServiceCollection();
services.AddI18n(defaultLanguage: "zh-CN");
services.AddI18nResource(factory =>
    factory.AddJsonDirectory<CatalogResources>(Path.Combine(AppContext.BaseDirectory, "i18n")));
services.AddTransient<CatalogService>();

using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
scope.ServiceProvider.GetRequiredService<CatalogService>().Show();

public sealed class CatalogResources { }

public sealed class CatalogService(IStringLocalizer<CatalogResources> localizer)
{
    public void Show() => Console.WriteLine(localizer["Cart:Items", 3]);
}
```

先调用 `AddI18n`，再调用 `AddI18nResource`。`AddJsonDirectory<T>` 会把资源限定到 `T` 所在程序集；使用 `IStringLocalizer<TCategory>` 时，资源 marker 类型和 `TCategory` 应位于同一程序集。若整个应用共用一套、不按程序集隔离的资源，可用 `AddJsonDirectory(path)` 并注入非泛型 `IStringLocalizer`。也可用 `AddJsonFile<T>(culture, path)` / `AddJsonFile(culture, path)` 单独加入文件。

## 语言选择、查找和回退

本地化器按 `I18nContext.Culture` 的文化名精确匹配；非 Web 应用可用 `using var culture = new I18nScope("en-US");` 临时设置当前文化。嵌套 JSON 通过 `localizer["Cart:ProductName"]` 读取，格式化字符串通过 `localizer["Cart:Items", count]` 读取。

没有父文化递归查找。未命中当前文化时，默认语言回退只查询注册到 DI 的 `I18nResource` 服务资源（`AddServiceType`）；用 `AddJsonDirectory` / `AddJsonFile` 加载的普通资源不会参与此默认语言回退。因此 JSON 语言文件应包含需要的 key，或将可回退的资源实现为 DI 服务。

## 运行时增加、替换和移除资源

`AddI18n` 注册的 `I18nResourceFactory` 是 Singleton，资源集合可在应用运行时修改。一个资源实例对应一种文化的一个资源源；同一语言可以有多个资源（例如不同程序集各自提供一份）。新增 JSON 资源时，工厂会同时记录资源和文化：

```csharp
using Microsoft.Extensions.DependencyInjection;

var factory = app.Services.GetRequiredService<I18nResourceFactory>();
var jsonPath = Path.Combine(AppContext.BaseDirectory, "i18n", "ja-JP.json");
factory.AddJsonFile<CatalogResources>("ja-JP", jsonPath);
```

替换某个资源时，先从 `Resources` 中移除目标资源，再调用 `AddJsonFile<T>` 加入新文件。移除资源的示例（假设当前应用只有这一份 `ko-KR` 资源）：

```csharp
var oldResource = factory.Resources.Single(
    resource => resource.SupportedCulture.Name == "ko-KR"
        && resource.GetType().IsGenericType
        && resource.GetType().GenericTypeArguments[0].Assembly == typeof(CatalogResources).Assembly);

factory.Resources.Remove(oldResource);

// 仅当所有程序集都不再提供 ko-KR 时，才从支持列表移除该文化。
if (!factory.Resources.Any(resource => resource.SupportedCulture.Name == "ko-KR"))
{
    var culture = factory.SupportedCultures.Single(c => c.Name == "ko-KR");
    factory.SupportedCultures.Remove(culture);
}
```

集合目前是普通可变集合，不提供并发更新协调，也不会因直接删除 `Resources` 项而自动删除 `SupportedCultures` 项。生产环境应串行化更新，并避免与正在读取资源的请求并发修改；同一文化还有其它资源时不要从支持列表中删除该文化。对于需要无停顿热更新的资源源，可实现自己的可变 `I18nResource`（内部用线程安全的字典/快照替换内容），启动时注册一次，之后只更新该资源对象的数据。

**本地化器可见性：** 非泛型本地化器查找时读取工厂当前的 `Resources` 集合。泛型 `IStringLocalizer<T>` 会在首次查找时缓存匹配当前程序集的静态资源；若资源集合已变化，应在新的 DI scope 中重新解析本地化器。不要复用已经缓存资源列表的泛型本地化器来验证更新。

## 自定义 `I18nResource`

实现 `I18nResource` 可将数据库、远程服务或内存字典接入统一的 `IStringLocalizer`。`SupportedCulture` 必须与资源实际提供的语言一致；`Get` 未命中时返回 `ResourceNotFound = true`：

```csharp
using Maomi.I18n;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

public sealed class MutableJapaneseResource : I18nResource
{
    private IReadOnlyDictionary<string, string> _values =
        new Dictionary<string, string>();

    public CultureInfo SupportedCulture { get; } = new("ja-JP");

    public void Replace(IReadOnlyDictionary<string, string> values) =>
        Volatile.Write(ref _values, values.ToDictionary(pair => pair.Key, pair => pair.Value));

    public LocalizedString Get(string culture, string name)
    {
        var values = Volatile.Read(ref _values);
        return culture == SupportedCulture.Name && values.TryGetValue(name, out var value)
            ? new LocalizedString(name, value)
            : new LocalizedString(name, name, resourceNotFound: true);
    }

    public LocalizedString Get(string culture, string name, params object[] arguments)
    {
        var result = Get(culture, name);
        return result.ResourceNotFound
            ? result
            : new LocalizedString(name, string.Format(result.Value, arguments));
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        Volatile.Read(ref _values).Select(pair => new LocalizedString(pair.Key, pair.Value));
}
```

注册到工厂，并将实例保留为 Singleton 供应用更新：

```csharp
using Microsoft.Extensions.DependencyInjection;

var japanese = new MutableJapaneseResource();
services.AddSingleton(japanese);
services.AddI18nResource(factory => factory.Add(japanese));
```

在纯 `Maomi.I18n` 中，`factory.Add(resource)` 会把 `resource.SupportedCulture` 一并加入支持文化集合。若使用 `AddServiceType(typeof(MyResource))` 让 i18n 从 DI 取得资源实例，当前方法只登记类型、不创建资源，也不会读取其 `SupportedCulture`；Web 应用还需在请求本地化 options 初始化前显式把语言加入 `factory.SupportedCultures`。Web 中间件的启动后热更新边界见 [`Maomi.I18n.AspNetCore`](../Maomi.I18n.AspNetCore/USAGE.md#运行时增加语言资源和请求白名单是两件事)。

## 常见问题

- JSON 文件名（不含扩展名）必须是有效文化名，例如 `en-US.json`；嵌套 key 用冒号而不是点号。
- JSON 文件在加入资源时读取，不会自动监控磁盘变化；文件更新后需重新加入或使用可变自定义资源。
- `I18nScope` 仅设置当前线程文化，不是 Web 请求文化选择器，也不负责切换 WPF `ResourceDictionary`。
- Web 请求文化选择、支持语言列表和运行期间新加语言的限制见 [`Maomi.I18n.AspNetCore 使用指南`](../Maomi.I18n.AspNetCore/USAGE.md)。
