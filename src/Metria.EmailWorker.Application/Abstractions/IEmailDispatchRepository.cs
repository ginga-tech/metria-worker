using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Application.Models;

namespace Metria.EmailWorker.Application.Abstractions;

public interface IEmailDispatchRepository
{
    Task<EmailDispatchReservation?> TryReserveAsync(EmailDigestMessageV1 message, CancellationToken cancellationToken);
    Task MarkSentAsync(Guid dispatchLogId, DateTime sentAtUtc, CancellationToken cancellationToken);
    Task MarkFailedAsync(Guid dispatchLogId, string status, CancellationToken cancellationToken);
}
