using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using Newtonsoft.Json.Linq;

namespace Demo6.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class IndexController : ControllerBase
	{
		[HttpGet("/query")]
		public string Query([FromQuery] string a, [FromQuery] string b)
		{
			return a + b;
		}

		[HttpGet("/header")]
		public string Header([FromHeader] string? myEmail)
		{
			return myEmail;
		}

		[HttpPost("/form1")]
		public string Form1([FromForm]Dictionary<string,string> dic)
		{
			return "success";
		}

		[HttpPost("/form2")]
		public string Form2([FromForm] Form2Model model)
		{
			return "success";
		}

		public class Form2Model
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public string Number { get; set; }
		}

		[HttpPost("/form3")]
		public string Form3([FromForm] IFormFile img)
		{
			return "success";
		}

		[HttpPost("/form4")]
		public string Form4([FromForm] IFormFileCollection imgs)
		{
			return "success";
		}

		[HttpPost("/form5")]
		public string Form5([FromForm] Form5Model model)
		{
			return "success";
		}

		public class Form5Model
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public IFormFile Img { get; set; }
		}

		[HttpPost("/json")]
		public string Json([FromBody] JsonModel model)
		{
			return "success";
		}

		[HttpPost("/json1")]
		public string Json1([FromBody] object model)
		{
			return "success";
		}

		public class JsonModel
		{
			public string Id { get; set; }
			public string Name { get; set; }
		}

		[HttpPost("/json2")]
		public string Json2([FromBody]object model)
		{
			if (model is System.Text.Json.Nodes.JsonObject jsonObject)
			{
			}
			else if (model is Newtonsoft.Json.Linq.JObject jObject)
			{
			}
			return "success";
		}
	}
}