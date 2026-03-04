using RabbitMQ.Client;

namespace Metria.EmailWorker.Infrastructure.Messaging;

public static class RabbitMqTopologyInitializer
{
    public static void EnsureTopology(IModel channel, string queueName)
    {
        var dlxExchange = $"{queueName}.dlx";
        var dlqName = $"{queueName}.dlq";

        channel.ExchangeDeclare(dlxExchange, ExchangeType.Direct, durable: true, autoDelete: false);

        channel.QueueDeclare(dlqName, durable: true, exclusive: false, autoDelete: false);

        channel.QueueBind(dlqName, dlxExchange, routingKey: queueName);

        var args = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = dlxExchange,
            ["x-dead-letter-routing-key"] = queueName
        };

        channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: args);
    }
}
