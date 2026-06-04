namespace OrderService.Domain.Constants;

public static class OrderItemStatusIds
{
    public const int Pending = 6;
    public const int SentToKitchen = 7;
    public const int Ready = 8;
    public const int Delivered = 9;
    public const int Cancelled = 10;

    private static readonly int[] ValidStatuses =
    [
        Pending,
        SentToKitchen,
        Ready,
        Delivered,
        Cancelled
    ];

    public static bool IsValid(int statusId)
        => ValidStatuses.Contains(statusId);
}
