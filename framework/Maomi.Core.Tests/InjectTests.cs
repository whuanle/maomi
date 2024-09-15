using Microsoft.Extensions.DependencyInjection;

namespace Maomi.Core.Tests;

public class TestModule : IModule
{
    public void ConfigureServices(ServiceContext context)
    {
    }
}

[InjectOnTransient]
public class IT
{
}

[InjectOnScoped]
public class ISc
{
}

[InjectOnSingleton]
public class ISi
{
}

[InjectOn]
public class IO
{
}
public class InjectTests
{
    [Fact]
    public void Inject_lifetime()
    {
        var services = new ServiceCollection();
        services.AddModule<TestModule>();
        var service = services.BuildServiceProvider();
        
    }
}
