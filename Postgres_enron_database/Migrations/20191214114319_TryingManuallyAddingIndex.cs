using Microsoft.EntityFrameworkCore.Migrations;

namespace Postgres_enron_database.Migrations
{
    public partial class TryingManuallyAddingIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Emails_FromId",
                table: "Emails",
                column: "FromId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emails_FromId",
                table: "Emails");
        }
    }
}
