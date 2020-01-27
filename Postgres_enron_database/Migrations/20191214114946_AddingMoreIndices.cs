using Microsoft.EntityFrameworkCore.Migrations;

namespace Postgres_enron_database.Migrations
{
    public partial class AddingMoreIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Emails_SendDate",
                table: "Emails",
                column: "SendDate");

            migrationBuilder.CreateIndex(
                name: "IX_DestinationEmails_SendType",
                table: "DestinationEmails",
                column: "SendType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emails_SendDate",
                table: "Emails");

            migrationBuilder.DropIndex(
                name: "IX_DestinationEmails_SendType",
                table: "DestinationEmails");
        }
    }
}
