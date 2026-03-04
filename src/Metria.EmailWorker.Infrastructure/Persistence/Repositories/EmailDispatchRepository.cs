using Microsoft.EntityFrameworkCore;
using Metria.EmailWorker.Application.Abstractions;
using Metria.EmailWorker.Application.Contracts.Messages.V1;
using Metria.EmailWorker.Application.Models;
using Metria.EmailWorker.Domain.Enums;
using Metria.EmailWorker.Infrastructure.Persistence.Entities;

namespace Metria.EmailWorker.Infrastructure.Persistence.Repositories;

public sealed class EmailDispatchRepository : IEmailDispatchRepository
{
    private readonly EmailWorkerDbContext _dbContext;

    public EmailDispatchRepository(EmailWorkerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EmailDispatchReservation?> TryReserveAsync(
        EmailDigestMessageV1 message,
        CancellationToken cancellationToken)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var id = Guid.NewGuid();

        var insertedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO "EmailDispatchLog"
                 ("Id", "UserId", "PeriodStartUtc", "PeriodEndUtc", "TemplateKey", "MessageId", "CorrelationId", "SentAtUtc", "Status")
             VALUES
                 ({id}, {message.UserId}, {message.PeriodStartUtc}, {message.PeriodEndUtc}, {message.TemplateKey}, {message.MessageId}, {message.CorrelationId}, {null}, {EmailDispatchStatus.Processing.ToString()})
             ON CONFLICT ("UserId", "PeriodStartUtc", "PeriodEndUtc", "TemplateKey")
             DO NOTHING
             """,
            cancellationToken);

        if (insertedRows == 1)
        {
            await tx.CommitAsync(cancellationToken);
            return new EmailDispatchReservation(id, message.MessageId, true);
        }

        var existing = await _dbContext.EmailDispatchLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.UserId == message.UserId &&
                     x.PeriodStartUtc == message.PeriodStartUtc &&
                     x.PeriodEndUtc == message.PeriodEndUtc &&
                     x.TemplateKey == message.TemplateKey,
                cancellationToken);

        await tx.CommitAsync(cancellationToken);

        if (existing is null)
        {
            return null;
        }

        var sameMessage = existing.MessageId == message.MessageId;
        var canReuse = sameMessage &&
                       string.Equals(existing.Status, EmailDispatchStatus.TransientFailed.ToString(), StringComparison.OrdinalIgnoreCase);

        if (!canReuse)
        {
            return null;
        }

        return new EmailDispatchReservation(existing.Id, existing.MessageId, false);
    }

    public async Task MarkSentAsync(Guid dispatchLogId, DateTime sentAtUtc, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.EmailDispatchLogs
            .SingleOrDefaultAsync(x => x.Id == dispatchLogId, cancellationToken);

        if (entity is null)
        {
            return;
        }

        entity.Status = EmailDispatchStatus.Sent.ToString();
        entity.SentAtUtc = sentAtUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(Guid dispatchLogId, string status, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.EmailDispatchLogs
            .SingleOrDefaultAsync(x => x.Id == dispatchLogId, cancellationToken);

        if (entity is null)
        {
            return;
        }

        entity.Status = status;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
