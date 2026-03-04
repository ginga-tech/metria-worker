namespace Metria.EmailWorker.Application.Contracts.Messages;

public sealed class EmailDigestEnvelope
{
    public string Version { get; init; } = MessageContractVersion.V1;
    public object? Payload { get; init; }
}
