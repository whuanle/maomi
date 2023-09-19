using CZGL.AOP;

namespace Demo2.AopLog;

public class LogAttribute : ActionAttribute
{
    public override void Before(AspectContext context)
    {
        Console.WriteLine($"{context.MethodInfo.Name} 函数被执行前");
        foreach (var item in context.MethodValues)
            Console.WriteLine(item.ToString());
    }

    public override object After(AspectContext context)
    {
        Console.WriteLine($"{context.MethodInfo.Name} 函数被执行后");
        Console.WriteLine(context.MethodResult.ToString());
        return context.MethodResult;
    }
}
