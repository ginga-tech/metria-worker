using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Infrastructure.IntegrationTests.Fixtures;
using Metria.EmailWorker.Infrastructure.Persistence;
using Metria.EmailWorker.Infrastructure.Persistence.Repositories;

namespace Metria.EmailWorker.Infrastructure.IntegrationTests;

[Collection("postgres")]
public sealed class EmailDispatchRepositoryIntegrationTests
{
    private readonly PostgreSqlFixture _fixture;

    public EmailDispatchRepositoryIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires Docker daemon for PostgreSQL Testcontainers.")]
    public async Task TryReserveAsync_WithParallelDuplicates_ShouldReserveOnlyOnce()
    {
        // Arrange
        await using var setupContext = CreateDbContext();
        await setupContext.Database.MigrateAsync();
        await setupContext.Database.ExecuteSqlRawAsync("""TRUNCATE TABLE "EmailDispatchLog" """);

        var userId = Guid.NewGuid();
        var periodStart = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-7), DateTimeKind.Utc);
        var periodEnd = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        var templateKey = "digest.v1";

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(async _ =>
            {
                await using var dbContext = CreateDbContext();
                var repository = new EmailDispatchRepository(dbContext);
                var message = CreateMessage(userId, periodStart, periodEnd, templateKey, Guid.NewGuid());
                return await repository.TryReserveAsync(message, CancellationToken.None);
            });

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Count(x => x is not null).Should().Be(1);
    }

    private EmailWorkerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EmailWorkerDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        return new EmailWorkerDbContext(options);
    }

    private static EmailDigestMessageV1 CreateMessage(
        Guid userId,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        string templateKey,
        Guid messageId) =>
        new()
        {
            MessageId = messageId,
            CorrelationId = Guid.NewGuid(),
            UserId = userId,
            Email = "user@example.com",
            FirstName = "Alex",
            Locale = "en-US",
            TimeZone = "UTC",
            PeriodStartUtc = periodStartUtc,
            PeriodEndUtc = periodEndUtc,
            TemplateKey = templateKey,
            MetadataMap = new Dictionary<string, string>()
        };
}
