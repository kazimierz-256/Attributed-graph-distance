using Microsoft.EntityFrameworkCore.Migrations;

namespace Postgres_enron_database.Migrations
{
    public partial class RemovedCollectionFromEmailObjectAndChangedFromFieldToKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DestinationEmails_Emails_EmailObjectId",
                table: "DestinationEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_Emails_EmailAddresses_FromId",
                table: "Emails");

            migrationBuilder.DropIndex(
                name: "IX_Emails_FromId",
                table: "Emails");

            migrationBuilder.DropIndex(
                name: "IX_DestinationEmails_EmailObjectId",
                table: "DestinationEmails");

            migrationBuilder.DropColumn(
                name: "EmailObjectId",
                table: "DestinationEmails");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmailObjectId",
                table: "DestinationEmails",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Emails_FromId",
                table: "Emails",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_DestinationEmails_EmailObjectId",
                table: "DestinationEmails",
                column: "EmailObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_DestinationEmails_Emails_EmailObjectId",
                table: "DestinationEmails",
                column: "EmailObjectId",
                principalTable: "Emails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_EmailAddresses_FromId",
                table: "Emails",
                column: "FromId",
                principalTable: "EmailAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
