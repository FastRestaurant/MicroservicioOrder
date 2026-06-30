using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [Migration("20260615191500_AddActiveOrderUniqueIndex")]
    public partial class AddActiveOrderUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_Orders_Active_TableId",
                table: "Orders",
                column: "TableId",
                unique: true,
                filter: "[StatusId] IN (1, 2, 3)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Orders_Active_TableId",
                table: "Orders");
        }
    }
}
