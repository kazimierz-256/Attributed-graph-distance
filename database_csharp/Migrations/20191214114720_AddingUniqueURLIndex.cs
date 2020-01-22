﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace database_csharp.Migrations
{
    public partial class AddingUniqueURLIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emails_URL",
                table: "Emails");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_URL",
                table: "Emails",
                column: "URL",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emails_URL",
                table: "Emails");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_URL",
                table: "Emails",
                column: "URL");
        }
    }
}
