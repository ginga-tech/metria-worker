using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Metria.EmailWorker.Infrastructure.Configuration;
using Metria.EmailWorker.Infrastructure.IntegrationTests.Fixtures;
using Metria.EmailWorker.Infrastructure.Messaging;
using RabbitMQ.Client;

namespace Metria.EmailWorker.Infrastructure.IntegrationTests;

[Collection("rabbitmq")]
public sealed class RabbitMqConsumerIntegrationTests
{
    private readonly RabbitMqFixture _fixture;

    public RabbitMqConsumerIntegrationTests(RabbitMqFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires Docker daemon for RabbitMQ Testcontainers.")]
    public async Task Consumer_ShouldReceiveAndAckMessage()
    {
        // Arrange
        var queueName = $"email-digest-it-{Guid.NewGuid():N}";
        var options = Options.Create(new RabbitMqOptions
        {
            Host = _fixture.Host,
            Port = _fixture.Port,
            User = _fixture.Username,
            Password = _fixture.Password,
            QueueEmailDigest = queueName
        });

        var consumer = new RabbitMqConsumer(options, NullLogger<RabbitMqConsumer>.Instance);
        var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await consumer.StartConsumingAsync(
            (delivery, _) =>
            {
                tcs.TrySetResult(delivery.Body);
                return Task.FromResult(RabbitMqMessageDisposition.Ack);
            },
            cts.Token);

        Publish(queueName, """{"messageId":"00000000-0000-0000-0000-000000000001"}""");

        // Act
        var consumedPayload = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(30), cts.Token);

        // Assert
        consumedPayload.Should().NotBeNull();
        consumedPayload.Length.Should().BeGreaterThan(0);

        await consumer.StopAsync();
    }

    private void Publish(string queueName, string payload)
    {
        var factory = new ConnectionFactory
        {
            HostName = _fixture.Host,
            Port = _fixture.Port,
            UserName = _fixture.Username,
            Password = _fixture.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: null,
            body: System.Text.Encoding.UTF8.GetBytes(payload));
    }
}
