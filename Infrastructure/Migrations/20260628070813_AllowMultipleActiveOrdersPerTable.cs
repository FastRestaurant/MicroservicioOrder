using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleActiveOrdersPerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'UX_Orders_Active_TableId'
                      AND object_id = OBJECT_ID(N'[Orders]')
                )
                DROP INDEX [UX_Orders_Active_TableId] ON [Orders];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_Orders_Active_TableId",
                table: "Orders",
                column: "TableId",
                unique: true,
                filter: "[StatusId] IN (1, 2, 3)");
        }
    }
}
