using System.ComponentModel.DataAnnotations;

namespace AuthApi.Dtos;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = "";

    [Required, MinLength(8), MaxLength(128)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";
}
