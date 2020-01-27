using Microsoft.EntityFrameworkCore.Migrations;

namespace Postgres_enron_database.Migrations
{
    public partial class ChangedFromType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emails_Emails_FromId",
                table: "Emails");

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_EmailAddresses_FromId",
                table: "Emails",
                column: "FromId",
                principalTable: "EmailAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emails_EmailAddresses_FromId",
                table: "Emails");

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_Emails_FromId",
                table: "Emails",
                column: "FromId",
                principalTable: "Emails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
