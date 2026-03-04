namespace Metria.EmailWorker.Infrastructure.Messaging;

public enum RabbitMqMessageDisposition
{
    Ack = 0,
    NackToDlq = 1
}
