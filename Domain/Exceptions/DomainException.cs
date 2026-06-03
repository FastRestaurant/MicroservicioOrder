namespace OrderService.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entity, Guid id)
        : base($"{entity} with id '{id}' was not found.") { }
}
