using System.ComponentModel.DataAnnotations;
using System.Linq;

public class Program
{
    static void Main()
    {
        // 示例 1
        var json = """
            {
              "Email":"aaa@qq.com"
            }
            """;
        var userInfo = System.Text.Json.JsonSerializer.Deserialize<UserInfo>(json);

        var (isValid, result) = VerifyModel(userInfo);
        if (!isValid)
        {
            foreach (var item in result)
            {
                Console.WriteLine($"{item.MemberNames.First()}:{item.ErrorMessage}");
            }
        }


        //bool isValid = new EmailAddressAttribute().IsValid(userInfo);

        // 示例 2
        var userInfoMaomi = System.Text.Json.JsonSerializer.Deserialize<UserInfoMaomi>(json);
        var validResult1 = new MaomiEmailAttribute().IsValid(userInfoMaomi);
        var (isValid2, result2) = VerifyModel(userInfoMaomi);
    }

    private static (bool IsValid, IReadOnlyList<ValidationResult> ValidationResult) VerifyModel(object o)
    {
        var result = new List<ValidationResult>();
        var validationContext = new ValidationContext(o);
        var isValid = Validator.TryValidateObject(o, validationContext, result, validateAllProperties: true);
        return (isValid, result);
    }
}
