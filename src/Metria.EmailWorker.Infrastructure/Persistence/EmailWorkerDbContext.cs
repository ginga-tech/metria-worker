using Microsoft.EntityFrameworkCore;
using Metria.EmailWorker.Infrastructure.Persistence.Configurations;
using Metria.EmailWorker.Infrastructure.Persistence.Entities;

namespace Metria.EmailWorker.Infrastructure.Persistence;

public sealed class EmailWorkerDbContext : DbContext
{
    public EmailWorkerDbContext(DbContextOptions<EmailWorkerDbContext> options) : base(options)
    {
    }

    public DbSet<EmailDispatchLog> EmailDispatchLogs => Set<EmailDispatchLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EmailDispatchLogConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
