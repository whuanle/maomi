using Microsoft.AspNetCore.Mvc;

namespace Demo7.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class IndexController : ControllerBase
	{
		[HttpGet]
		public string Header([FromHeader]string? myEmail)
		{
			return myEmail;
		}
	}
}