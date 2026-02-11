using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorRealtimeChat.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerToServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Servers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Servers_OwnerId",
                table: "Servers",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_Users_OwnerId",
                table: "Servers",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_Users_OwnerId",
                table: "Servers");

            migrationBuilder.DropIndex(
                name: "IX_Servers_OwnerId",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Servers");
        }
    }
}
