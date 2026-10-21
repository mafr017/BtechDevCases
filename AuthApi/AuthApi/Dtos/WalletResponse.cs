namespace AuthApi.Dtos;

public class WalletResponse
{
    public decimal Balance { get; set; }

    public List<TransferItem> Transfers { get; set; } = [];
}

public class TransferItem
{
    public Guid Id { get; set; }

    public string Direction { get; set; } = "";

    public string Counterparty { get; set; } = "";

    public decimal Amount { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
