using System.Text.Json;
using System.Text.Json.Serialization;

public class EnumStringConverterFactory : JsonConverterFactory
{
	// 获取需要转换的类型
	public static bool IsEnum(Type objectType)
	{
		if (objectType.IsEnum) return true;

		var sourceType = Nullable.GetUnderlyingType(objectType);
		return sourceType is not null && sourceType.IsEnum;
	}

	public static Type GetSourceType(Type typeToConvert)
	{
		if (typeToConvert.IsEnum) return typeToConvert;
		return Nullable.GetUnderlyingType(typeToConvert);
	}

	public override bool CanConvert(Type typeToConvert) => IsEnum(typeToConvert);
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var sourceType = GetSourceType(typeToConvert);
		var converter = typeof(EnumStringConverter<>).MakeGenericType(typeToConvert);
		return (JsonConverter)Activator.CreateInstance(converter, new object[] { sourceType != typeToConvert });
	}
}
