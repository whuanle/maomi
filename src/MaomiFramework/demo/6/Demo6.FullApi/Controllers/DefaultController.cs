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
		//private readonly ITest ggg;
		//public DefaultController(ITest ttt)
		//{
		//	ggg = ttt;
		//}

		public class AObj
		{
			public int B { get; set; }
		}

		[HttpGet("ao")]
		public async Task<int> AO(int? a, AObj obj)
		{
			return 0;
		}

		[HttpGet("a")]
		public async Task<int> A([FromQuery] int? a, [FromBody] AObj obj)
		{
			return 0;
		}

		[HttpGet("ac")]
		public async Task<int> AC([FromQuery] int? a, [FromBody] AObj obj, CancellationToken cancellationToken)
		{
			return 0;
		}
	}
}