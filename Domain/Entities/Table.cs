namespace OrderService.Domain.Entities;

public class Table
{
    public Guid Id { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public int SeatCount { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }

    private Table() { }

    public static Table Create(string number, int seatCount, string location, bool isEnabled)
    {
        return new Table
        {
            Id = Guid.NewGuid(),
            Number = number,
            SeatCount = seatCount,
            Location = location,
            IsEnabled = isEnabled
        };
    }

    public void Update(string number, int seatCount, string location, bool isEnabled)
    {
        Number = number;
        SeatCount = seatCount;
        Location = location;
        IsEnabled = isEnabled;
    }

    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
}
