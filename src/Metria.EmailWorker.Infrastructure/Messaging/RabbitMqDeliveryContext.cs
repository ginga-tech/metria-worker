namespace Metria.EmailWorker.Infrastructure.Messaging;

public sealed record RabbitMqDeliveryContext(
    ulong DeliveryTag,
    byte[] Body,
    bool Redelivered,
    string RoutingKey);
