using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Application.Models;

namespace Metria.EmailWorker.Application.Abstractions;

public interface IEmailTemplateRenderer
{
    EmailSendPayload Render(EmailDigestMessageV1 message);
}
