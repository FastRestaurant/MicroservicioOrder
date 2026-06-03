using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Handlers;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;
using OrderService.Presentation.Middlewares;
using System;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// ── Handlers (Application layer) ──────────────────────────────────────────────
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<AddItemToOrderHandler>();
builder.Services.AddScoped<RemoveItemFromOrderHandler>();
builder.Services.AddScoped<ChangeOrderStatusHandler>();
builder.Services.AddScoped<AddNoteToOrderHandler>();
builder.Services.AddScoped<UpdateItemStatusHandler>();
builder.Services.AddScoped<GetOrderByIdHandler>();
builder.Services.AddScoped<GetAllOrdersHandler>();
builder.Services.AddScoped<GetOrdersByStatusHandler>();
builder.Services.AddScoped<GetOrdersByTableHandler>();

// ── API ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Auto-migrate on startup in dev
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
