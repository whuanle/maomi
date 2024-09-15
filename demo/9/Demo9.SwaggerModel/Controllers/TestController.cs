using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Demo9.SwaggerModel.Controllers
{

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