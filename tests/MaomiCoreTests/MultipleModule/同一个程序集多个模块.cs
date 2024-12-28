using Maomi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MultipleModule;

public class 同一个程序集多个模块
{
    /*
     同一个程序集有多个模块类，每个模块类都会实例化，
     但该程序集只扫描一次
     */

    [Fact]
    public void 程序集只扫描一次()
    {
        var services = new ServiceCollection();
        var congiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        services.AddScoped<IConfiguration>(s => congiguration);

        var moduleBuilder = new ModuleBuilder_TotalCount(services, new ModuleBuilderParamters(new ModuleOptions { }, typeof(ModuleA)));
        moduleBuilder.Build();

        var ioc = moduleBuilder.ServiceProvider;

        Assert.Single(moduleBuilder.AssemblyScanTotalCount);
        Assert.Equal(1, moduleBuilder.AssemblyScanTotalCount[typeof(ModuleA).Assembly]);
        Assert.Equal(2, moduleBuilder.InitializedModules.Count);
        Assert.Single(moduleBuilder.InitializedAssemblys);
        Assert.Equal(2, moduleBuilder.ModuleInstances.Count);
        Assert.Empty(moduleBuilder.ModuleCoreInstances);

        var moduleA = ioc.GetRequiredService<ModuleA>();
        var moduleB = ioc.GetRequiredService<ModuleB>();

        Assert.Equal(1, moduleA.InitCount);
        Assert.Equal(1, moduleB.InitCount);
    }

    [Fact]
    public void Filter_ModuleCore()
    {
        var services = new ServiceCollection();
        var congiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        services.AddScoped<IConfiguration>(s => congiguration);

        var moduleBuilder = new ModuleBuilder_TotalCount(services, new ModuleBuilderParamters(new ModuleOptions { }, typeof(ModuleC)));
        moduleBuilder.Build();

        var ioc = moduleBuilder.ServiceProvider;

        Assert.Single(moduleBuilder.AssemblyScanTotalCount);
        Assert.Equal(1, moduleBuilder.AssemblyScanTotalCount[typeof(ModuleA).Assembly]);
        Assert.Equal(3, moduleBuilder.InitializedModules.Count);
        Assert.Single(moduleBuilder.InitializedAssemblys);
        Assert.Equal(3, moduleBuilder.ModuleInstances.Count);
        Assert.Single(moduleBuilder.ModuleCoreInstances);

        var moduleA = ioc.GetRequiredService<ModuleA>();
        var moduleB = ioc.GetRequiredService<ModuleB>();
        var moduleC = ioc.GetRequiredService<ModuleC>();

        Assert.Equal(1, moduleA.InitCount);
        Assert.Equal(1, moduleB.InitCount);
        Assert.Equal(1, moduleC.InitCount);
        Assert.True(moduleC.FilterCount > 2);
    }

    [Fact]
    public void Import_Dll()
    {
        var services = new ServiceCollection();
        var congiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        services.AddScoped<IConfiguration>(s => congiguration);

        var moduleOptions = new ModuleOptions();
        moduleOptions.CustomAssembies.Add(typeof(ModuleAssembly.ModuleDll).Assembly);

        var moduleBuilder = new ModuleBuilder_TotalCount(services, new ModuleBuilderParamters(moduleOptions, typeof(ModuleC)));
        moduleBuilder.Build();

        var ioc = moduleBuilder.ServiceProvider;

        Assert.Equal(2, moduleBuilder.AssemblyScanTotalCount.Count);
        foreach (var item in moduleBuilder.AssemblyScanTotalCount)
        {
            Assert.Equal(1, item.Value);
        }

        Assert.Equal(4, moduleBuilder.InitializedModules.Count);
        Assert.Equal(2, moduleBuilder.InitializedAssemblys.Count);
        Assert.Equal(4, moduleBuilder.ModuleInstances.Count);
        Assert.Single(moduleBuilder.ModuleCoreInstances);

        var moduleA = ioc.GetRequiredService<ModuleA>();
        var moduleB = ioc.GetRequiredService<ModuleB>();
        var moduleC = ioc.GetRequiredService<ModuleC>();
        var moduleDll = ioc.GetRequiredService<ModuleAssembly.ModuleDll>();

        Assert.Equal(1, moduleA.InitCount);
        Assert.Equal(1, moduleB.InitCount);
        Assert.Equal(1, moduleC.InitCount);
        Assert.True(moduleC.FilterCount > 2);
    }
}

public class ModuleBuilder_TotalCount : ModuleBuilder
{
    public Dictionary<Assembly, int> AssemblyScanTotalCount { get; private set; } = new();

    public IServiceProvider ServiceProvider => _serviceProvider;

    public HashSet<Type> InitializedModules => _initializedModules;

    public HashSet<Assembly> InitializedAssemblys => _initializedAssemblys;

    public HashSet<IModule> ModuleInstances => _moduleInstances;

    public HashSet<ModuleCore> ModuleCoreInstances => _moduleCoreInstances;

    public ModuleBuilder_TotalCount(IServiceCollection services, ModuleBuilderParamters modulerParamters) : base(services, modulerParamters)
    {
    }

    protected override void ScanServiceFromAssembly(Assembly assembly)
    {
        if (AssemblyScanTotalCount.TryGetValue(assembly, out var count))
        {
            AssemblyScanTotalCount[assembly] = count + 1;
        }
        else
        {
            AssemblyScanTotalCount[assembly] = 1;
        }

        base.ScanServiceFromAssembly(assembly);
    }
}

[InjectModule<ModuleB>]
public class ModuleA : IModule
{
    public int InitCount { get; private set; }
    public void ConfigureServices(ServiceContext context)
    {
        InitCount++;
    }
}

public class ModuleB : IModule
{
    public int InitCount { get; private set; }
    public void ConfigureServices(ServiceContext context)
    {
        InitCount++;
    }
}

[InjectModule<ModuleA>]
public class ModuleC : ModuleCore
{
    public int InitCount { get; private set; }

    public int FilterCount { get; private set; }

    public override void ConfigureServices(ServiceContext context)
    {
        InitCount++;
    }

    public override void TypeFilter(Type type)
    {
        FilterCount++;
    }
}