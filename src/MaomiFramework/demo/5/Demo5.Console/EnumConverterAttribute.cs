using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Text.Json.Serialization;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class EnumConverterAttribute : JsonConverterAttribute
{
	public override JsonConverter CreateConverter(Type typeToConvert)
	{
		return new EnumStringConverterFactory();
	}
}