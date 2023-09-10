using CZGL.AOP;

public class LogAttribute : ActionAttribute
{
    public override void Before(AspectContext context)
    {
        Console.WriteLine("执行前");
    }

    public override object After(AspectContext context)
    {
        Console.WriteLine("执行后");
        if (context.IsMethod)
            return context.MethodResult;
        else if (context.IsProperty)
            return context.PropertyValue;
        return null;
    }
}

public interface ITest
{
    void MyMethod();
}

[Interceptor]
public class Test : ITest
{
    [Log]
    public virtual string A { get; set; }
    public Test()
    {
        Console.WriteLine("构造函数没问题");
    }
    [Log]
    public virtual void MyMethod()
    {
        Console.WriteLine("运行中");
    }
}


public class Program
{
    static void Main()
    {
        ITest test1 = AopInterceptor.CreateProxyOfInterface<ITest, Test>();
        Test test2 = AopInterceptor.CreateProxyOfClass<Test>();
        test1.MyMethod();
        test2.MyMethod();
    }
}