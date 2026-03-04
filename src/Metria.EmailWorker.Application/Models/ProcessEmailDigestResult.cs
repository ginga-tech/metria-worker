namespace Metria.EmailWorker.Application.Models;

public sealed record ProcessEmailDigestResult(
    bool Sent,
    bool SkippedDuplicate,
    Guid MessageId,
    Guid CorrelationId,
    Guid UserId)
{
    public static ProcessEmailDigestResult Duplicate(Guid messageId, Guid correlationId, Guid userId) =>
        new(false, true, messageId, correlationId, userId);

    public static ProcessEmailDigestResult SentOk(Guid messageId, Guid correlationId, Guid userId) =>
        new(true, false, messageId, correlationId, userId);
}
