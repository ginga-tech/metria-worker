namespace Metria.EmailWorker.Infrastructure.IntegrationTests.Fixtures;

[CollectionDefinition("postgres")]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
}

[CollectionDefinition("rabbitmq")]
public sealed class RabbitMqCollection : ICollectionFixture<RabbitMqFixture>
{
}
