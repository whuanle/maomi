using Maomi.Web.Core.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Demo10.Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class IndexController : ControllerBase
    {
        [HttpPost("user")]
        public string GetUserName([FromBody] UserInfo info)
        {
            return info.Email.Split("@").FirstOrDefault();
        }
    }
}