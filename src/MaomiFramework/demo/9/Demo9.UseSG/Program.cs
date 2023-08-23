using System.Reflection;

namespace Demo9.UseSG
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = typeof(Program).Assembly;
            var testType = assembly.GetTypes().FirstOrDefault(x => x.GetInterfaces().Any(i => i == typeof(ITest)));
            var test = Activator.CreateInstance(testType) as ITest;
            var sum = test.Sum(1, 2);
            Console.WriteLine(sum);
        }
    }

    public interface ITest
    {
        int Sum(int a, int b);
    }
}