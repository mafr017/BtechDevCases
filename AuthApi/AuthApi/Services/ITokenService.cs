using AuthApi.Data.Entities;

namespace AuthApi.Services;

public class AccessToken
{
    public string Token { get; set; } = "";

    public DateTime ExpiresAt { get; set; }
}

public interface ITokenService
{
    AccessToken CreateToken(User user);
}
