using CZGL.AOP;
using Demo2.AopLog;

public class Program
{
    static void Main(string[] args)
    {
        Hello hello = AopInterceptor.CreateProxyOfClass<Hello>();
        hello.SayHello("any one");
        Console.Read();
    }
}