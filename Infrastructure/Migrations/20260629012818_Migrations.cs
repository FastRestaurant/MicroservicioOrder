using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <summary>
/// Agrega la posición compartida del plano sin recrear el esquema existente.
/// </summary>
public partial class Migrations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "PositionX",
            table: "Tables",
            type: "decimal(5,2)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "PositionY",
            table: "Tables",
            type: "decimal(5,2)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PositionX", table: "Tables");
        migrationBuilder.DropColumn(name: "PositionY", table: "Tables");
    }
}
