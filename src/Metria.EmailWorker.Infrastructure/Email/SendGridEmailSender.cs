using System.Net.Mail;
using Microsoft.Extensions.Options;
using Metria.EmailWorker.Application.Abstractions;
using Metria.EmailWorker.Application.Exceptions;
using Metria.EmailWorker.Application.Models;
using Metria.EmailWorker.Infrastructure.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Metria.EmailWorker.Infrastructure.Email;

public sealed class SendGridEmailSender : IEmailSender
{
    private readonly SendGridOptions _options;
    private readonly SendGridClient _client;

    public SendGridEmailSender(IOptions<SendGridOptions> options)
    {
        _options = options.Value;
        _client = new SendGridClient(_options.ApiKey);
    }

    public async Task SendDigestAsync(EmailSendPayload payload, CancellationToken cancellationToken)
    {
        if (!MailAddress.TryCreate(payload.ToEmail, out _))
        {
            throw new PermanentProcessingException("Recipient email is invalid.");
        }

        if (!MailAddress.TryCreate(_options.From, out _))
        {
            throw new PermanentProcessingException("SENDGRID_FROM is invalid.");
        }

        var message = MailHelper.CreateSingleEmail(
            new EmailAddress(_options.From),
            new EmailAddress(payload.ToEmail),
            payload.Subject,
            payload.TextBody,
            payload.HtmlBody);

        var response = await _client.SendEmailAsync(message, cancellationToken);
        var statusCode = (int)response.StatusCode;

        if (statusCode is >= 200 and < 300)
        {
            return;
        }

        if (statusCode == 429 || statusCode >= 500)
        {
            throw new TransientProcessingException($"SendGrid transient failure with status {statusCode}.");
        }

        throw new PermanentProcessingException($"SendGrid permanent failure with status {statusCode}.");
    }
}
