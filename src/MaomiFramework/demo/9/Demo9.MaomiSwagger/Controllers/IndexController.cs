using Microsoft.AspNetCore.Mvc;

namespace Demo9.MaomiSwagger.Controllers
{
	/// <summary>
	/// ������A
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	public class AController : ControllerBase
	{
		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test1")]
		public string Get1() => "true";

		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test2")]
		public string Get2() => "true";
	}

	/// <summary>
	/// ������B
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("1.0")]
	public class BController : ControllerBase
	{
		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test1")]
		public string Get1() => "true";

		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test2")]
		public string Get2() => "true";
	}

	/// <summary>
	/// ������C
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("1.0")]
	public class CController : ControllerBase
	{
		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test1")]
		[ApiVersion("1.0")]
		public string Get1() => "true";

		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test2")]
		[ApiVersion("2.0")]
		public string Get2() => "true";
	}

	/// <summary>
	/// ������D
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	public class DController : ControllerBase
	{
		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		[ApiVersion("2.0")]
		public string Get() => "true";
	}

	/// <summary>
	/// ������E
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiExplorerSettings(GroupName = "������E")]
	public class EController : ControllerBase
	{
		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		public string Get() => "true";
	}

	/// <summary>
	/// ������F
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("1.0")]
	[ApiExplorerSettings(GroupName = "������F")]
	public class FController : ControllerBase
	{
		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		public string Get() => "true";
	}

	/// <summary>
	/// ������G
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("1.0")]
	public class GController : ControllerBase
	{
		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		[ApiVersion("2.0")]
		public string Get() => "true";
	}

	/// <summary>
	/// ������H
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiExplorerSettings(GroupName = "������H")]
	public class HController : ControllerBase
	{
		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		[ApiVersion("2.0")]
		public string Get() => "true";
	}

	/// <summary>
	/// ������H
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[ApiExplorerSettings(GroupName = "������H")]
	[ApiVersion("2.0")]
	public class IController : ControllerBase
	{
		/// <summary>
		/// ����
		/// </summary>
		/// <returns></returns>
		[HttpGet("test")]
		public string Get() => "true";
	}
}