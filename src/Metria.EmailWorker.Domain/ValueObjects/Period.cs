using Metria.EmailWorker.Domain.Exceptions;

namespace Metria.EmailWorker.Domain.ValueObjects;

public sealed record Period
{
    public DateTime StartUtc { get; }
    public DateTime EndUtc { get; }

    public Period(DateTime startUtc, DateTime endUtc)
    {
        if (startUtc.Kind != DateTimeKind.Utc || endUtc.Kind != DateTimeKind.Utc)
        {
            throw new DomainValidationException("Period dates must be UTC.");
        }

        if (startUtc >= endUtc)
        {
            throw new DomainValidationException("Period start must be earlier than period end.");
        }

        StartUtc = startUtc;
        EndUtc = endUtc;
    }
}
