namespace Metria.EmailWorker.Application.Contracts.Messages;

public interface IEmailDigestMessage
{
    Guid MessageId { get; }
    Guid CorrelationId { get; }
    Guid UserId { get; }
    string Email { get; }
    string? FirstName { get; }
    string Locale { get; }
    string TimeZone { get; }
    DateTime PeriodStartUtc { get; }
    DateTime PeriodEndUtc { get; }
    string TemplateKey { get; }
    IReadOnlyDictionary<string, string> Metadata { get; }
}
