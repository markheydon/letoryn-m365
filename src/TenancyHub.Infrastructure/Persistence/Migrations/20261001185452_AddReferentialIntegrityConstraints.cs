using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenancyHub.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddReferentialIntegrityConstraints : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_PlatformOperatorAssignments_AgencyId",
            table: "PlatformOperatorAssignments",
            column: "AgencyId");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformOperatorAssignments_AssignedByUserIdentityId",
            table: "PlatformOperatorAssignments",
            column: "AssignedByUserIdentityId");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_AgencyId",
            table: "Notifications",
            column: "AgencyId");

        migrationBuilder.CreateIndex(
            name: "IX_AuditEvents_ActorUserIdentityId",
            table: "AuditEvents",
            column: "ActorUserIdentityId");

        migrationBuilder.CreateIndex(
            name: "IX_AuditEvents_TargetUserIdentityId",
            table: "AuditEvents",
            column: "TargetUserIdentityId");

        migrationBuilder.CreateIndex(
            name: "IX_AgencyMemberships_AgencyId_UserIdentityId",
            table: "AgencyMemberships",
            columns: new[] { "AgencyId", "UserIdentityId" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_AgencyMemberships_Agencies_AgencyId",
            table: "AgencyMemberships",
            column: "AgencyId",
            principalTable: "Agencies",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_AgencyMemberships_UserIdentities_UserIdentityId",
            table: "AgencyMemberships",
            column: "UserIdentityId",
            principalTable: "UserIdentities",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_AuditEvents_Agencies_AgencyId",
            table: "AuditEvents",
            column: "AgencyId",
            principalTable: "Agencies",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_AuditEvents_UserIdentities_ActorUserIdentityId",
            table: "AuditEvents",
            column: "ActorUserIdentityId",
            principalTable: "UserIdentities",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_AuditEvents_UserIdentities_TargetUserIdentityId",
            table: "AuditEvents",
            column: "TargetUserIdentityId",
            principalTable: "UserIdentities",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Notifications_Agencies_AgencyId",
            table: "Notifications",
            column: "AgencyId",
            principalTable: "Agencies",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Notifications_UserIdentities_UserIdentityId",
            table: "Notifications",
            column: "UserIdentityId",
            principalTable: "UserIdentities",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_PlatformOperatorAssignments_Agencies_AgencyId",
            table: "PlatformOperatorAssignments",
            column: "AgencyId",
            principalTable: "Agencies",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_PlatformOperatorAssignments_UserIdentities_AssignedByUserId~",
            table: "PlatformOperatorAssignments",
            column: "AssignedByUserIdentityId",
            principalTable: "UserIdentities",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_PlatformOperatorAssignments_UserIdentities_UserIdentityId",
            table: "PlatformOperatorAssignments",
            column: "UserIdentityId",
            principalTable: "UserIdentities",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_UserSessions_UserIdentities_UserIdentityId",
            table: "UserSessions",
            column: "UserIdentityId",
            principalTable: "UserIdentities",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AgencyMemberships_Agencies_AgencyId",
            table: "AgencyMemberships");

        migrationBuilder.DropForeignKey(
            name: "FK_AgencyMemberships_UserIdentities_UserIdentityId",
            table: "AgencyMemberships");

        migrationBuilder.DropForeignKey(
            name: "FK_AuditEvents_Agencies_AgencyId",
            table: "AuditEvents");

        migrationBuilder.DropForeignKey(
            name: "FK_AuditEvents_UserIdentities_ActorUserIdentityId",
            table: "AuditEvents");

        migrationBuilder.DropForeignKey(
            name: "FK_AuditEvents_UserIdentities_TargetUserIdentityId",
            table: "AuditEvents");

        migrationBuilder.DropForeignKey(
            name: "FK_Notifications_Agencies_AgencyId",
            table: "Notifications");

        migrationBuilder.DropForeignKey(
            name: "FK_Notifications_UserIdentities_UserIdentityId",
            table: "Notifications");

        migrationBuilder.DropForeignKey(
            name: "FK_PlatformOperatorAssignments_Agencies_AgencyId",
            table: "PlatformOperatorAssignments");

        migrationBuilder.DropForeignKey(
            name: "FK_PlatformOperatorAssignments_UserIdentities_AssignedByUserId~",
            table: "PlatformOperatorAssignments");

        migrationBuilder.DropForeignKey(
            name: "FK_PlatformOperatorAssignments_UserIdentities_UserIdentityId",
            table: "PlatformOperatorAssignments");

        migrationBuilder.DropForeignKey(
            name: "FK_UserSessions_UserIdentities_UserIdentityId",
            table: "UserSessions");

        migrationBuilder.DropIndex(
            name: "IX_PlatformOperatorAssignments_AgencyId",
            table: "PlatformOperatorAssignments");

        migrationBuilder.DropIndex(
            name: "IX_PlatformOperatorAssignments_AssignedByUserIdentityId",
            table: "PlatformOperatorAssignments");

        migrationBuilder.DropIndex(
            name: "IX_Notifications_AgencyId",
            table: "Notifications");

        migrationBuilder.DropIndex(
            name: "IX_AuditEvents_ActorUserIdentityId",
            table: "AuditEvents");

        migrationBuilder.DropIndex(
            name: "IX_AuditEvents_TargetUserIdentityId",
            table: "AuditEvents");

        migrationBuilder.DropIndex(
            name: "IX_AgencyMemberships_AgencyId_UserIdentityId",
            table: "AgencyMemberships");
    }
}
