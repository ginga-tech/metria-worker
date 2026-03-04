using Metria.EmailWorker.Domain.Exceptions;
using Metria.EmailWorker.Domain.ValueObjects;

namespace Metria.EmailWorker.Domain.Entities;

public sealed class EmailDigest
{
    public Guid MessageId { get; }
    public Guid CorrelationId { get; }
    public Guid UserId { get; }
    public EmailAddress Email { get; }
    public string? FirstName { get; }
    public string Locale { get; }
    public string TimeZone { get; }
    public Period Period { get; }
    public TemplateKey TemplateKey { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public EmailDigest(
        Guid messageId,
        Guid correlationId,
        Guid userId,
        EmailAddress email,
        string? firstName,
        string locale,
        string timeZone,
        Period period,
        TemplateKey templateKey,
        IReadOnlyDictionary<string, string>? metadata)
    {
        if (messageId == Guid.Empty)
        {
            throw new DomainValidationException("MessageId must not be empty.");
        }

        if (correlationId == Guid.Empty)
        {
            throw new DomainValidationException("CorrelationId must not be empty.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("UserId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(locale))
        {
            throw new DomainValidationException("Locale must be provided.");
        }

        if (string.IsNullOrWhiteSpace(timeZone))
        {
            throw new DomainValidationException("TimeZone must be provided.");
        }

        MessageId = messageId;
        CorrelationId = correlationId;
        UserId = userId;
        Email = email;
        FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim();
        Locale = locale.Trim();
        TimeZone = timeZone.Trim();
        Period = period;
        TemplateKey = templateKey;
        Metadata = metadata ?? new Dictionary<string, string>();
    }
}
