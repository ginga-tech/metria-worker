using Metria.EmailWorker.Application.Abstractions;
using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Application.Exceptions;
using Metria.EmailWorker.Application.Models;

namespace Metria.EmailWorker.Infrastructure.Email;

public sealed class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private static readonly HashSet<string> SupportedTemplates = new(StringComparer.OrdinalIgnoreCase)
    {
        "digest.v1",
        "digest.biweekly.v1",
        "weekly-digest"
    };

    public EmailSendPayload Render(EmailDigestMessageV1 message)
    {
        if (!SupportedTemplates.Contains(message.TemplateKey))
        {
            throw new PermanentProcessingException($"Template '{message.TemplateKey}' is not supported.");
        }

        var greeting = ResolveGreeting(message.Locale, message.FirstName);
        var subject = BuildSubject(message.Locale, message.PeriodStartUtc, message.PeriodEndUtc);

        var htmlBody =
            $"""
             <html>
               <body>
                 <p>{greeting},</p>
                 <p>Your digest for {message.PeriodStartUtc:yyyy-MM-dd} to {message.PeriodEndUtc:yyyy-MM-dd} is ready.</p>
                 <p>Template: {message.TemplateKey}</p>
               </body>
             </html>
             """;

        var textBody =
            $"{greeting}, your digest for {message.PeriodStartUtc:yyyy-MM-dd} to {message.PeriodEndUtc:yyyy-MM-dd} is ready.";

        return new EmailSendPayload(
            message.MessageId,
            message.CorrelationId,
            message.UserId,
            message.Email,
            subject,
            htmlBody,
            textBody);
    }

    private static string ResolveGreeting(string locale, string? firstName)
    {
        var safeName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim();
        var baseGreeting = locale.StartsWith("pt", StringComparison.OrdinalIgnoreCase)
            ? "Olá"
            : locale.StartsWith("es", StringComparison.OrdinalIgnoreCase)
                ? "Hola"
                : "Hello";

        return safeName is null ? $"{baseGreeting} there" : $"{baseGreeting} {safeName}";
    }

    private static string BuildSubject(string locale, DateTime periodStartUtc, DateTime periodEndUtc)
    {
        if (locale.StartsWith("pt", StringComparison.OrdinalIgnoreCase))
        {
            return $"Resumo Metria ({periodStartUtc:dd/MM} - {periodEndUtc:dd/MM})";
        }

        if (locale.StartsWith("es", StringComparison.OrdinalIgnoreCase))
        {
            return $"Resumen Metria ({periodStartUtc:dd/MM} - {periodEndUtc:dd/MM})";
        }

        return $"Metria Digest ({periodStartUtc:MM/dd} - {periodEndUtc:MM/dd})";
    }
}
