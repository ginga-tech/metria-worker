using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Domain.Entities;
using Metria.EmailWorker.Domain.ValueObjects;

namespace Metria.EmailWorker.Application.Validation;

public sealed class EmailDigestMessageValidator
{
    public EmailDigest ValidateAndMap(EmailDigestMessageV1 message)
    {
        var startUtc = EnsureUtc(message.PeriodStartUtc);
        var endUtc = EnsureUtc(message.PeriodEndUtc);

        return new EmailDigest(
            message.MessageId,
            message.CorrelationId,
            message.UserId,
            new EmailAddress(message.Email),
            message.FirstName,
            message.Locale,
            message.TimeZone,
            new Period(startUtc, endUtc),
            new TemplateKey(message.TemplateKey),
            message.Metadata);
    }

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
