using Microsoft.EntityFrameworkCore.Migrations;

namespace database_csharp.Migrations
{
    public partial class AddedEmailIsFromEnron : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BelongsToEnron",
                table: "EmailAddresses",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BelongsToEnron",
                table: "EmailAddresses");
        }
    }
}
