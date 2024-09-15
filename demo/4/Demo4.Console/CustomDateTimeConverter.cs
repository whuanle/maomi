using System.Text.Json;
using System.Text.Json.Serialization;

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