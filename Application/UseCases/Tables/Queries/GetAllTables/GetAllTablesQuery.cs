namespace OrderService.Application.UseCases.Tables.Queries.GetAllTables;

public sealed class GetAllTablesQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}
