namespace AuthApi.Dtos;

public class TokenResponse
{
    public string AccessToken { get; set; } = "";

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAt { get; set; }

    public int ExpiresIn { get; set; }
}
