using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Metria.EmailWorker.Application.Abstractions;
using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Application.Models;
using Metria.EmailWorker.Application.UseCases;
using Metria.EmailWorker.Application.Validation;
using Metria.EmailWorker.Infrastructure.IntegrationTests.Fixtures;
using Metria.EmailWorker.Infrastructure.Persistence;
using Metria.EmailWorker.Infrastructure.Persistence.Repositories;
using Metria.EmailWorker.Infrastructure.Time;

namespace Metria.EmailWorker.Infrastructure.IntegrationTests;

[Collection("postgres")]
public sealed class EmailAdapterIntegrationTests
{
    private readonly PostgreSqlFixture _fixture;

    public EmailAdapterIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires Docker daemon for PostgreSQL Testcontainers.")]
    public async Task DuplicateDispatch_ShouldSendEmailOnlyOnce()
    {
        // Arrange
        await using var setupContext = CreateDbContext();
        await setupContext.Database.MigrateAsync();
        await setupContext.Database.ExecuteSqlRawAsync("""TRUNCATE TABLE "EmailDispatchLog" """);

        var periodStart = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-7), DateTimeKind.Utc);
        var periodEnd = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        var userId = Guid.NewGuid();
        var message1 = CreateMessage(Guid.NewGuid(), userId, periodStart, periodEnd, "digest.v1");
        var message2 = CreateMessage(Guid.NewGuid(), userId, periodStart, periodEnd, "digest.v1");
        var payload = new EmailSendPayload(
            message1.MessageId,
            message1.CorrelationId,
            userId,
            message1.Email,
            "subject",
            "<p>body</p>",
            "body");

        var sender = new Mock<IEmailSender>();
        sender.Setup(x => x.SendDigestAsync(It.IsAny<EmailSendPayload>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var renderer = new Mock<IEmailTemplateRenderer>();
        renderer.Setup(x => x.Render(It.IsAny<EmailDigestMessageV1>())).Returns(payload);

        await using var dbContext = CreateDbContext();
        var repository = new EmailDispatchRepository(dbContext);
        var useCase = new ProcessEmailDigestUseCase(
            new EmailDigestMessageValidator(),
            repository,
            renderer.Object,
            sender.Object,
            new SystemClock());

        // Act
        var firstResult = await useCase.ExecuteAsync(message1, CancellationToken.None);
        var secondResult = await useCase.ExecuteAsync(message2, CancellationToken.None);

        // Assert
        firstResult.Sent.Should().BeTrue();
        secondResult.SkippedDuplicate.Should().BeTrue();
        sender.Verify(
            x => x.SendDigestAsync(It.IsAny<EmailSendPayload>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private EmailWorkerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EmailWorkerDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        return new EmailWorkerDbContext(options);
    }

    private static EmailDigestMessageV1 CreateMessage(
        Guid messageId,
        Guid userId,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        string templateKey) =>
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
