using MySqlConnector;
using System.Data;

public class Test
{
	public int Id { get; set; }

	public string Name { get; set; }
}

public class Program
{
	static async Task Main()
	{
		var builder = new MySqlConnectionStringBuilder
		{
			Server = "localhost:3306",
			Database = "test",
			UserID = "root",
			Password = "123456",
			SslMode = MySqlSslMode.Required,
		};
		IDbConnection connction = new MySqlConnection(builder.ConnectionString);

		var context = new MyDBContext(connction);
		var list = await context.Select<Test>()
		.Where(x => x.Id >= 2 && x.Name.Contains("工良") && x.Id < 4)
		.ToListAsync();
	}
}
