using OrderService.Domain.Exceptions;

namespace OrderService.Domain.Entities;

public class Table
{
    public Guid Id { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public int SeatCount { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public decimal? PositionX { get; private set; }
    public decimal? PositionY { get; private set; }
    public byte[] Version { get; private set; } = [];

    private Table() { }

    public static Table Create(string number, int seatCount, string location, bool isEnabled)
    {
        Validate(number, seatCount, location);
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
        Validate(number, seatCount, location);
        Number = number;
        SeatCount = seatCount;
        Location = location;
        IsEnabled = isEnabled;
    }

    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;

    public void UpdatePosition(decimal positionX, decimal positionY)
    {
        if (positionX is < 0 or > 100 || positionY is < 0 or > 100)
            throw new ValidationException("Las coordenadas de la mesa deben estar entre 0 y 100.");

        PositionX = positionX;
        PositionY = positionY;
    }

    private static void Validate(string number, int seatCount, string location)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ValidationException("El número de mesa es obligatorio.");

        if (seatCount <= 0)
            throw new ValidationException("La cantidad de sillas debe ser mayor a cero.");

        if (string.IsNullOrWhiteSpace(location))
            throw new ValidationException("La ubicación de la mesa es obligatoria.");
    }
}
