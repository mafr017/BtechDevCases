using AuthApi.Data;
using AuthApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AuthApi.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);

    public async Task<bool> TryAddAsync(User user, CancellationToken ct)
    {
        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            db.Entry(user).State = EntityState.Detached;
            return false;
        }
    }
}
