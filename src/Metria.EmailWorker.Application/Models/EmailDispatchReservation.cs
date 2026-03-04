namespace Metria.EmailWorker.Application.Models;

public sealed record EmailDispatchReservation(
    Guid DispatchLogId,
    Guid MessageId,
    bool IsNewReservation);
