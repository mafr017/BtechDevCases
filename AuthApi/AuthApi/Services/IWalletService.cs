using AuthApi.Data.Entities;
using AuthApi.Repositories;

namespace AuthApi.Services;

public sealed record WalletSummary(decimal Balance, IReadOnlyList<Transfer> Transfers);

public interface IWalletService
{
    Task<WalletSummary?> GetWalletAsync(Guid userId, CancellationToken ct);

    Task<TransferResult> TransferAsync(
        Guid senderId, string recipientEmail, decimal amount, string? notes, Guid idempotencyKey, CancellationToken ct);
}
