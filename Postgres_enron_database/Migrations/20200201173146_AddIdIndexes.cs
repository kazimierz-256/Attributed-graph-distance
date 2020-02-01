using Microsoft.EntityFrameworkCore.Migrations;

namespace Postgres_enron_database.Migrations
{
    public partial class AddIdIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Emails_Id",
                table: "Emails",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emails_Id",
                table: "Emails");
        }
    }
}
