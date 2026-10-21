using AuthApi.Data.Entities;

namespace AuthApi.Repositories;

public enum TransferStatus
{
    Completed,
    Replayed,
    InsufficientFunds,
    RecipientNotFound,
    SelfTransfer,
}

public sealed record TransferResult(TransferStatus Status, Transfer? Transfer = null);

public interface IWalletRepository
{
    Task<IReadOnlyList<Transfer>> GetRecentTransfersAsync(Guid userId, int limit, CancellationToken ct);

    Task<TransferResult> TransferAsync(
        Guid senderId, Guid recipientId, decimal amount, string? notes, Guid idempotencyKey, CancellationToken ct);
}
