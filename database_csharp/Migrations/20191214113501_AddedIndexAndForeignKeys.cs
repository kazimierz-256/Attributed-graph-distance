﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace database_csharp.Migrations
{
    public partial class AddedIndexAndForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Emails_URL",
                table: "Emails",
                column: "URL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emails_URL",
                table: "Emails");
        }
    }
}
