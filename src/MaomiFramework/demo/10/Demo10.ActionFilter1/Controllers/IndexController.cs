using Maomi.Web.Core.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Demo10.ActionFilter1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndexController : ControllerBase
    {
        [ServiceFilter(typeof(MaomiActionFilter))]
        [HttpPost("user")]
        public string GetUserName([FromBody] UserInfo info)
        {
            return info.Email.Split("@").FirstOrDefault();
        }
    }
}