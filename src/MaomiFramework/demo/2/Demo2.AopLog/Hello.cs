using CZGL.AOP;

namespace Demo2.AopLog
{
    [Interceptor]
    public class Hello
    {
        [Log]
        public virtual void SayHello(string content)
        {
            var str = $"Hello,{content}";
            Console.WriteLine(str);
        }
    }
}
