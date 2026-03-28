using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedTeamsUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeamsMeetingUrl",
                table: "TeamsCalls",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamsMeetingUrl",
                table: "TeamsCalls");
        }
    }
}
