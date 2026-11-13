# Maomi.I18n.Wpf 使用指南

## 用途与环境

`Maomi.I18n.Wpf` 将 `Maomi.I18n` 本地化器连接到 WPF 资源字典，支持切换 `CultureInfo` 并替换应用的合并字典。支持 `net8.0-windows`、`net9.0-windows`、`net10.0-windows`，项目需启用 WPF。此包依赖 `Maomi.I18n`。

```bash
dotnet add package Maomi.I18n.Wpf --version 2.3.0
```

## 初始化与资源

先注册基础 `Maomi.I18n`，再调用 `AddI18nWpf(appName, localization)`。`appName` 必须对应 WPF 资源所在程序集名；默认资源路径为 `localization`。每种语言放一个编译进 WPF 程序集的字典，例如 `localization/en-US.xaml`：

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:sys="clr-namespace:System;assembly=mscorlib">
    <sys:String x:Key="MainWindow_Title">Catalog</sys:String>
</ResourceDictionary>
```

```csharp
using Maomi.I18n;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddI18n(defaultLanguage: "zh-CN");
services.AddI18nWpf(appName: "CatalogApp", localization: "localization");
using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
scope.ServiceProvider.GetRequiredService<WpfI18nContext>().SetLanguage("en-US");
```

WPF 将 `Application.ResourceAssembly` 中以该路径开头的资源字典载入，并以文件名（例如 `en-US.xaml` 的 `en-US`）作为文化名。请在 WPF `Application` 已初始化后调用 `AddI18nWpf`，并在切换时对同一 DI scope 使用 `WpfI18nContext`。

`SetLanguage` 更新本地化上下文和当前线程文化，并将对应 XAML 字典加入 `Application.Current.Resources.MergedDictionaries`。别名 `en` 会映射到 `en-US`；中文别名映射到 `zh-CN` 或 `zh-HK`，未列出的名称回退到 `zh-CN`，因此相应的 XAML 文件必须存在。

## 绑定与本地化器

WPF 资源字典可由 XAML 动态资源引用；本地化字符串也可在 ViewModel 中注入非泛型 `IStringLocalizer`（WPF 扩展注册的是非泛型字典资源）：

```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="{DynamicResource MainWindow_Title}">
</Window>
```

```csharp
using Microsoft.Extensions.Localization;

public sealed class MainWindowViewModel(IStringLocalizer localizer)
{
    public string Title => localizer["MainWindow_Title"];
}
```

请在 WPF UI dispatcher 上调用 `SetLanguage`，因为它会替换 `Application.Current.Resources`。字典替换后 `DynamicResource` 会重新查找；已缓存在 ViewModel 属性或控件中的本地化字符串不会自动通知更新，需重新读取或由应用自行触发属性变更。

## 常见问题

- `AddI18nWpf` 不会调用 `AddI18n`；漏掉基础注册会令资源工厂和本地化器缺少 DI 服务。
- `AddI18nWpf` 会在注册时扫描 `Application.ResourceAssembly`，需在 WPF 资源程序集可用、服务集合完成初始化时调用。
- `appName`、`localization`、字典实际路径和文件名文化必须一致；资源字典必须被 WPF 编译为程序集资源。
- 使用 `StaticResource` 或预先缓存的文字不保证在切换后刷新；需使用 `DynamicResource` 或自行通知 UI 更新。
