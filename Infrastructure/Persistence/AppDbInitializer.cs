using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public static class AppDbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedTablesAsync(context, cancellationToken);
        await SeedFacturasAsync(context, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTablesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var existingTables = await context.Tables
            .ToDictionaryAsync(table => table.Number, cancellationToken);

        foreach (var seed in BuildTables())
        {
            if (existingTables.TryGetValue(seed.Number, out var existing))
                continue;

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

    private static async Task SeedFacturasAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Facturas.AnyAsync(cancellationToken))
            return;

        var random = new Random();

        var productos = new List<(string name, decimal price)>
    {
        ("Hamburguesa", 10000),
        ("Pizza Muzzarella", 12000),
        ("Papas fritas", 6000),
        ("Milanesa Napolitana", 14000),
        ("Coca Cola", 3000),
        ("Empanadas", 1500),
        ("Lomito", 15000),
        ("Agua", 2000),
        ("Cerveza", 3500),
        ("Helado", 4000),
        ("Ravioles", 11000),
        ("Asado", 20000),
        ("Tostado", 7000)
    };

        var facturas = new List<Factura>();

        var startDate = DateTime.Now.AddYears(-1);

        for (int i = 0; i < 100; i++)
        {
            // 📅 distribución realista en 1 año
            var date = startDate.AddDays(random.Next(0, 365))
                                .AddHours(random.Next(12, 23))
                                .AddMinutes(random.Next(0, 60));

            var factura = new Factura
            {
                TableName = random.Next(1, 15).ToString(),
                Date = date,
                IsPaid = random.NextDouble() > 0.35, // 65% pagadas
                Details = new List<FacturaDetail>()
            };

            int items = random.Next(1, 5); // 1 a 4 platos por factura
            decimal total = 0;

            var usedProducts = new HashSet<int>();

            for (int j = 0; j < items; j++)
            {
                int index;

                // evitar repetir plato en la misma factura
                do
                {
                    index = random.Next(productos.Count);
                }
                while (usedProducts.Contains(index));

                usedProducts.Add(index);

                var p = productos[index];
                var qty = random.Next(1, 3);

                factura.Details.Add(new FacturaDetail
                {
                    ProductName = p.name,
                    Quantity = qty,
                    Price = p.price
                });

                total += p.price * qty;
            }

            factura.Total = total;

            facturas.Add(factura);
        }

        await context.Facturas.AddRangeAsync(facturas, cancellationToken);
    }
}
