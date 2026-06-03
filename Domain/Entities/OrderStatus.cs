namespace OrderService.Domain.Entities;

public static class OrderStatus
{
    public const string Open = "Open";
    public const string InProgress = "InProgress";
    public const string ReadyToClose = "ReadyToClose";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";

    private static readonly Dictionary<string, string[]> ValidTransitions = new()
    {
        [Open] = [InProgress, Cancelled],
        [InProgress] = [ReadyToClose, Cancelled],
        [ReadyToClose] = [Closed, InProgress],
        [Closed] = [],
        [Cancelled] = []
    };

    public static bool IsValidTransition(string current, string next)
        => ValidTransitions.TryGetValue(current, out var allowed) && allowed.Contains(next);
}

public static class OrderItemStatus
{
    public const string Pending = "Pending";
    public const string SentToKitchen = "SentToKitchen";
    public const string Ready = "Ready";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
}
