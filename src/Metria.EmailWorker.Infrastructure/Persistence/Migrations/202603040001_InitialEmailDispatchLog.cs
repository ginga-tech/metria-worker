using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metria.EmailWorker.Infrastructure.Persistence.Migrations
{
    public partial class InitialEmailDispatchLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailDispatchLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailDispatchLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailDispatch_MessageId",
                table: "EmailDispatchLog",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailDispatch_Unique",
                table: "EmailDispatchLog",
                columns: new[] { "UserId", "PeriodStartUtc", "PeriodEndUtc", "TemplateKey" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailDispatchLog");
        }
    }
}
