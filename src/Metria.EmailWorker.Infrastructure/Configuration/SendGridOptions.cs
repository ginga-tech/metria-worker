namespace Metria.EmailWorker.Infrastructure.Configuration;

public sealed class SendGridOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
