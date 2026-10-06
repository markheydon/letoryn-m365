using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Letoryn.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class UserIdentityEmailUnique : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_UserIdentities_Email",
            table: "UserIdentities");

        migrationBuilder.CreateIndex(
            name: "IX_UserIdentities_Email",
            table: "UserIdentities",
            column: "Email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_UserIdentities_Email",
            table: "UserIdentities");

        migrationBuilder.CreateIndex(
            name: "IX_UserIdentities_Email",
            table: "UserIdentities",
            column: "Email");
    }
}
