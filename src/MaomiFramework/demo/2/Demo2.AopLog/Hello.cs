using CZGL.AOP;

namespace Demo2.AopLog
{
	[Interceptor]
	public class Hello
	{
		[Log]
		public virtual string SayHello(string content)
		{
			var str = $"Hello,{content}";
			return str;
		}
	}
}
