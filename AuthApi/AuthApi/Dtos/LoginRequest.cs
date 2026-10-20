using System.ComponentModel.DataAnnotations;

namespace AuthApi.Dtos;

public class LoginRequest
{
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = "";

    [Required, MaxLength(128)]
    public string Password { get; set; } = "";
}
