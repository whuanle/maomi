using System.Text.Json.Serialization;
using System.Text.Json;
public class EnumStringConverter<TEnum> : JsonConverter<TEnum>
{
	private readonly bool _isNullable;

	public EnumStringConverter(bool isNullType)
	{
		_isNullable = isNullType;
	}

	public override bool CanConvert(Type objectType) => EnumStringConverterFactory.IsEnum(objectType);

	// JSON => 值
	// typeToConvert: 模型类属性/字段的类型
	public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		if (value == null)
		{
			if (_isNullable) return default;
			throw new ArgumentNullException(nameof(value));
		}

		// 是否为可空类型
		var sourceType = EnumStringConverterFactory.GetSourceType(typeof(TEnum));
		if (Enum.TryParse(sourceType, value.ToString(), out var result))
		{
			return (TEnum)result!;
		}
		throw new InvalidOperationException($"{value} 值不在枚举 {typeof(TEnum).Name} 范围中");
	}

	// 值 => JSON
	public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
	{
		if (value == null) writer.WriteNullValue();
		else writer.WriteStringValue(Enum.GetName(value.GetType(), value));
	}
}