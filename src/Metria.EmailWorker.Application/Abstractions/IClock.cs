namespace Metria.EmailWorker.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
