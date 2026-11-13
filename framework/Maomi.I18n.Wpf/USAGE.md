# Maomi.I18n.Wpf 使用指南

本包把 [`Maomi.I18n`](../Maomi.I18n/USAGE.md) 的本地化器和 WPF `ResourceDictionary` 连接起来。调用 `SetLanguage` 可替换合并字典，适合通过 `{DynamicResource ...}` 更新界面。

- NuGet：`dotnet add package Maomi.I18n.Wpf --version 2.4.0`
- 目标框架：`net8.0-windows`、`net9.0-windows`、`net10.0-windows`
- 前提：WPF 项目（`<UseWPF>true</UseWPF>`）
- 官方教程：[i18n 多语言支持](https://docs.whuanle.cn/zh/maomi_framework/start/2.i18n)

## 添加语言字典

默认资源目录为 `localization`。每种语言放一个 WPF `ResourceDictionary`，设置为 `Page`/WPF 编译资源，例如 `localization/en-US.xaml`：

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:sys="clr-namespace:System;assembly=mscorlib">
    <sys:String x:Key="MainWindow_Title">Catalog</sys:String>
</ResourceDictionary>
```

在 WPF 应用启动且 `Application.ResourceAssembly` 可用后注册服务。`appName` 是包含这些 WPF 资源的程序集名，不是随意的显示名：

```csharp
var services = new ServiceCollection();
services.AddI18n(defaultLanguage: "zh-CN");
services.AddI18nWpf(appName: "CatalogApp", localization: "localization");
using var provider = services.BuildServiceProvider();
```

每个资源字典同时被导入 `Maomi.I18n`，可用非泛型 `IStringLocalizer` 按 key 查询；目录与程序集名必须匹配 pack URI。若 ViewModel 需要本地化字符串，需确保它从 DI scope 中创建。

## 切换语言

`WpfI18nContext.SetLanguage` 更新当前文化并替换 `Application.Current.Resources.MergedDictionaries` 中的 Maomi 语言字典。在 WPF UI dispatcher 上调用：

```csharp
using Maomi.I18n;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

// App 已初始化；应用本身通常管理一个长期存活的服务 provider。
using var scope = provider.CreateScope();
var i18n = scope.ServiceProvider.GetRequiredService<WpfI18nContext>();
Application.Current.Dispatcher.Invoke(() => i18n.SetLanguage("en-US"));
```

当前内置语言别名映射包括 `en`/`en-US` → `en-US`，日文 `ja`/`ja-JP` → `jp`，韩文 `ko`/`ko-KR` → `ko`，俄文 `ru`/`ru-RU` → `ru`，简体中文 → `zh-CN`，繁体中文 → `zh-HK`。对应的字典文件名必须使用映射后的名称，例如日文是 `localization/jp.xaml`。未列出的语言会回退到 `zh-CN`，因此需保证 `zh-CN.xaml` 存在。

```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="{DynamicResource MainWindow_Title}">
</Window>
```

`DynamicResource` 会在合并字典替换后重新查找；`StaticResource`、已缓存到 ViewModel 属性的字符串不会自动通知刷新。需要刷新缓存文本时，由应用自行触发属性变更。

## 增加新的 WPF 语言

WPF 字典由当前 `Application.ResourceAssembly` 的编译资源在 `AddI18nWpf` 注册时发现，随后通过 pack URI 加载。因此运行时只向 `I18nResourceFactory` 添加 JSON/`I18nResource`，不会自动创建或加载新的 WPF XAML 字典。新增字典需要编译/部署到程序集，并确保 `WpfI18nContext` 的语言映射、文件名和部署版本一致；当前实现没有运行时扫描新增 XAML 文件的 API。

若只需动态更新 `IStringLocalizer` 的字符串而不替换 WPF 字典，可在基础包中注册自定义可变 `I18nResource`；参见 [`Maomi.I18n` 的自定义资源与运行时更新](../Maomi.I18n/USAGE.md#运行时增加替换和移除资源)。

## 常见问题

- `AddI18nWpf` 不会代替 `AddI18n`；先注册基础包。
- 在 WPF `Application` 初始化、资源程序集可用后调用 `AddI18nWpf`。
- 文件必须是编译进程序集的 WPF 资源，`appName`、目录名、文化映射和文件名必须匹配。
- 仅在 UI dispatcher 上切换语言；`DynamicResource` 会重查字典，缓存的普通字符串不会自动更新。
- JSON 资源、程序集隔离、默认语言回退见 [`Maomi.I18n/USAGE.md`](../Maomi.I18n/USAGE.md)。
