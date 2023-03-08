using System.ComponentModel;
using System.Text.Json.Serialization;

public class Model
{
	public string Name { get; set; }
	// [EnumConverter]
	// [JsonConverter(typeof(EnumConverter))]
	public NetworkType Netwotk1 { get; set; }
	// [EnumConverter]
	// [JsonConverter(typeof(EnumConverter))]
	public NetworkType? Netwotk2 { get; set; }
}
