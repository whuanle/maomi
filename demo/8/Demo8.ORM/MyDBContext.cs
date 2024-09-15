using System.Data;

public class MyDBContext
{
	protected readonly IDbConnection _connction;
	public MyDBContext(IDbConnection connction)
	{
		_connction = connction;
	}

	public MyDBContext<T> Select<T>() where T : class, new()
	{
		return new MyDBContext<T>(_connction);
	}
}
