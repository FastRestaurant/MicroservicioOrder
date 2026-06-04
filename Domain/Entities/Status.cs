namespace OrderService.Domain.Entities;

public class Status
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;

    private Status() { }

    public Status(int id, string name, string type)
    {
        Id = id;
        Name = name;
        Type = type;
    }
}
