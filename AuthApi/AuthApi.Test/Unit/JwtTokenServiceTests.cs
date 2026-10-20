using AuthApi.Data.Entities;
using AuthApi.Options;
using AuthApi.Services;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Test.Unit;

public class JwtTokenServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Secret = "unit-test-secret-that-is-at-least-32-chars",
        Issuer = "auth-api",
        Audience = "auth-web",
        AccessTokenMinutes = 15,
    };

    private static readonly User User = new() { Id = Guid.NewGuid(), Email = "budi@example.com" };

    private static AccessToken CreateToken() =>
        new JwtTokenService(Microsoft.Extensions.Options.Options.Create(Options)).CreateToken(User);

    [Fact]
    public void Token_contains_user_id_and_email()
    {
        var jwt = new JsonWebToken(CreateToken().Token);

        Assert.Equal(User.Id.ToString(), jwt.Subject);
        Assert.Equal(User.Email, jwt.GetClaim(JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(Options.Issuer, jwt.Issuer);
        Assert.Contains(Options.Audience, jwt.Audiences);
    }

    [Fact]
    public void Token_expires_after_configured_minutes()
    {
        var token = CreateToken();
        var jwt = new JsonWebToken(token.Token);

        var expected = DateTime.UtcNow.AddMinutes(Options.AccessTokenMinutes);
        Assert.InRange(jwt.ValidTo, expected.AddSeconds(-5), expected.AddSeconds(5));
        Assert.Equal(jwt.ValidTo, token.ExpiresAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Token_is_signed_with_configured_secret()
    {
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(CreateToken().Token, new TokenValidationParameters
        {
            ValidIssuer = Options.Issuer,
            ValidAudience = Options.Audience,
            IssuerSigningKey = Options.GetSigningKey(),
        });

        Assert.True(result.IsValid);
    }
}
