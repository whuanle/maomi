using System.ComponentModel.DataAnnotations;
using System.Linq;

public class Program
{
    static void Main()
    {
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
    }

    private static (bool IsValid, IReadOnlyList<ValidationResult> ValidationResult) VerifyModel(object o)
    {
        var result = new List<ValidationResult>();
        var validationContext = new ValidationContext(o);
        var isValid = Validator.TryValidateObject(o, validationContext, result, validateAllProperties: true);
        return (isValid, result);
    }
}

public class UserInfo
{
    [Required]
    [EmailAddress]
    [MaomiEmail]
    public string Email { get; set; }
}

public class MaomiEmailAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is string email)
        {
            return email.EndsWith("@maomi.com");
        }
        return false;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // 成员名称
        var memberName = validationContext.MemberName;
        // [DisplayName] 定义的名称
        var displayName = validationContext.DisplayName;
        // 实例对象
        var obj = validationContext.ObjectInstance;
        if (this.IsValid(value))
        {
            return ValidationResult.Success;
        }
        string[] memberNames = memberName != null ? new string[] { memberName } : null;
        return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName), memberNames);
    }
    public override string FormatErrorMessage(string name)
    {
        return $"{name} 不是 @maomi.com 邮箱后缀！";
    }
}