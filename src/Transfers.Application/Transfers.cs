using MediatR;
using Transfers.Domain;

namespace Transfers.Application;

public enum RegistrationStatus { Created, Replayed, Conflict, Invalid }

public sealed record RegistrationResult(RegistrationStatus Status, Transfer? Transfer = null, string? Error = null);

public interface ITransferRepository
{
    RegistrationStatus Register(Transfer transfer);
    Transfer? Find(Guid id);
}

public sealed record RegisterTransfer(Guid Id, string? SourceAccount, string? DestinationAccount, decimal Amount)
    : IRequest<RegistrationResult>;

public sealed record GetTransfer(Guid Id) : IRequest<Transfer?>;

public sealed class RegisterTransferHandler(ITransferRepository repository)
    : IRequestHandler<RegisterTransfer, RegistrationResult>
{
    public Task<RegistrationResult> Handle(RegisterTransfer request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Transfer transfer;
        try
        {
            transfer = Transfer.Create(request.Id, request.SourceAccount, request.DestinationAccount, request.Amount);
        }
        catch (ArgumentException exception)
        {
            return Task.FromResult(new RegistrationResult(RegistrationStatus.Invalid, Error: exception.Message));
        }

        var status = repository.Register(transfer);
        return Task.FromResult(new RegistrationResult(status, status == RegistrationStatus.Conflict ? null : transfer));
    }
}

public sealed class GetTransferHandler(ITransferRepository repository) : IRequestHandler<GetTransfer, Transfer?>
{
    public Task<Transfer?> Handle(GetTransfer request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(repository.Find(request.Id));
    }
}
