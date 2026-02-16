using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroSolutions.Alerts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToAlert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecipientEmail",
                table: "Alerts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientEmail",
                table: "Alerts");
        }
    }
}
