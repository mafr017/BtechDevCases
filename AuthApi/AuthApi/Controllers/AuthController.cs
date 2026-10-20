using System.Security.Claims;
using AuthApi.Dtos;
using AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth, ITokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var user = await auth.RegisterAsync(request.Email, request.Password, ct);
        if (user is null)
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Email is already registered.");

        return StatusCode(StatusCodes.Status201Created, new RegisterResponse { Id = user.Id, Email = user.Email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await auth.AuthenticateAsync(request.Email, request.Password, ct);
        if (user is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.");

        return Ok(ToResponse(tokens.CreateToken(user)));
    }

    [Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
            return Unauthorized();

        var user = await auth.FindByIdAsync(userId, ct);
        if (user is null)
            return Unauthorized();

        return Ok(ToResponse(tokens.CreateToken(user)));
    }

    private static TokenResponse ToResponse(AccessToken token) => new()
    {
        AccessToken = token.Token,
        ExpiresAt = token.ExpiresAt,
        ExpiresIn = (int)Math.Round((token.ExpiresAt - DateTime.UtcNow).TotalSeconds),
    };
}
