using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Metria.EmailWorker.Infrastructure.Persistence.Entities;

namespace Metria.EmailWorker.Infrastructure.Persistence.Configurations;

public sealed class EmailDispatchLogConfiguration : IEntityTypeConfiguration<EmailDispatchLog>
{
    public void Configure(EntityTypeBuilder<EmailDispatchLog> builder)
    {
        builder.ToTable("EmailDispatchLog");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.PeriodStartUtc).IsRequired();
        builder.Property(x => x.PeriodEndUtc).IsRequired();
        builder.Property(x => x.TemplateKey).IsRequired().HasMaxLength(200);
        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.CorrelationId).IsRequired();
        builder.Property(x => x.SentAtUtc).IsRequired(false);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(64);

        builder.HasIndex(
                x => new { x.UserId, x.PeriodStartUtc, x.PeriodEndUtc, x.TemplateKey })
            .IsUnique()
            .HasDatabaseName("IX_EmailDispatch_Unique");

        builder.HasIndex(x => x.MessageId)
            .HasDatabaseName("IX_EmailDispatch_MessageId");
    }
}
