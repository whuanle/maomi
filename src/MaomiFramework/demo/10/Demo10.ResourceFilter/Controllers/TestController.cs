using Maomi.Web.Core;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Demo10.ResourceFilter.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {

        [HttpGet(Name = "get")]
        public IEnumerable<TestModel> Get()
        {
            var data = new List<TestModel>()
            {
                new TestModel(){ Name = "1"},
                new TestModel(){ Name = "2"}
            };
            return data;
        }
    }

    public class TestModel
    {
        public string Name { get; set; }
    }
}