using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Demo9.SwaggerModel.Controllers
{
	/// <summary>
	/// Ä£ÐÍÀà
	/// </summary>
	public class Test
	{
		[JsonConverter(typeof(string))]
		public Boolean Value1 { get; set; }

		[JsonConverter(typeof(string))]
		public char Value2 { get; set; }

		[JsonConverter(typeof(string))]
		public sbyte Value3 { get; set; }

		[JsonConverter(typeof(string))]
		public byte Value4 { get; set; }

		[JsonConverter(typeof(string))]
		public Int16 Value5 { get; set; }

		[JsonConverter(typeof(string))]
		public UInt16 Value6 { get; set; }

		[JsonConverter(typeof(string))]
		public Int32 Value7 { get; set; }

		[JsonConverter(typeof(string))]
		public UInt32 Value8 { get; set; }

		[JsonConverter(typeof(string))]
		public Int64 Value9 { get; set; }

		[JsonConverter(typeof(string))]
		public UInt64 Value { get; set; }

		[JsonConverter(typeof(string))]
		public Single Value10 { get; set; }

		[JsonConverter(typeof(string))]
		public Double Value11 { get; set; }

		[JsonConverter(typeof(string))]
		public Decimal Value12 { get; set; }

		[JsonConverter(typeof(string))]
		public DateTime Value13 { get; set; }

		[JsonConverter(typeof(string))]
		public String Value14 { get; set; }
	}
	[ApiController]
	[Route("[controller]")]
	public class TestController : ControllerBase
	{
		[HttpPost("test")]
		public string Test([FromBody] Test test)
		{
			return "test";
		}
	}
}