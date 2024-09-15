namespace Maomi.Core.Tests.Module1
{
    public interface IA { }
    public interface IB { }
    public interface IC { }


    public class ParentService { }
    [InjectOn]
    public class MyService : ParentService, IA, IB, IC { }
}
