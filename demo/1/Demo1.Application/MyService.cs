using Maomi;

namespace Demo1.Application
{
    //[InjectOn(ServiceLifetime.Scoped, Own = true)]
    [InjectOnScoped(Own = true)]
    public class MyService : IMyService
    {
        public int Sum(int a, int b)
        {
            return a + b;
        }
    }
}
