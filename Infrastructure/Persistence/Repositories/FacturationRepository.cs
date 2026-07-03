using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using OrderService.Infrastructure.Persistence;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class FacturationRepository : IFacturationRepository
    {
        private readonly AppDbContext _context;

        public FacturationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Factura> Items, int Total)> GetFacturasAsync(
            int pageNumber,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            PaymentFilter filter)
        {
            var query = _context.Facturas
                .Include(x => x.Details)
                .AsQueryable();

            // =========================
            // FILTRO FECHA
            // =========================
            if (fromDate.HasValue)
                query = query.Where(x => x.Date.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.Date.Date <= toDate.Value.Date);

            // =========================
            // FILTRO PAGO
            // =========================
            query = filter switch
            {
                PaymentFilter.Paid => query.Where(x => x.IsPaid),
                PaymentFilter.Pending => query.Where(x => !x.IsPaid),
                _ => query
            };

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }


        public async Task<bool> MarkAsPaidAsync(int facturaId)
        {
            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f => f.Id == facturaId);

            if (factura == null)
                return false;

            if (factura.IsPaid)
                return true;

            factura.IsPaid = true;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<Factura> CreateAsync(Factura factura)
        {
            if (factura == null)
                throw new ArgumentNullException(nameof(factura));

            if (factura.Details == null || factura.Details.Count == 0)
                throw new InvalidOperationException("La factura debe tener al menos un detalle.");

            factura.Date = DateTime.UtcNow;

            factura.IsPaid = false;

            factura.Total = factura.Details.Sum(d => d.Price * d.Quantity);


            foreach (var detail in factura.Details)
            {
                detail.FacturaId = factura.Id; 
            }

            await _context.Facturas.AddAsync(factura);
            await _context.SaveChangesAsync();

            return factura;
        }

        public async Task AddAsync(Factura factura, CancellationToken ct = default)
        {
            await _context.Facturas.AddAsync(factura, ct);
        }



    }
}
