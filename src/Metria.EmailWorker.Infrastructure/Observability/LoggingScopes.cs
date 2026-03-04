using Microsoft.Extensions.Logging;

namespace Metria.EmailWorker.Infrastructure.Observability;

public static class LoggingScopes
{
    public static IDisposable BeginMessageScope(
        ILogger logger,
        Guid messageId,
        Guid correlationId,
        Guid userId)
    {
        var scope = new Dictionary<string, object>
        {
            ["messageId"] = messageId,
            ["correlationId"] = correlationId,
            ["userId"] = userId
        };

        return logger.BeginScope(scope)!;
    }
}
