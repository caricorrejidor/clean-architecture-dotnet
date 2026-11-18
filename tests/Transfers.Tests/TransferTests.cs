using Transfers.Application;
using Transfers.Infrastructure;

namespace Transfers.Tests;

public sealed class TransferTests
{
    private static RegisterTransfer Sample() => new(Guid.NewGuid(), "ACCT-01", "ACCT-02", 125.50m);

    [Fact]
    public async Task RegistersAndReadsATransfer()
    {
        var repository = new InMemoryTransferRepository();
        var command = Sample();
        var result = await new RegisterTransferHandler(repository).Handle(command, CancellationToken.None);
        var transfer = await new GetTransferHandler(repository).Handle(new GetTransfer(command.Id), CancellationToken.None);

        Assert.Equal(RegistrationStatus.Created, result.Status);
        Assert.NotNull(transfer);
        Assert.Equal(command.Amount, transfer.Amount);
        Assert.Equal(result.Transfer, transfer);
    }

    [Fact]
    public async Task ReplaysAnIdenticalTransferAndRejectsConflictingDetails()
    {
        var repository = new InMemoryTransferRepository();
        var handler = new RegisterTransferHandler(repository);
        var command = Sample();

        Assert.Equal(RegistrationStatus.Created, (await handler.Handle(command, CancellationToken.None)).Status);
        Assert.Equal(RegistrationStatus.Replayed, (await handler.Handle(command, CancellationToken.None)).Status);
        var conflict = await handler.Handle(command with { Amount = 40m }, CancellationToken.None);

        Assert.Equal(RegistrationStatus.Conflict, conflict.Status);
        Assert.Equal(command.Amount, repository.Find(command.Id)?.Amount);
    }

    [Theory]
    [InlineData("ACCT-01", "ACCT-01", "10")]
    [InlineData("ACCT-01", "ACCT-02", "0")]
    [InlineData("ACCT-01", "ACCT-02", "1.001")]
    [InlineData("ACCT-01", " ", "10")]
    public async Task RejectsInvalidTransfers(string source, string destination, string amount)
    {
        var repository = new InMemoryTransferRepository();
        var command = new RegisterTransfer(Guid.NewGuid(), source, destination,
            decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture));
        var result = await new RegisterTransferHandler(repository).Handle(command, CancellationToken.None);

        Assert.Equal(RegistrationStatus.Invalid, result.Status);
        Assert.Null(repository.Find(command.Id));
    }
}
