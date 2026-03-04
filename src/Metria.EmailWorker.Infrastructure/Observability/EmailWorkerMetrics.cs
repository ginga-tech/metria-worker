using System.Diagnostics.Metrics;
using Metria.EmailWorker.Application.Abstractions;

namespace Metria.EmailWorker.Infrastructure.Observability;

public sealed class EmailWorkerMetrics : IEmailWorkerMetrics, IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _processed;
    private readonly Counter<long> _sent;
    private readonly Counter<long> _failed;
    private readonly Counter<long> _retried;

    public EmailWorkerMetrics()
    {
        _meter = new Meter("Metria.EmailWorker", "1.0.0");
        _processed = _meter.CreateCounter<long>("emails_processed_total");
        _sent = _meter.CreateCounter<long>("emails_sent_total");
        _failed = _meter.CreateCounter<long>("emails_failed_total");
        _retried = _meter.CreateCounter<long>("emails_retried_total");
    }

    public void IncrementProcessed() => _processed.Add(1);
    public void IncrementSent() => _sent.Add(1);
    public void IncrementFailed() => _failed.Add(1);
    public void IncrementRetried() => _retried.Add(1);

    public void Dispose()
    {
        _meter.Dispose();
    }
}
