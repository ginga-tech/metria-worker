using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Metria.EmailWorker.Infrastructure.Persistence;

#nullable disable

namespace Metria.EmailWorker.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(EmailWorkerDbContext))]
    partial class EmailWorkerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("Metria.EmailWorker.Infrastructure.Persistence.Entities.EmailDispatchLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedNever()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("MessageId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("SentAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("PeriodEndUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("PeriodStartUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("TemplateKey")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("MessageId")
                        .HasDatabaseName("IX_EmailDispatch_MessageId");

                    b.HasIndex("UserId", "PeriodStartUtc", "PeriodEndUtc", "TemplateKey")
                        .IsUnique()
                        .HasDatabaseName("IX_EmailDispatch_Unique");

                    b.ToTable("EmailDispatchLog");
                });
#pragma warning restore 612, 618
        }
    }
}
