using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [Migration("20260615190000_NormalizeTableLocationAccents")]
    public partial class NormalizeTableLocationAccents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Tables] SET [Location] = N'Salón' WHERE [Location] = N'Salon'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Tables] SET [Location] = N'Salon' WHERE [Location] = N'Salón'");
        }
    }
}
