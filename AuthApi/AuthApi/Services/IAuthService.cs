using AuthApi.Data.Entities;

namespace AuthApi.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(string email, string password, CancellationToken ct);

    Task<User?> AuthenticateAsync(string email, string password, CancellationToken ct);

    Task<User?> FindByIdAsync(Guid id, CancellationToken ct);
}
