namespace Metria.EmailWorker.Infrastructure.Persistence.Entities;

public sealed class EmailDispatchLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public Guid MessageId { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
}
