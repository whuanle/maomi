using Microsoft.AspNetCore.Mvc;

namespace Demo9.MaomiSwagger.Controllers
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

		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test2")]
		public string Get2() => "true";
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

		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test2")]
		public string Get2() => "true";
	}

	/// <summary>
	/// ¿ØÖÆÆ÷C
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("1.0")]
	public class CController : ControllerBase
	{
		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test1")]
		[ApiVersion("1.0")]
		public string Get1() => "true";

		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test2")]
		[ApiVersion("2.0")]
		public string Get2() => "true";
	}

	/// <summary>
	/// ¿ØÖÆÆ÷D
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	public class DController : ControllerBase
	{
		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		[ApiVersion("2.0")]
		public string Get() => "true";
	}

	/// <summary>
	/// ¿ØÖÆÆ÷E
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiExplorerSettings(GroupName = "¿ØÖÆÆ÷E")]
	public class EController : ControllerBase
	{
		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		public string Get() => "true";
	}

	/// <summary>
	/// ¿ØÖÆÆ÷F
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("1.0")]
	[ApiExplorerSettings(GroupName = "¿ØÖÆÆ÷F")]
	public class FController : ControllerBase
	{
		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		public string Get() => "true";
	}

	/// <summary>
	/// ¿ØÖÆÆ÷G
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("1.0")]
	public class GController : ControllerBase
	{
		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		[ApiVersion("2.0")]
		public string Get() => "true";
	}

	/// <summary>
	/// ¿ØÖÆÆ÷H
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiExplorerSettings(GroupName = "¿ØÖÆÆ÷H")]
	public class HController : ControllerBase
	{
		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		[ApiVersion("2.0")]
		public string Get() => "true";
	}

	/// <summary>
	/// ¿ØÖÆÆ÷H
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiExplorerSettings(GroupName = "¿ØÖÆÆ÷H")]
	[ApiVersion("2.0")]
	public class IController : ControllerBase
	{
		/// <summary>
		/// ²âÊÔ
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		public string Get() => "true";
	}
}