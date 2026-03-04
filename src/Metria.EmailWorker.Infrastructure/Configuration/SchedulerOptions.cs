namespace Metria.EmailWorker.Infrastructure.Configuration;

public sealed class SchedulerOptions
{
    public bool Enabled { get; set; }
    public string Cron { get; set; } = string.Empty;
    public string TimeZoneDefault { get; set; } = string.Empty;
}
