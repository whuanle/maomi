using System.ComponentModel.DataAnnotations;

namespace Demo9.Swagger.Controllers
{
    public class UserInfo
    {
        [EmailAddress(ErrorMessage = "邮箱地址格式不正确")]
        public string Email { get; set; }
    }
}