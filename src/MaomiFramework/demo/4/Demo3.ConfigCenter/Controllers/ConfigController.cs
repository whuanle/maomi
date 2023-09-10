using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace Demo3.ConfigCenter.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ConfigController : ControllerBase
	{
		private static JsonObject JSON;
		[HttpPost("update")]
		public string Update([FromBody] JsonObject json)
		{
			JSON = json;
			return "�Ѹ�������";
		}

		[HttpGet("get")]
		public JsonObject Get() => JSON ?? new JsonObject();
	}
}