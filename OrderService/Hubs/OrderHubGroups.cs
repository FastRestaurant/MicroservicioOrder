namespace OrderService.Presentation.Hubs;

/// <summary>
/// Nombres de los grupos de SignalR del <see cref="OrderHub"/>.
/// Un usuario Admin se suma a los tres grupos; Waitress y Kitchen solo al propio.
/// </summary>
public static class OrderHubGroups
{
    public const string Admin = "role:Admin";
    public const string Waitress = "role:Waitress";
    public const string Kitchen = "role:Kitchen";
}
