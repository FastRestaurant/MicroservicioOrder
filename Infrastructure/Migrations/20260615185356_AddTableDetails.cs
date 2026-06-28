using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTableDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Tables",
                newName: "IsEnabled");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Tables",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Salón");

            migrationBuilder.AddColumn<int>(
                name: "SeatCount",
                table: "Tables",
                type: "int",
                nullable: false,
                defaultValue: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "SeatCount",
                table: "Tables");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "Tables",
                newName: "Status");
        }
    }
}
