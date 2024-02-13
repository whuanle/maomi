using Demo8.Mapper;



public class A
{
	public string C { get; set; }
	public int D { get; set; }
}
public class B
{
	public string C { get; set; }
	public string D { get; set; }
}

public class Program
{
	static void Main()
	{
		var builder = new MapperBuilder<A, B>();
		// 将左侧的值赋予右侧的字段或属性
		builder.Set(a => a.D.ToString(), b => b.D);
		builder.Build();

		A a = new A()
		{
			C = "C",
			D = 123
		};
		var b = builder.Map(a);
	}
}