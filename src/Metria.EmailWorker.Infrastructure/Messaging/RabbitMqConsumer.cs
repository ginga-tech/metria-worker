using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Metria.EmailWorker.Infrastructure.Configuration;
using Metria.EmailWorker.Infrastructure.Observability;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Metria.EmailWorker.Infrastructure.Messaging;

public sealed class RabbitMqConsumer : IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private string? _consumerTag;

    public RabbitMqConsumer(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConsumer> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task StartConsumingAsync(
        Func<RabbitMqDeliveryContext, CancellationToken, Task<RabbitMqMessageDisposition>> messageHandler,
        CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.User,
            Password = _options.Password,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.BasicQos(0, prefetchCount: 10, global: false);

        RabbitMqTopologyInitializer.EnsureTopology(_channel, _options.QueueEmailDigest);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var context = new RabbitMqDeliveryContext(
                ea.DeliveryTag,
                ea.Body.ToArray(),
                ea.Redelivered,
                ea.RoutingKey);

            var disposition = await messageHandler(context, cancellationToken);

            if (_channel is null || !_channel.IsOpen)
            {
                return;
            }

            switch (disposition)
            {
                case RabbitMqMessageDisposition.Ack:
                    _channel.BasicAck(context.DeliveryTag, multiple: false);
                    break;
                case RabbitMqMessageDisposition.NackToDlq:
                    _channel.BasicNack(context.DeliveryTag, multiple: false, requeue: false);
                    break;
                default:
                    _channel.BasicNack(context.DeliveryTag, multiple: false, requeue: false);
                    break;
            }
        };

        _consumerTag = _channel.BasicConsume(
            queue: _options.QueueEmailDigest,
            autoAck: false,
            consumer: consumer);

        using (LoggingScopes.BeginMessageScope(_logger, Guid.Empty, Guid.Empty, Guid.Empty))
        {
            _logger.LogInformation(
                "RabbitMQ consumer started for queue {queueName}.",
                _options.QueueEmailDigest);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (_channel?.IsOpen == true && !string.IsNullOrWhiteSpace(_consumerTag))
        {
            _channel.BasicCancel(_consumerTag);
        }

        _channel?.Close();
        _connection?.Close();

        _channel?.Dispose();
        _connection?.Dispose();

        _channel = null;
        _connection = null;

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
