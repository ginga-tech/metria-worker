using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Metria.EmailWorker.Application.Abstractions;
using Metria.EmailWorker.Application.Exceptions;
using Metria.EmailWorker.Application.Models;
using Metria.EmailWorker.Infrastructure.Configuration;

namespace Metria.EmailWorker.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendDigestAsync(EmailSendPayload payload, CancellationToken cancellationToken)
    {
        if (!MailboxAddress.TryParse(payload.ToEmail, out var recipient))
        {
            throw new PermanentProcessingException("Recipient email is invalid.");
        }

        if (!MailboxAddress.TryParse(_options.From, out var from))
        {
            throw new PermanentProcessingException("SMTP_FROM is invalid.");
        }

        var message = new MimeMessage();
        message.From.Add(from);
        message.To.Add(recipient);
        message.Subject = payload.Subject;
        message.Body = new BodyBuilder
        {
            HtmlBody = payload.HtmlBody,
            TextBody = payload.TextBody
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        try
        {
            await smtp.ConnectAsync(
                _options.Host,
                _options.Port,
                SecureSocketOptions.StartTlsWhenAvailable,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.User))
            {
                await smtp.AuthenticateAsync(_options.User, _options.Password, cancellationToken);
            }

            await smtp.SendAsync(message, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
        catch (SmtpCommandException ex) when ((int)ex.StatusCode >= 500)
        {
            throw new TransientProcessingException("SMTP 5xx response from provider.", ex);
        }
        catch (SmtpCommandException ex)
        {
            throw new PermanentProcessingException("SMTP command rejected permanently.", ex);
        }
        catch (SmtpProtocolException ex)
        {
            throw new TransientProcessingException("SMTP protocol/network failure.", ex);
        }
        catch (TimeoutException ex)
        {
            throw new TransientProcessingException("SMTP timeout.", ex);
        }
    }
}
