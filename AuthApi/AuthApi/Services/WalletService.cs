using AuthApi.Repositories;

namespace AuthApi.Services;

public sealed class WalletService(IUserRepository users, IWalletRepository wallet) : IWalletService
{
    private const int RecentTransfersLimit = 20;

    public async Task<WalletSummary?> GetWalletAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(userId, ct);
        if (user is null)
            return null;

        return new WalletSummary(user.Balance, await wallet.GetRecentTransfersAsync(userId, RecentTransfersLimit, ct));
    }

    public async Task<TransferResult> TransferAsync(
        Guid senderId, string recipientEmail, decimal amount, string? notes, Guid idempotencyKey, CancellationToken ct)
    {
        var recipient = await users.GetByEmailAsync(AuthService.NormalizeEmail(recipientEmail), ct);
        if (recipient is null)
            return new TransferResult(TransferStatus.RecipientNotFound);
        if (recipient.Id == senderId)
            return new TransferResult(TransferStatus.SelfTransfer);

        notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        return await wallet.TransferAsync(senderId, recipient.Id, amount, notes, idempotencyKey, ct);
    }
}
