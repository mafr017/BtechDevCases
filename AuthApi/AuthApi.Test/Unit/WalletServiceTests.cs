using AuthApi.Data.Entities;
using AuthApi.Repositories;
using AuthApi.Services;

namespace AuthApi.Test.Unit;

public class WalletServiceTests
{
    private static readonly User Budi = new() { Id = Guid.NewGuid(), Email = "budi@example.com", Balance = 1000m };
    private static readonly User Siti = new() { Id = Guid.NewGuid(), Email = "siti@example.com", Balance = 1000m };

    private readonly FakeWalletRepository _wallet = new();
    private readonly WalletService _service;

    public WalletServiceTests()
    {
        _service = new WalletService(new FakeUserRepository(Budi, Siti), _wallet);
    }

    [Fact]
    public async Task Transfer_to_unknown_recipient_is_rejected_without_moving_money()
    {
        var result = await _service.TransferAsync(Budi.Id, "nobody@example.com", 10m, null, Guid.NewGuid(), default);

        Assert.Equal(TransferStatus.RecipientNotFound, result.Status);
        Assert.Null(_wallet.LastCall);
    }

    [Fact]
    public async Task Transfer_to_yourself_is_rejected_without_moving_money()
    {
        var result = await _service.TransferAsync(Budi.Id, "budi@example.com", 10m, null, Guid.NewGuid(), default);

        Assert.Equal(TransferStatus.SelfTransfer, result.Status);
        Assert.Null(_wallet.LastCall);
    }

    [Fact]
    public async Task Transfer_finds_recipient_by_normalized_email_and_trims_notes()
    {
        var key = Guid.NewGuid();

        var result = await _service.TransferAsync(Budi.Id, "  Siti@Example.com ", 10m, "  lunch  ", key, default);

        Assert.Equal(TransferStatus.Completed, result.Status);
        Assert.Equal((Budi.Id, Siti.Id, 10m, "lunch", key), _wallet.LastCall);
    }

    [Fact]
    public async Task Blank_notes_are_stored_as_null()
    {
        await _service.TransferAsync(Budi.Id, "siti@example.com", 10m, "   ", Guid.NewGuid(), default);

        Assert.Null(_wallet.LastCall!.Value.Notes);
    }

    [Fact]
    public async Task Wallet_shows_balance_and_recent_transfers()
    {
        var summary = await _service.GetWalletAsync(Budi.Id, default);

        Assert.NotNull(summary);
        Assert.Equal(1000m, summary.Balance);
        Assert.Equal(Budi.Id, _wallet.LastHistoryUserId);
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(users.FirstOrDefault(u => u.Id == id));

        public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct) =>
            Task.FromResult(users.FirstOrDefault(u => u.Email == normalizedEmail));

        public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct) =>
            Task.FromResult(users.Any(u => u.Email == normalizedEmail));

        public Task<bool> TryAddAsync(User user, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakeWalletRepository : IWalletRepository
    {
        public (Guid SenderId, Guid RecipientId, decimal Amount, string? Notes, Guid Key)? LastCall { get; private set; }

        public Guid? LastHistoryUserId { get; private set; }

        public Task<IReadOnlyList<Transfer>> GetRecentTransfersAsync(Guid userId, int limit, CancellationToken ct)
        {
            LastHistoryUserId = userId;
            return Task.FromResult<IReadOnlyList<Transfer>>([]);
        }

        public Task<TransferResult> TransferAsync(
            Guid senderId, Guid recipientId, decimal amount, string? notes, Guid idempotencyKey, CancellationToken ct)
        {
            LastCall = (senderId, recipientId, amount, notes, idempotencyKey);
            return Task.FromResult(new TransferResult(TransferStatus.Completed));
        }
    }
}
