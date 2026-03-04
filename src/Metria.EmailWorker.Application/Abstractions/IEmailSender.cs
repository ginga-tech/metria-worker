using Metria.EmailWorker.Application.Models;

namespace Metria.EmailWorker.Application.Abstractions;

public interface IEmailSender
{
    Task SendDigestAsync(EmailSendPayload payload, CancellationToken cancellationToken);
}
