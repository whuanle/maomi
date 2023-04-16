using Maomi.Web.Core.Filtters;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Demo10.Api.Controllers
{
    public class UserInfo
    {
        [EmailAddress(ErrorMessage = "邮箱地址格式不正确")]
        public string Email { get; set; }
    }

    [ServiceFilter(typeof(ActionFiltter))]
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