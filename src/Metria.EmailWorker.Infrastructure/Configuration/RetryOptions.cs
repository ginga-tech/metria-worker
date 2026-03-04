namespace Metria.EmailWorker.Infrastructure.Configuration;

public sealed class RetryOptions
{
    public int MaxAttempts { get; set; } = 5;
    public int BaseDelaySeconds { get; set; } = 2;
    public int MaxDelaySeconds { get; set; } = 60;
}
