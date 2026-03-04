namespace Metria.EmailWorker.Domain.Enums;

public enum EmailDispatchStatus
{
    Processing = 0,
    Sent = 1,
    TransientFailed = 2,
    PermanentFailed = 3
}
