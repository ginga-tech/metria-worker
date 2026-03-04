using Testcontainers.RabbitMq;

namespace Metria.EmailWorker.Infrastructure.IntegrationTests.Fixtures;

public sealed class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder()
        .WithImage("rabbitmq:3.13-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    private Uri ConnectionUri => new(_container.GetConnectionString());

    public string Host => ConnectionUri.Host;
    public int Port => ConnectionUri.Port;
    public string Username => ConnectionUri.UserInfo.Split(':')[0];
    public string Password => ConnectionUri.UserInfo.Split(':').Length > 1
        ? ConnectionUri.UserInfo.Split(':')[1]
        : string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
