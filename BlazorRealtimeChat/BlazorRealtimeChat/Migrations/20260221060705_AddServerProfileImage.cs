using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorRealtimeChat.Migrations
{
    /// <inheritdoc />
    public partial class AddServerProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Servers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Servers");
        }
    }
}
