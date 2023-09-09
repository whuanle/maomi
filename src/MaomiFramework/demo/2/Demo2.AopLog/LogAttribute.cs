using CZGL.AOP;

namespace Demo2.AopLog;

    public class LogAttribute : ActionAttribute
    {
        public override void Before(AspectContext context)
        {
            Console.WriteLine($"{context.MethodInfo.Name} 函数被执行前");
        }

        public override object After(AspectContext context)
        {
            Console.WriteLine($"{context.MethodInfo.Name} 函数被执行后");
            return null;
        }
    }
