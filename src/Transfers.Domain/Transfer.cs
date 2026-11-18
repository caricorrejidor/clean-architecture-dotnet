namespace Transfers.Domain;

public sealed record Transfer
{
    public Guid Id { get; }
    public string SourceAccount { get; }
    public string DestinationAccount { get; }
    public decimal Amount { get; }

    private Transfer(Guid id, string sourceAccount, string destinationAccount, decimal amount)
    {
        Id = id;
        SourceAccount = sourceAccount;
        DestinationAccount = destinationAccount;
        Amount = amount;
    }

    public static Transfer Create(Guid id, string? sourceAccount, string? destinationAccount, decimal amount)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("A transfer ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(sourceAccount) || string.IsNullOrWhiteSpace(destinationAccount))
            throw new ArgumentException("Both account identifiers are required.");
        if (sourceAccount.Trim() == destinationAccount.Trim())
            throw new ArgumentException("The source and destination must differ.");
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new ArgumentException("Amount must be positive with at most two decimal places.", nameof(amount));

        return new Transfer(id, sourceAccount.Trim(), destinationAccount.Trim(), amount);
    }
}
