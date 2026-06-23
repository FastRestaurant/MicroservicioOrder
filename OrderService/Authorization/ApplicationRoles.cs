namespace OrderService.Presentation.Authorization;

internal static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string Waitress = "Waitress";
    public const string Kitchen = "Kitchen";

    public const string AdminOrWaitress = "Admin,Waitress";
    public const string AdminOrKitchen = "Admin,Kitchen";
    public const string AdminWaitressOrKitchen = "Admin,Waitress,Kitchen";
}
