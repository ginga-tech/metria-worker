namespace Metria.EmailWorker.Application.Abstractions;

public interface IEmailWorkerMetrics
{
    void IncrementProcessed();
    void IncrementSent();
    void IncrementFailed();
    void IncrementRetried();
}
