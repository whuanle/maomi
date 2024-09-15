using Microsoft.AspNetCore.Mvc;

namespace Demo9.SwaggerVersion.Controllers
{
	/// <summary>
	/// ¿ØÖÆÆ÷A
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	public class AController : ControllerBase
	{
		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test1")]
		public string Get1() => "true";
	}

	/// <summary>
	/// ¿ØÖÆÆ÷B
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("1.0")]
	public class BController : ControllerBase
	{
		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test1")]
		public string Get1() => "true";
	}

	/// <summary>
	/// ¿ØÖÆÆ÷C
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("2.0")]
	public class CController : ControllerBase
	{
		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test1")]
		public string Get1() => "true";
	}
}