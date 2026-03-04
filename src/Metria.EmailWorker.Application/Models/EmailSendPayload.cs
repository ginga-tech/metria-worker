namespace Metria.EmailWorker.Application.Models;

public sealed record EmailSendPayload(
    Guid MessageId,
    Guid CorrelationId,
    Guid UserId,
    string ToEmail,
    string Subject,
    string HtmlBody,
    string TextBody);
