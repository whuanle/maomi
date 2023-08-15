using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Demo7.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndexController : ControllerBase
    {
        [HttpGet("name")]
        public string GetName([FromQuery] string name)
        {
            return name;
        }
    }
}