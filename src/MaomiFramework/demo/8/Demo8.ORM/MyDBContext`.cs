using MySqlConnector;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

public class MyDBContext<T> : MyDBContext
	where T : class, new()
{
	public MyDBContext(IDbConnection connction) : base(connction)
	{
	}

	private readonly StringBuilder _strBuilder = new StringBuilder();

	public MyDBContext<T> Where(Expression<Func<T, bool>> predicate)
	{
		var bin = predicate.Body as BinaryExpression;
		ArgumentNullException.ThrowIfNull(bin);
		var content = $"{Parse(bin.Left)} {GetChar(bin.NodeType)} {Parse(bin.Right)}";
		_strBuilder.Append(content);
		return this;
	}

	// 解析条件
	private static string Parse(Expression ex)
	{
		if (ex is BinaryExpression bin)
		{
			ArgumentNullException.ThrowIfNull(bin);
			var left = bin.Left;
			var right = bin.Right;
			var content = $"{Parse(bin.Left)} {GetChar(bin.NodeType)} {Parse(bin.Right)}";
			return content;
		}
		else if (ex is MemberExpression p)
		{
			var name = $"`{p.Member.Name}`";
			return name;
		}
		else if (ex is ConstantExpression c)
		{
			var obj = c.Value;
			if (obj == null) return "null";
			var typeCode = TypeInfo.GetTypeCode(obj.GetType());
			if (typeCode == TypeCode.String) return $"'{obj.ToString()}'";
			return obj.ToString();
		}
		else if (ex is MethodCallExpression m)
		{
			if (m.Method.Name == "Contains")
			{
				return $"{Parse(m.Object)} like '%{Parse(m.Arguments.FirstOrDefault()).Trim('\'')}%'";
			}
		}
		throw new InvalidOperationException("不支持的表达式");
	}

	// 解析连接符
	private static string GetChar(ExpressionType type)
	{
		switch (type)
		{
			case ExpressionType.And: return "&";
			case ExpressionType.AndAlso: return "&&";
			case ExpressionType.Or: return "|";
			case ExpressionType.OrElse: return "||";
			case ExpressionType.Equal: return "=";
			case ExpressionType.NotEqual: return "!=";
			case ExpressionType.GreaterThan: return ">";
			case ExpressionType.GreaterThanOrEqual: return ">=";
			case ExpressionType.LessThan: return "<";
			case ExpressionType.LessThanOrEqual: return "<=";
		}
		throw new InvalidOperationException("不支持的表达式");
	}

	public async Task<List<T>> ToListAsync()
	{
		var sql = $"SELECT * FROM {typeof(T).Name} Where {_strBuilder.ToString()}";

		_connction.Open();
		var command = new MySqlCommand();
		command.Connection = _connction as MySqlConnection;
		command.CommandText = sql;

		var reader = await command.ExecuteReaderAsync();

		List<T> list = new List<T>();
		var ps = typeof(T).GetProperties();

		while (await reader.ReadAsync())
		{
			T t = new T();
			list.Add(t);
			for (int i = 0; i < ps.Length; i++)
			{
				var p = ps[i];
				object v = null;
				switch (TypeInfo.GetTypeCode(p.PropertyType))
				{
					case TypeCode.Int32: v = reader.GetInt32(i); break;
					case TypeCode.Int64: v = reader.GetInt64(i); break;
					case TypeCode.Double: v = reader.GetDouble(i); break;
					case TypeCode.String: v = reader.GetString(i); break;
					default: v = null; break;
				}
				p.SetValue(t, v);
			}
		}

		return list;
	}

	public async Task<T> FirstAsync()
	{
		return (await ToListAsync()).FirstOrDefault();
	}
}