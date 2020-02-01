using Microsoft.EntityFrameworkCore.Migrations;

namespace Postgres_enron_database.Migrations
{
    public partial class AddBelongsToEnronIndexAndEmailAddressIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EmailAddresses_BelongsToEnron",
                table: "EmailAddresses",
                column: "BelongsToEnron");

            migrationBuilder.CreateIndex(
                name: "IX_EmailAddresses_Id",
                table: "EmailAddresses",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailAddresses_BelongsToEnron",
                table: "EmailAddresses");

            migrationBuilder.DropIndex(
                name: "IX_EmailAddresses_Id",
                table: "EmailAddresses");
        }
    }
}
