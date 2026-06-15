using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Commands.AddItemToOrder;
using OrderService.Application.UseCases.Orders.Commands.AddNoteToOrder;
using OrderService.Application.UseCases.Orders.Commands.ChangeOrderStatus;
using OrderService.Application.UseCases.Orders.Commands.CreateOrder;
using OrderService.Application.UseCases.Orders.Commands.RemoveItemFromOrder;
using OrderService.Application.UseCases.Orders.Commands.UpdateItemStatus;
using OrderService.Application.UseCases.Orders.Queries.GetAllOrders;
using OrderService.Application.UseCases.Orders.Queries.GetOrderById;
using OrderService.Application.UseCases.Orders.Queries.GetOrderItemStatuses;
using OrderService.Application.UseCases.Orders.Queries.GetOrdersByStatus;
using OrderService.Application.UseCases.Orders.Queries.GetOrdersByTable;
using OrderService.Application.UseCases.Orders.Queries.GetOrderStatuses;
using OrderService.Application.UseCases.Tables.Commands.CreateTable;
using OrderService.Application.UseCases.Tables.Commands.ToggleTableStatus;
using OrderService.Application.UseCases.Tables.Queries.GetAllTables;
using OrderService.Application.UseCases.Tables.Queries.GetTableById;
using OrderService.Infrastructure.Persistence.Repositories;
using OrderService.Infrastructure.ExternalServices;
using OrderService.Infrastructure.Persistence;
using OrderService.Presentation.Middlewares;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["ExternalServices:Users:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
        client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IMenuCatalogClient, MenuCatalogClient>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["ExternalServices:MenuCatalog:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
        client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<ICreateOrderCommandHandler, CreateOrderCommandHandler>();
builder.Services.AddScoped<IAddItemToOrderCommandHandler, AddItemToOrderCommandHandler>();
builder.Services.AddScoped<IRemoveItemFromOrderCommandHandler, RemoveItemFromOrderCommandHandler>();
builder.Services.AddScoped<IChangeOrderStatusCommandHandler, ChangeOrderStatusCommandHandler>();
builder.Services.AddScoped<IAddNoteToOrderCommandHandler, AddNoteToOrderCommandHandler>();
builder.Services.AddScoped<IUpdateItemStatusCommandHandler, UpdateItemStatusCommandHandler>();
builder.Services.AddScoped<IGetAllOrdersQueryHandler, GetAllOrdersQueryHandler>();
builder.Services.AddScoped<IGetOrdersByStatusQueryHandler, GetOrdersByStatusQueryHandler>();
builder.Services.AddScoped<IGetOrdersByTableQueryHandler, GetOrdersByTableQueryHandler>();
builder.Services.AddScoped<IGetOrderStatusesQueryHandler, GetOrderStatusesQueryHandler>();
builder.Services.AddScoped<IGetOrderItemStatusesQueryHandler, GetOrderItemStatusesQueryHandler>();
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<IGetAllTablesQueryHandler, GetAllTablesQueryHandler>();
builder.Services.AddScoped<IGetOrderByIdQueryHandler, GetOrderByIdQueryHandler>();
builder.Services.AddScoped<IGetTableByIdQueryHandler, GetTableByIdQueryHandler>();
builder.Services.AddScoped<ICreateTableCommandHandler, CreateTableCommandHandler>();
builder.Services.AddScoped<IToggleTableStatusCommandHandler, ToggleTableStatusCommandHandler>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Values
                .SelectMany(modelState => modelState.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "La solicitud es invalida." : error.ErrorMessage)
                .ToArray();

            var response = new ErrorResponseDto
            {
                Message = errors.Length == 0 ? "La solicitud es invalida." : string.Join(" ", errors),
                StatusCode = StatusCodes.Status400BadRequest,
                Timestamp = DateTime.UtcNow
            };

            return new BadRequestObjectResult(response);
        };
    });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)),
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await AppDbInitializer.InitializeAsync(db);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
