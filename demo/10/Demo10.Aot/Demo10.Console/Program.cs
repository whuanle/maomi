// <copyright file="Program.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Maomi;
using Microsoft.Extensions.DependencyInjection;

// 模块初始化顺序记录器（在 AddModule 之前注册，模块可注入）
var recorder = new OrderRecorder();

var services = new ServiceCollection();
services.AddSingleton(recorder);

// 编译期由源生成器生成注册计划，运行时零反射
services.AddModule<RootModule>();

using var provider = services.BuildServiceProvider();

// 1. 模块依赖顺序：依赖模块先于入口模块初始化
Console.WriteLine("模块初始化顺序: " + string.Join(" -> ", recorder.Order));

// 2. 自动注册的服务
var clock = provider.GetRequiredService<IClock>();
Console.WriteLine($"IClock          -> {clock.GetType().Name} (Scoped)");

var cache = provider.GetRequiredService<ICacheService>();
Console.WriteLine($"ICacheService   -> {cache.GetType().Name} (Singleton)");

// 3. 键控服务
var mysql = provider.GetRequiredKeyedService<IKeyedRepo>("mysql");
var redis = provider.GetRequiredKeyedService<IKeyedRepo>("redis");
Console.WriteLine($"IKeyedRepo[mysql] -> {mysql.Name}");
Console.WriteLine($"IKeyedRepo[redis] -> {redis.Name}");

// 4. 构造函数注入（含 [FromKeyedServices]）
var storage = provider.GetRequiredService<IStorage>();
Console.WriteLine($"IStorage        -> {storage.GetType().Name}，后端 = {storage.Backend}");

// 5. 模块注册的服务
var fromModule = provider.GetRequiredService<IRegisterFromModule>();
Console.WriteLine($"Module 注册的服务 -> {fromModule.GetType().Name}");

// 6. 模块自身也可从容器解析
var root = provider.GetRequiredService<RootModule>();
Console.WriteLine($"RootModule 已从容器解析 -> {root != null}");

public interface IClock
{
    DateTimeOffset Now { get; }
}

[InjectOnScoped]
public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}

public interface ICacheService
{
}

[InjectOnSingleton]
public sealed class MemoryCacheService : ICacheService
{
}

public interface IKeyedRepo
{
    string Name { get; }
}

[InjectOnSingleton(ServiceKey = "mysql")]
public sealed class MySqlRepo : IKeyedRepo
{
    public string Name => "mysql";
}

[InjectOnSingleton(ServiceKey = "redis")]
public sealed class RedisRepo : IKeyedRepo
{
    public string Name => "redis";
}

public interface IStorage
{
    string Backend { get; }
}

[InjectOnScoped]
public sealed class StorageService : IStorage
{
    public StorageService([FromKeyedServices("mysql")] IKeyedRepo repo)
    {
        Backend = repo.Name;
    }

    public string Backend { get; }
}

public interface IRegisterFromModule
{
}

public sealed class RegisterFromModuleService : IRegisterFromModule
{
}

public sealed class OrderRecorder
{
    public List<string> Order { get; } = new();
}

public sealed class InfrastructureModule : IModule
{
    private readonly OrderRecorder _recorder;

    public InfrastructureModule(OrderRecorder recorder)
    {
        _recorder = recorder;
    }

    public void ConfigureServices(ServiceContext context)
    {
        _recorder.Order.Add("InfrastructureModule");
        context.Services.AddSingleton<IRegisterFromModule, RegisterFromModuleService>();
    }
}

[InjectModule<InfrastructureModule>]
public sealed class ApplicationModule : IModule
{
    private readonly OrderRecorder _recorder;

    public ApplicationModule(OrderRecorder recorder)
    {
        _recorder = recorder;
    }

    public void ConfigureServices(ServiceContext context)
    {
        _recorder.Order.Add("ApplicationModule");
    }
}

[InjectModule<ApplicationModule>]
public sealed class RootModule : IModule
{
    private readonly OrderRecorder _recorder;

    public RootModule(OrderRecorder recorder)
    {
        _recorder = recorder;
    }

    public void ConfigureServices(ServiceContext context)
    {
        _recorder.Order.Add("RootModule");
    }
}
