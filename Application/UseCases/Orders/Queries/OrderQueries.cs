namespace OrderService.Application.UseCases.Orders.Queries;

public record GetOrderByIdQuery(Guid Id);
public record GetAllOrdersQuery();
public record GetOrdersByStatusQuery(string Status);
public record GetOrdersByTableQuery(int TableNumber);
