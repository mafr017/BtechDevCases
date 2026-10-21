using AuthApi.Data;
using AuthApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AuthApi.Repositories;

public sealed class WalletRepository(AppDbContext db) : IWalletRepository
{
    public async Task<IReadOnlyList<Transfer>> GetRecentTransfersAsync(Guid userId, int limit, CancellationToken ct) =>
        await TransfersWithUsers()
            .Where(t => t.SenderId == userId || t.RecipientId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<TransferResult> TransferAsync(
        Guid senderId, Guid recipientId, decimal amount, string? notes, Guid idempotencyKey, CancellationToken ct)
    {
        var existing = await FindByIdempotencyKeyAsync(senderId, idempotencyKey, ct);
        if (existing is not null)
            return new TransferResult(TransferStatus.Replayed, existing);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var accounts = await db.Users
            .FromSql($"SELECT * FROM users WHERE id IN ({senderId}, {recipientId}) ORDER BY id FOR UPDATE")
            .ToListAsync(ct);
        var sender = accounts.Single(u => u.Id == senderId);
        var recipient = accounts.Single(u => u.Id == recipientId);

        if (sender.Balance < amount)
            return new TransferResult(TransferStatus.InsufficientFunds);

        sender.Balance -= amount;
        recipient.Balance += amount;
        var transfer = new Transfer
        {
            Id = Guid.CreateVersion7(),
            SenderId = senderId,
            RecipientId = recipientId,
            Amount = amount,
            Notes = notes,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow,
        };
        db.Transfers.Add(transfer);

        try
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await tx.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return new TransferResult(TransferStatus.Replayed, await FindByIdempotencyKeyAsync(senderId, idempotencyKey, ct));
        }

        return new TransferResult(TransferStatus.Completed, transfer);
    }

    private Task<Transfer?> FindByIdempotencyKeyAsync(Guid senderId, Guid idempotencyKey, CancellationToken ct) =>
        TransfersWithUsers().FirstOrDefaultAsync(t => t.SenderId == senderId && t.IdempotencyKey == idempotencyKey, ct);

    private IQueryable<Transfer> TransfersWithUsers() =>
        db.Transfers.AsNoTracking().Include(t => t.Sender).Include(t => t.Recipient);
}
