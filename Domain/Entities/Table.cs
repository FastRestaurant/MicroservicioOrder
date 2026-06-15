namespace OrderService.Domain.Entities;

public class Table
{
    public Guid Id { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public bool Status { get; private set; }

    private Table() { }

    public static Table Create(string number)
    {
        return new Table
        {
            Id = Guid.NewGuid(),
            Number = number,
            Status = true
        };
    }

    public void Enable() => Status = true;
    public void Disable() => Status = false;
}