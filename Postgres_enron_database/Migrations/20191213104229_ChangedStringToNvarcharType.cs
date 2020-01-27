using Microsoft.EntityFrameworkCore.Migrations;

namespace Postgres_enron_database.Migrations
{
    public partial class ChangedStringToNvarcharType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "URL",
                table: "Emails",
                type: "varchar",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "EmailAddresses",
                type: "varchar",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "URL",
                table: "Emails",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "EmailAddresses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar");
        }
    }
}
