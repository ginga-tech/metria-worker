using FluentAssertions;
using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Application.Exceptions;
using Metria.EmailWorker.Infrastructure.Email;

namespace Metria.EmailWorker.Application.UnitTests;

public sealed class EmailTemplateRendererTests
{
    [Fact]
    public void Render_WhenFirstNameMissing_ShouldUseFallbackGreeting()
    {
        // Arrange
        var renderer = new EmailTemplateRenderer();
        var message = CreateMessage(firstName: null, templateKey: "digest.v1");

        // Act
        var payload = renderer.Render(message);

        // Assert
        payload.HtmlBody.Should().Contain("Hello there");
        payload.TextBody.Should().Contain("Hello there");
    }

    [Fact]
    public void Render_WhenTemplateIsUnsupported_ShouldThrowPermanentException()
    {
        // Arrange
        var renderer = new EmailTemplateRenderer();
        var message = CreateMessage(firstName: "Maria", templateKey: "unsupported-key");

        // Act
        Action act = () => renderer.Render(message);

        // Assert
        act.Should().Throw<PermanentProcessingException>();
    }

    private static EmailDigestMessageV1 CreateMessage(string? firstName, string templateKey) =>
        new()
        {
            MessageId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = firstName,
            Locale = "en-US",
            TimeZone = "UTC",
            PeriodStartUtc = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-7), DateTimeKind.Utc),
            PeriodEndUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            TemplateKey = templateKey,
            MetadataMap = new Dictionary<string, string>()
        };
}
