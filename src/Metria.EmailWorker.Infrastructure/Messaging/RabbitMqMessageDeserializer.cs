using System.Text.Json;
using Metria.EmailWorker.Application.Contracts.Messages;
using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Application.Exceptions;

namespace Metria.EmailWorker.Infrastructure.Messaging;

public sealed class RabbitMqMessageDeserializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public EmailDigestMessageV1 Deserialize(byte[] body)
    {
        try
        {
            var json = JsonDocument.Parse(body);
            var root = json.RootElement;

            if (root.TryGetProperty("version", out var versionElement))
            {
                var version = versionElement.GetString();
                var effectiveVersion = string.IsNullOrWhiteSpace(version)
                    ? MessageContractVersion.V1
                    : version.Trim().ToLowerInvariant();

                if (!root.TryGetProperty("payload", out var payloadElement))
                {
                    throw new PermanentProcessingException("Envelope payload is missing.");
                }

                return effectiveVersion switch
                {
                    MessageContractVersion.V1 => payloadElement.Deserialize<EmailDigestMessageV1>(SerializerOptions)
                                                 ?? throw new PermanentProcessingException("Invalid v1 payload."),
                    _ => throw new PermanentProcessingException($"Unsupported message version '{effectiveVersion}'.")
                };
            }

            return root.Deserialize<EmailDigestMessageV1>(SerializerOptions)
                   ?? throw new PermanentProcessingException("Invalid v1 message body.");
        }
        catch (PermanentProcessingException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new PermanentProcessingException("Message JSON is invalid.", ex);
        }
    }
}
