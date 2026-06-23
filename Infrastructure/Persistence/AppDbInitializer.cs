using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public static class AppDbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedTablesAsync(context, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTablesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var existingTables = await context.Tables
            .ToDictionaryAsync(table => table.Number, cancellationToken);

        foreach (var seed in BuildTables())
        {
            if (existingTables.TryGetValue(seed.Number, out var existing))
            {
                existing.Update(seed.Number, seed.SeatCount, seed.Location, seed.IsEnabled);
                continue;
            }

            await context.Tables.AddAsync(
                Table.Create(seed.Number, seed.SeatCount, seed.Location, seed.IsEnabled),
                cancellationToken);
        }
    }

    private static List<TableSeed> BuildTables() => new()
    {
        new("1", 2, "Salón", true),
        new("2", 2, "Salón", true),
        new("3", 4, "Salón", true),
        new("4", 4, "Salón", true),
        new("5", 4, "Salón", true),
        new("6", 6, "Salón", true),
        new("7", 2, "Patio", true),
        new("8", 4, "Patio", true),
        new("9", 4, "Patio", true),
        new("10", 6, "Patio", true),
        new("11", 2, "Barra", true),
        new("12", 2, "Barra", true),
        new("13", 8, "Salón", true),
        new("14", 4, "Terraza", true)
    };

    private sealed record TableSeed(string Number, int SeatCount, string Location, bool IsEnabled);
}
