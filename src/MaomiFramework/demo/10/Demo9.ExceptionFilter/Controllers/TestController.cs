using Maomi.Web.Core;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Demo9.ExceptionFilter.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet(Name = "get")]
        public IEnumerable<TestModel> Get()
        {
            throw new Exception("test");
        }
    }

    public class TestModel
    {
        public string Name { get; set; }
    }
}