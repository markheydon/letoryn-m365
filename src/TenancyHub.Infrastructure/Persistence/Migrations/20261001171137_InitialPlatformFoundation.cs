using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenancyHub.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialPlatformFoundation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Agencies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                PrimaryContactEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                PrimaryContactPhone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                LifecycleStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                LastLifecycleChangeAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Agencies", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AgencyMemberships",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AgencyId = table.Column<Guid>(type: "uuid", nullable: false),
                UserIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                AgencyRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                InvitedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                InvitedRoleSnapshot = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AgencyMemberships", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AuditEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ActorUserIdentityId = table.Column<Guid>(type: "uuid", nullable: true),
                ActionType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                TargetUserIdentityId = table.Column<Guid>(type: "uuid", nullable: true),
                PayloadJson = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditEvents", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AgencyId = table.Column<Guid>(type: "uuid", nullable: false),
                UserIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Body = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                IsRead = table.Column<bool>(type: "boolean", nullable: false),
                ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PlatformOperatorAssignments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                AgencyId = table.Column<Guid>(type: "uuid", nullable: false),
                AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                AssignedByUserIdentityId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformOperatorAssignments", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "UserIdentities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EntraObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                IsPlatformOperator = table.Column<bool>(type: "boolean", nullable: false),
                LastUsedAgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserIdentities", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "UserSessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                LastActivityAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserSessions", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AgencyMemberships_AgencyId_Status",
            table: "AgencyMemberships",
            columns: new[] { "AgencyId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_AgencyMemberships_UserIdentityId_Status",
            table: "AgencyMemberships",
            columns: new[] { "UserIdentityId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditEvents_AgencyId_OccurredAt",
            table: "AuditEvents",
            columns: new[] { "AgencyId", "OccurredAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_UserIdentityId_AgencyId_CreatedAt",
            table: "Notifications",
            columns: new[] { "UserIdentityId", "AgencyId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_PlatformOperatorAssignments_UserIdentityId_AgencyId",
            table: "PlatformOperatorAssignments",
            columns: new[] { "UserIdentityId", "AgencyId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserIdentities_Email",
            table: "UserIdentities",
            column: "Email");

        migrationBuilder.CreateIndex(
            name: "IX_UserIdentities_EntraObjectId",
            table: "UserIdentities",
            column: "EntraObjectId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserSessions_UserIdentityId",
            table: "UserSessions",
            column: "UserIdentityId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Agencies");

        migrationBuilder.DropTable(
            name: "AgencyMemberships");

        migrationBuilder.DropTable(
            name: "AuditEvents");

        migrationBuilder.DropTable(
            name: "Notifications");

        migrationBuilder.DropTable(
            name: "PlatformOperatorAssignments");

        migrationBuilder.DropTable(
            name: "UserIdentities");

        migrationBuilder.DropTable(
            name: "UserSessions");
    }
}
