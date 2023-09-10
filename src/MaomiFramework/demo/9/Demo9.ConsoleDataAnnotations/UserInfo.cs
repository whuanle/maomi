using System.ComponentModel.DataAnnotations;

public class UserInfo
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

public class UserInfoMaomi
{
    [Required]
    [EmailAddress]
    [MaomiEmail]
    public string Email { get; set; }
}