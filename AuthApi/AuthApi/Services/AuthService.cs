using AuthApi.Data.Entities;
using AuthApi.Repositories;
using Microsoft.AspNetCore.Identity;

namespace AuthApi.Services;

public sealed class AuthService(IUserRepository users, IPasswordHasher<User> hasher) : IAuthService
{
    private static readonly string DummyHash = new PasswordHasher<User>().HashPassword(new User(), "dummy-password");

    public async Task<User?> RegisterAsync(string email, string password, CancellationToken ct)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (await users.EmailExistsAsync(normalizedEmail, ct))
            return null;

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = normalizedEmail,
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = hasher.HashPassword(user, password);

        return await users.TryAddAsync(user, ct) ? user : null;
    }

    public async Task<User?> AuthenticateAsync(string email, string password, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(NormalizeEmail(email), ct);
        if (user is null)
        {
            hasher.VerifyHashedPassword(new User(), DummyHash, password);
            return null;
        }

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    public Task<User?> FindByIdAsync(Guid id, CancellationToken ct) => users.GetByIdAsync(id, ct);

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
