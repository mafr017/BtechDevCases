namespace AuthApi.Data.Entities;

public class Transfer
{
    public Guid Id { get; set; }

    public Guid SenderId { get; set; }

    public User Sender { get; set; } = null!;

    public Guid RecipientId { get; set; }

    public User Recipient { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Notes { get; set; }

    public Guid IdempotencyKey { get; set; }

    public DateTime CreatedAt { get; set; }
}
