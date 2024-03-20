# 猫咪框架

Maomi 框架是一个简单的、简洁的开发框架，除了框架本身提供的功能之外，Maomi 还作为一个易于阅读的开源项目，能够给开发者提供设计框架的思路和代码。



Maomi 框架目前具有模块化和自动服务注册、多语言、事件总线、Web 四个模块。而整个解决方案中一共有 62 个项目，包括了日常部分框架的编写示例，例如怎么制作类似 dotnet-dump 的诊断工具、怎么定制日志框架以及怎么写一个日志框架、怎么使用 EMIT 写一个 AOP、怎么使用 Roslyn 写一个代码编译器、怎么设计类似 ABP 的模块化等，还包括了单元测试。

如果你想从零编写一个自己的开发框架，那么 62 个项目，每个部分都是独立的，可以帮助你学习、了解怎么编写各类框架。

关于如何使用 Maomi 框架，以及如何定制、编写常用框架，请参考文档地址：https://maomi.whuanle.cn

* [1.模块化和自动服务注册](https://maomi.whuanle.cn/1.module.html) 

  > 讲解 Maomi.Core 的使用方法和基本原理

* [2.模块化和自动服务注册的设计和实现](https://maomi.whuanle.cn/2.design_module.html) 

  > 讲解 Maomi.Core 是如何设计和实现，我们想开发一个框架时，怎么从设计、抽象、编码到最后实现。讲解了模块化和自动服务注册的原理，如何从零开发，最后制作 nuget 包，分发到 nuget.org 给所有人使用。

* [3.故障诊断和日志](https://maomi.whuanle.cn/3.0.gz_log.html)

  > 介绍故障诊断的一些方法，以及 .NET 中的日志抽象接口。

* [3.1.自定义开发日志框架](https://maomi.whuanle.cn/3.1.design_log.html)

  > 如何自己设计、开发一个日志框架。

* [3.2. .NET 日志使用技巧](https://maomi.whuanle.cn/3.2.serilog.html)

  > 非常推荐阅读，介绍了 Serilog 的配置、使用方法，介绍了生命周期作用域、属性、日志模板等相关说明，以便在程序运行时，输出非常高效的日志，为排查问题带来方便。很多开发者使用日志都很敷衍，不知道怎么利用好日志工具，那么这篇文章可以帮到你。

* [3.3.开发 .NET 诊断工具](https://maomi.whuanle.cn/3.3.diagostics.html)

  > 介绍一些 .NET 诊断的方法和原理，然后介绍如何开发 dotnet-trace、dotnet-counters、dotnet-dump 等这样的工具，没错，我们也可以写出这样的工具！

* [4.配置和选项](https://maomi.whuanle.cn/4.pz.html)

  > 简述了 IConfiguration 、Options 的原理和使用方法，自定义配置提供器、使用 signalR 实现一个配置中心。

* [5.NET 中的序列化和反序列化](https://maomi.whuanle.cn/5.xlh.html)

  > 本章的内容比较丰富，讲解了 .NET 下序列化和反序列化的一些特征、自定义配置、使用技巧，如何自定义枚举转换器、字符串转换器、时间格式转换器等，详细讲解了实现细节。最后介绍了 Utf8JsonReader 和怎么编写性能测试代码，通过 Utf8JsonReader 解析 json 的示例，让读者掌握原理，在后续章节中，还会介绍如何使用 Utf8JsonReader 实现多语言等基础能力。

* [6.多语言](https://maomi.whuanle.cn/6.i18n.html)

  > 本章内容比较丰富，首先介绍 Maomi.I18n 框架的使用方法，ASP.NET Core 是怎么识别多语言请求和使用多语言的，了解 i18n 框架需要做什么，然后开始设计抽象、编写实现代码。编写框架完毕后，还需要编写单元测试，笔者介绍了如何编写单元测试。接着介绍了如何基于 Redis 实现多语言，最后介绍如何在 nuget 包中打包多语言文件与他人共享。

* [7.http 应用开发](https://maomi.whuanle.cn/7.http.html)

  > 本章内容详细介绍了 HttpClient 的使用方法，除了基础知识外，还包括比如请求参数、请求凭证、异常处理，接着详细介绍了 IHttpClientFactory ，包括请求拦截、请求策略（重试、超时）等技术。介绍了 Refit 工具的使用方法，如何在业务开发中使用 Refit 快速生成 http 请求代码，简化开发过程。最后介绍如何自己编写一个类似 curl 的工具，掌握使用 .NET 编写命令行工具的技术和技巧。

* [8.事件总线框架的设计](https://maomi.whuanle.cn/8.event.html)

  > 事件总线是 DDD 开发中最常用的解耦通讯手段，所以本章会带着读者从零设计一个事件总线框架，从抽象设计到编写，讲解了每个环节的原理和实现代码。事件总线中会使用到反射、委托、表达式树等技术，如果你对表达式树不了解，没关系，先照着做、按照教程学，不需要死扣技术细节，只需要掌握大体设计和开发思路即可。

* [9.动态代码](https://maomi.whuanle.cn/9.dt.html)

  > 本章内容比较丰富，讲解了 EMIT 技术和如何开发 AOP 框架，表达式树的两种使用方法、编写对象映射框架、简单的 ORM 框架，介绍 Roslyn 技术、代码生成和编译、Natasha 框架的简单使用，最后介绍了 Source Generators (简称 sg 技术)实现代码生成。
  >
  > 限于篇幅，本章不会过隙讨论各种技术，如果读者需要打好基础，可以参考笔者其它电子书：
  >
  > 反射基础： https://reflect.whuanle.cn/
  >
  > 表达式树基础：https://ex.whuanle.cn

* [10.Web 框架定制开发](https://maomi.whuanle.cn/10.web.html)

  > 本章内容比较丰富，日常开发中大家都会定制 Web 框架，以使用企业内部需求，那么本章介绍了开发中比较常见的东西，以及如何定制它，比如模型验证是怎么实现的、如何自定义模型验证器、模型验证器中使用 i18n，各种筛选器的使用方法和技巧、定制开发筛选器(Action 筛选器、资源筛选器、异常筛选器)，Swagger 定制（模型类属性类型转换、接口分组、接口版本号、微服务路由后缀）等。

* [11.对象映射框架](https://maomi.whuanle.cn/11.mapper.html)

  > 详细介绍了 Maomi.Mapper 的使用方法。