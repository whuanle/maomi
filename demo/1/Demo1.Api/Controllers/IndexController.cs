using Demo1.Application;
using Microsoft.AspNetCore.Mvc;

namespace Demo1.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndexController : ControllerBase
    {

        private readonly IMyService _service;

        public IndexController(IMyService service)
        {
            _service = service;
        }

        [HttpGet(Name = "sum")]
        public int Get(int a, int b)
        {
            return _service.Sum(a, b);
        }
    }
}