using FluentAssertions;
using Moq;
using Metria.EmailWorker.Application.Abstractions;
using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Application.Exceptions;
using Metria.EmailWorker.Application.Models;
using Metria.EmailWorker.Application.UseCases;
using Metria.EmailWorker.Application.Validation;
using Polly;

namespace Metria.EmailWorker.Application.UnitTests;

public sealed class ProcessEmailDigestUseCaseTests
{
    private readonly EmailDigestMessageValidator _validator = new();

    [Fact]
    public async Task ExecuteAsync_WhenDispatchAlreadyExists_ShouldSkipDuplicate()
    {
        // Arrange
        var message = CreateMessage();
        var repository = new Mock<IEmailDispatchRepository>();
        var sender = new Mock<IEmailSender>();
        var renderer = new Mock<IEmailTemplateRenderer>();
        var clock = new Mock<IClock>();
        var sut = new ProcessEmailDigestUseCase(
            _validator,
            repository.Object,
            renderer.Object,
            sender.Object,
            clock.Object);

        repository
            .Setup(x => x.TryReserveAsync(message, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailDispatchReservation?)null);

        // Act
        var result = await sut.ExecuteAsync(message, CancellationToken.None);

        // Assert
        result.SkippedDuplicate.Should().BeTrue();
        result.Sent.Should().BeFalse();
        renderer.Verify(x => x.Render(It.IsAny<EmailDigestMessageV1>()), Times.Never);
        sender.Verify(x => x.SendDigestAsync(It.IsAny<EmailSendPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithTransientFailure_ShouldAllowRetryPolicyToSucceed()
    {
        // Arrange
        var message = CreateMessage();
        var reservation = new EmailDispatchReservation(Guid.NewGuid(), message.MessageId, true);
        var payload = new EmailSendPayload(
            message.MessageId,
            message.CorrelationId,
            message.UserId,
            message.Email,
            "subject",
            "<p>body</p>",
            "body");

        var repository = new Mock<IEmailDispatchRepository>();
        var sender = new Mock<IEmailSender>();
        var renderer = new Mock<IEmailTemplateRenderer>();
        var clock = new Mock<IClock>();

        repository
            .Setup(x => x.TryReserveAsync(message, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        renderer.Setup(x => x.Render(message)).Returns(payload);
        clock.SetupGet(x => x.UtcNow).Returns(DateTime.UtcNow);

        sender.SetupSequence(x => x.SendDigestAsync(payload, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TransientProcessingException("temporary"))
            .ThrowsAsync(new TransientProcessingException("temporary"))
            .Returns(Task.CompletedTask);

        var sut = new ProcessEmailDigestUseCase(
            _validator,
            repository.Object,
            renderer.Object,
            sender.Object,
            clock.Object);

        var retryPolicy = Policy
            .Handle<TransientProcessingException>()
            .WaitAndRetryAsync(2, _ => TimeSpan.Zero);

        // Act
        var result = await retryPolicy.ExecuteAsync(
            ct => sut.ExecuteAsync(message, ct),
            CancellationToken.None);

        // Assert
        result.Sent.Should().BeTrue();
        sender.Verify(x => x.SendDigestAsync(payload, It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailInvalid_ShouldThrowPermanentError()
    {
        // Arrange
        var message = CreateMessage();
        message = new EmailDigestMessageV1
        {
            MessageId = message.MessageId,
            CorrelationId = message.CorrelationId,
            UserId = message.UserId,
            Email = "invalid-email",
            FirstName = message.FirstName,
            Locale = message.Locale,
            TimeZone = message.TimeZone,
            PeriodStartUtc = message.PeriodStartUtc,
            PeriodEndUtc = message.PeriodEndUtc,
            TemplateKey = message.TemplateKey,
            MetadataMap = message.MetadataMap
        };
        var repository = new Mock<IEmailDispatchRepository>();
        var sender = new Mock<IEmailSender>();
        var renderer = new Mock<IEmailTemplateRenderer>();
        var clock = new Mock<IClock>();
        var sut = new ProcessEmailDigestUseCase(
            _validator,
            repository.Object,
            renderer.Object,
            sender.Object,
            clock.Object);

        // Act
        Func<Task> act = () => sut.ExecuteAsync(message, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<PermanentProcessingException>();
        repository.Verify(x => x.TryReserveAsync(It.IsAny<EmailDigestMessageV1>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateCorrelationIdInResult()
    {
        // Arrange
        var message = CreateMessage();
        var reservation = new EmailDispatchReservation(Guid.NewGuid(), message.MessageId, true);
        var payload = new EmailSendPayload(
            message.MessageId,
            message.CorrelationId,
            message.UserId,
            message.Email,
            "subject",
            "<p>body</p>",
            "body");

        var repository = new Mock<IEmailDispatchRepository>();
        var sender = new Mock<IEmailSender>();
        var renderer = new Mock<IEmailTemplateRenderer>();
        var clock = new Mock<IClock>();

        repository.Setup(x => x.TryReserveAsync(message, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        renderer.Setup(x => x.Render(message)).Returns(payload);
        sender.Setup(x => x.SendDigestAsync(payload, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        clock.SetupGet(x => x.UtcNow).Returns(DateTime.UtcNow);

        var sut = new ProcessEmailDigestUseCase(
            _validator,
            repository.Object,
            renderer.Object,
            sender.Object,
            clock.Object);

        // Act
        var result = await sut.ExecuteAsync(message, CancellationToken.None);

        // Assert
        result.CorrelationId.Should().Be(message.CorrelationId);
    }

    private static EmailDigestMessageV1 CreateMessage() =>
        new()
        {
            MessageId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = "Alex",
            Locale = "en-US",
            TimeZone = "UTC",
            PeriodStartUtc = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-7), DateTimeKind.Utc),
            PeriodEndUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            TemplateKey = "digest.v1",
            MetadataMap = new Dictionary<string, string>()
        };
}
