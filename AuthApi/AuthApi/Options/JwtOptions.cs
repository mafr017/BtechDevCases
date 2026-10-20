using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32)]
    public string Secret { get; set; } = null!;

    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 15;

    public SymmetricSecurityKey GetSigningKey() => new(Encoding.UTF8.GetBytes(Secret));
}
