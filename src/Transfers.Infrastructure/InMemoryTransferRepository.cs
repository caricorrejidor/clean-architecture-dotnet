using System.Collections.Concurrent;
using Transfers.Application;
using Transfers.Domain;

namespace Transfers.Infrastructure;

public sealed class InMemoryTransferRepository : ITransferRepository
{
    private readonly ConcurrentDictionary<Guid, Transfer> transfers = new();

    public RegistrationStatus Register(Transfer transfer)
    {
        if (transfers.TryAdd(transfer.Id, transfer))
            return RegistrationStatus.Created;

        return transfers[transfer.Id] == transfer ? RegistrationStatus.Replayed : RegistrationStatus.Conflict;
    }

    public Transfer? Find(Guid id) => transfers.GetValueOrDefault(id);
}
