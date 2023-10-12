using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DefaultController : ControllerBase
	{
		private readonly ITest ggg;
		public DefaultController(ITest ttt)
		{
			ggg = ttt;
		}
		[HttpGet("aaa")]
		public async Task<JsonResult> AAA(int? a, int? b)
		{
			if (a == null | b == null)
				return new JsonResult(new { code = 0, result = "aaaaaaaa" });
			return new JsonResult(new { code = 200, result = a + "|" + b });
		}
		public class AppJson { public int? a { get; set; } public int? b { get; set; } }
		[HttpPost("bbb")]
		public async Task<JsonResult> BBB([FromBody] AppJson ss)
		{
			if (ss.a == null || ss.b == null) return new JsonResult(new
			{
				code = 0,
				result = "aaaaaaaa"
			});
			return new JsonResult(new { code = 2000, result = ss.a + "|" + ss.b });
		}

		public class AppJson
		{
			public int? a { get; set; }
			public int? b { get; set; }
		}
		[HttpPost("bbb")]
		public async Task<JsonResult> BBB([FromBody] AppJson ss)
		{
			if (ss.a == null || ss.b == null) return new JsonResult(new { code = 0, result = "aaaaaaaa" });
			return new JsonResult(new { code = 200, result = ss.a + "|" + ss.b });
		}

		[HttpPost("ccc")]
		public async Task<JsonResult> CCC([FromForm] AppJson ss)
		{
			if (ss.a == null || ss.b == null)
				return new JsonResult(new { code = 0, result = "aaaaaaaa" });
			return new JsonResult(new { code = 200, result = ss.a + "|" + ss.b });
		}
		[HttpPost("ddd")]
		public async Task<JsonResult> DDD([FromHeader] int? a, [FromHeader] int? b)
		{
			if (a == null || b == null)
				return new JsonResult(new { code = 0, result = "aaaaaaaa" });
			return new JsonResult(new { code = 200, result = a + "|" + b });
		}
		[HttpPost("eee")]
		public async Task<JsonResult> EEE([FromQuery] int? a, [FromQuery] int? b)
		{
			if (a == null || b == null) return new JsonResult(new { code = 0, result = "aaaaaaaa" });
			return new JsonResult(new { code = 200, result = a + "|" + b });
		}
		[HttpPost("fff")]
		public async Task<JsonResult> FFFxxx(int a, int b, [FromRoute] string controller, [FromRoute] string action)
		{
			// 这里就不处理 a和 b了
			return new JsonResult(new { code = 200, result = controller + "|" + action });
		}
		[HttpPost("/ooo")]
		public async Task<JsonResult> FFFooo(int a, int b, [FromRoute] string controller, [FromRoute] string action)
		{
			// 这里就不处理 a和 b了
			return new JsonResult(new { code = 200, result = controller + "|" + action });
		}
		[HttpPost("ggg")]
		public async Task<JsonResult> GGG([FromServices] ITest t)
		{
			return new JsonResult(new { code = 200, result = t.GGG });
		}
		public class TestBind
		{
			public string A { get; set; }
			public string B { get; set; }
			public string C { get; set; }
			public string D { get; set; }
			public string E { get; set; }
			public string F { get; set; }
			public string G { get; set; }
		}
		[HttpPost("hhh")]
		public async Task<JsonResult> HHH(
			string A, string B,
			string E, string F, string G,
			[Bind("A,B,C,D")] TestBind test,
			 string C, string D,
			 string J, string Q)
		{
			if (ModelState.IsValid == true)
				return new JsonResult(new
				{
					data1 = test,
					dataA = A,
					dataB = B,
					dataC = C,
					dataD = D,
					dataE = E,
					dataF = F,
					dataG = G,
					dataJ = J,
					dataQ = Q
				});
			return new JsonResult(new { Code = 0, Result = "验证不通过" });
		}
	}
}