using System.Diagnostics;
using Microsoft.Extensions.Options;
using Metria.EmailWorker.Application.Abstractions;
using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Application.Exceptions;
using Metria.EmailWorker.Application.Models;
using Metria.EmailWorker.Application.UseCases;
using Metria.EmailWorker.Infrastructure.Configuration;
using Metria.EmailWorker.Infrastructure.Messaging;
using Metria.EmailWorker.Infrastructure.Observability;
using Polly;

namespace Metria.EmailWorker.Processor.HostedServices;

public sealed class EmailDigestConsumerHostedService : BackgroundService
{
    private readonly RabbitMqConsumer _consumer;
    private readonly RabbitMqMessageDeserializer _deserializer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RetryOptions _retryOptions;
    private readonly IEmailWorkerMetrics _metrics;
    private readonly ILogger<EmailDigestConsumerHostedService> _logger;

    public EmailDigestConsumerHostedService(
        RabbitMqConsumer consumer,
        RabbitMqMessageDeserializer deserializer,
        IServiceScopeFactory scopeFactory,
        IOptions<RetryOptions> retryOptions,
        IEmailWorkerMetrics metrics,
        ILogger<EmailDigestConsumerHostedService> logger)
    {
        _consumer = consumer;
        _deserializer = deserializer;
        _scopeFactory = scopeFactory;
        _retryOptions = retryOptions.Value;
        _metrics = metrics;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartConsumingAsync(HandleDeliveryAsync, stoppingToken);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumer.StopAsync();
        await base.StopAsync(cancellationToken);
    }

    private async Task<RabbitMqMessageDisposition> HandleDeliveryAsync(
        RabbitMqDeliveryContext deliveryContext,
        CancellationToken cancellationToken)
    {
        EmailDigestMessageV1? message = null;
        IDisposable? logScope = null;
        var retryCount = 0;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            message = _deserializer.Deserialize(deliveryContext.Body);
            logScope = LoggingScopes.BeginMessageScope(
                _logger,
                message.MessageId,
                message.CorrelationId,
                message.UserId);

            var policy = Policy<ProcessEmailDigestResult>
                .Handle<TransientProcessingException>()
                .WaitAndRetryAsync(
                    Math.Max(_retryOptions.MaxAttempts - 1, 0),
                    attempt => CalculateBackoff(attempt),
                    (outcome, delay, attempt, _) =>
                    {
                        retryCount = attempt;
                        _metrics.IncrementRetried();

                        _logger.LogWarning(
                            outcome.Exception,
                            "Transient failure while processing email digest. retryCount={retryCount} delayMs={delayMs}",
                            retryCount,
                            delay.TotalMilliseconds);
                    });

            var result = await policy.ExecuteAsync(
                async ct =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var useCase = scope.ServiceProvider.GetRequiredService<ProcessEmailDigestUseCase>();
                    return await useCase.ExecuteAsync(message, ct);
                },
                cancellationToken);

            if (result.Sent)
            {
                _metrics.IncrementSent();
            }

            _logger.LogInformation(
                "Message processed successfully. sent={sent} skippedDuplicate={skippedDuplicate} durationMs={durationMs} retryCount={retryCount}",
                result.Sent,
                result.SkippedDuplicate,
                stopwatch.ElapsedMilliseconds,
                retryCount);

            return RabbitMqMessageDisposition.Ack;
        }
        catch (PermanentProcessingException ex)
        {
            var scope = logScope ?? LoggingScopes.BeginMessageScope(_logger, Guid.Empty, Guid.Empty, Guid.Empty);
            using (scope)
            {
                _logger.LogError(
                    ex,
                    "Permanent failure. Routing message to DLQ. durationMs={durationMs} retryCount={retryCount}",
                    stopwatch.ElapsedMilliseconds,
                    retryCount);
            }

            _metrics.IncrementFailed();
            return RabbitMqMessageDisposition.NackToDlq;
        }
        catch (TransientProcessingException ex)
        {
            var scope = logScope ?? LoggingScopes.BeginMessageScope(_logger, Guid.Empty, Guid.Empty, Guid.Empty);
            using (scope)
            {
                _logger.LogError(
                    ex,
                    "Transient retries exhausted. Routing message to DLQ. durationMs={durationMs} retryCount={retryCount}",
                    stopwatch.ElapsedMilliseconds,
                    retryCount);
            }

            _metrics.IncrementFailed();
            return RabbitMqMessageDisposition.NackToDlq;
        }
        catch (Exception ex)
        {
            var scope = logScope ?? LoggingScopes.BeginMessageScope(_logger, Guid.Empty, Guid.Empty, Guid.Empty);
            using (scope)
            {
                _logger.LogError(
                    ex,
                    "Unexpected failure. Routing message to DLQ. durationMs={durationMs} retryCount={retryCount}",
                    stopwatch.ElapsedMilliseconds,
                    retryCount);
            }

            _metrics.IncrementFailed();
            return RabbitMqMessageDisposition.NackToDlq;
        }
        finally
        {
            logScope?.Dispose();
            _metrics.IncrementProcessed();
            stopwatch.Stop();
        }
    }

    private TimeSpan CalculateBackoff(int attempt)
    {
        var exponent = Math.Pow(2, attempt);
        var delaySeconds = _retryOptions.BaseDelaySeconds * exponent;
        var cappedSeconds = Math.Min(delaySeconds, _retryOptions.MaxDelaySeconds);
        return TimeSpan.FromSeconds(cappedSeconds);
    }
}
