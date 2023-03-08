using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// 值类型和字符串互转
/// </summary>
public class JsonStringToNumberConverter : JsonConverterFactory
{
	/// <summary>
	/// 获取默认实例
	/// </summary>
	public static JsonStringToNumberConverter Default { get; } = new JsonStringToNumberConverter();

	/// <summary>
	/// 返回类型是否可以转换
	/// </summary>
	/// <param name="typeToConvert"></param>
	/// <returns></returns>
	public override bool CanConvert(Type typeToConvert)
	{
		var typeCode = Type.GetTypeCode(typeToConvert);
		return typeCode == TypeCode.Int32 ||
			typeCode == TypeCode.Decimal ||
			typeCode == TypeCode.Double ||
			typeCode == TypeCode.Single ||
			typeCode == TypeCode.Int64 ||
			typeCode == TypeCode.Int16 ||
			typeCode == TypeCode.Byte ||
			typeCode == TypeCode.UInt32 ||
			typeCode == TypeCode.UInt64 ||
			typeCode == TypeCode.UInt16 ||
			typeCode == TypeCode.SByte;
	}

	/// <summary>
	/// 创建转换器
	/// </summary>
	/// <param name="typeToConvert"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var type = typeof(StringNumberConverter<>).MakeGenericType(typeToConvert);
		var converter = Activator.CreateInstance(type);
		if (converter == null)
		{
			throw new InvalidOperationException($"无法创建 {type.Name} 类型的转换器");
		}
		return (JsonConverter)converter;
	}
}

/// <summary>
/// 文本表示的数值转换器
/// </summary>
/// <typeparam name="T"></typeparam>
public class StringNumberConverter<T> : JsonConverter<T>
{
	private static readonly TypeCode typeCode = Type.GetTypeCode(typeof(T));

	/// <summary>
	/// 读取
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="typeToConvert"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.Number:
				if (typeCode == TypeCode.Int32)
				{
					if (reader.TryGetInt32(out var value))
					{
						return Unsafe.As<int, T>(ref value);
					}
				}
				if (typeCode == TypeCode.Int64)
				{
					if (reader.TryGetInt64(out var value))
					{
						return Unsafe.As<long, T>(ref value);
					}
				}
				if (typeCode == TypeCode.Decimal)
				{
					if (reader.TryGetDecimal(out var value))
					{
						return Unsafe.As<decimal, T>(ref value);
					}
				}
				if (typeCode == TypeCode.Double)
				{
					if (reader.TryGetDouble(out var value))
					{
						return Unsafe.As<double, T>(ref value);
					}
				}
				if (typeCode == TypeCode.Single)
				{
					if (reader.TryGetSingle(out var value))
					{
						return Unsafe.As<float, T>(ref value);
					}
				}
				if (typeCode == TypeCode.Byte)
				{
					if (reader.TryGetByte(out var value))
					{
						return Unsafe.As<byte, T>(ref value);
					}
				}
				if (typeCode == TypeCode.SByte)
				{
					if (reader.TryGetSByte(out var value))
					{
						return Unsafe.As<sbyte, T>(ref value);
					}
				}
				if (typeCode == TypeCode.Int16)
				{
					if (reader.TryGetInt16(out var value))
					{
						return Unsafe.As<short, T>(ref value);
					}
				}
				if (typeCode == TypeCode.UInt16)
				{
					if (reader.TryGetUInt16(out var value))
					{
						return Unsafe.As<ushort, T>(ref value);
					}
				}
				if (typeCode == TypeCode.UInt32)
				{
					if (reader.TryGetUInt32(out var value))
					{
						return Unsafe.As<uint, T>(ref value);
					}
				}
				if (typeCode == TypeCode.UInt64)
				{
					if (reader.TryGetUInt64(out var value))
					{
						return Unsafe.As<ulong, T>(ref value);
					}
				}
				break;

			case JsonTokenType.String:
				IConvertible str = reader.GetString() ?? "";
				return (T)str.ToType(typeof(T), null);

		}

		throw new NotSupportedException($"无法将{reader.TokenType}转换为{typeToConvert}");
	}

	/// <summary>
	/// 写入
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="value"></param>
	/// <param name="options"></param>
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		switch (typeCode)
		{
			case TypeCode.Int32:
				writer.WriteNumberValue(Unsafe.As<T, int>(ref value));
				break;
			case TypeCode.UInt32:
				writer.WriteNumberValue(Unsafe.As<T, uint>(ref value));
				break;
			case TypeCode.Decimal:
				writer.WriteNumberValue(Unsafe.As<T, decimal>(ref value));
				break;
			case TypeCode.Double:
				writer.WriteNumberValue(Unsafe.As<T, double>(ref value));
				break;
			case TypeCode.Single:
				writer.WriteNumberValue(Unsafe.As<T, uint>(ref value));
				break;
			case TypeCode.UInt64:
				writer.WriteNumberValue(Unsafe.As<T, ulong>(ref value));
				break;
			case TypeCode.Int64:
				writer.WriteNumberValue(Unsafe.As<T, long>(ref value));
				break;
			case TypeCode.Int16:
				writer.WriteNumberValue(Unsafe.As<T, short>(ref value));
				break;
			case TypeCode.UInt16:
				writer.WriteNumberValue(Unsafe.As<T, ushort>(ref value));
				break;
			case TypeCode.Byte:
				writer.WriteNumberValue(Unsafe.As<T, byte>(ref value));
				break;
			case TypeCode.SByte:
				writer.WriteNumberValue(Unsafe.As<T, sbyte>(ref value));
				break;
			default:
				throw new NotSupportedException($"不支持非数字类型{typeof(T)}");
		}
	}
}


public class CustomDateTimeConverter : JsonConverter<DateTime>
{
	private readonly string _format;
	public CustomDateTimeConverter(string format)
	{
		_format = format;
	}
	public override void Write(Utf8JsonWriter writer, DateTime date, JsonSerializerOptions options)
	{
		writer.WriteStringValue(date.ToString(_format));
	}
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString() ?? throw new FormatException("当前字段格式错误");
		return DateTime.ParseExact(value, _format, null);
	}
}