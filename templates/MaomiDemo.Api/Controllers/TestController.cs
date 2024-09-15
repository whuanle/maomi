using MaomiDemo.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MaomiDemo.Api.Controllers
{
	/// <summary>
	/// øÿ÷∆∆˜B
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("1.0")]
	public class TestController : ControllerBase
	{
		/// <summary>
		/// ≤‚ ‘
		/// </summary>
		/// <returns></returns>
		[HttpGet("test1")]
		public string Get1() => "true";

		/// <summary>
		/// ≤‚ ‘
		/// </summary>
		/// <returns></returns>
		[HttpGet("test2")]
		public int Get2([FromServices] IMyService myService, int a, int b)
		{
			return myService.Sum(a, b);
		}
	}
}