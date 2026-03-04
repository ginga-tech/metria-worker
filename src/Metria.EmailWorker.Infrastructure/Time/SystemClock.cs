using Metria.EmailWorker.Application.Abstractions;

namespace Metria.EmailWorker.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
