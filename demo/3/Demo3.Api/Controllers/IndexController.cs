using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace Demo3.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class IndexController : ControllerBase
	{
		[HttpGet("get")]
		public string Get([FromServices] IConfiguration configuration, string name) => configuration[name];
	}
}