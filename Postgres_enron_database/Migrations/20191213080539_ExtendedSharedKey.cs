using Microsoft.EntityFrameworkCore.Migrations;

namespace Postgres_enron_database.Migrations
{
    public partial class ExtendedSharedKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DestinationEmails",
                table: "DestinationEmails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DestinationEmails",
                table: "DestinationEmails",
                columns: new[] { "EmailId", "EmailAddressId", "SendType" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DestinationEmails",
                table: "DestinationEmails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DestinationEmails",
                table: "DestinationEmails",
                columns: new[] { "EmailId", "EmailAddressId" });
        }
    }
}
