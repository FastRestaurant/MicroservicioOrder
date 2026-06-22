namespace OrderService.Domain.Constants;

public static class OrderStatusIds
{
    public const int Open = 1;
    public const int InProgress = 2;
    public const int ReadyToClose = 3;
    public const int Closed = 4;
    public const int Cancelled = 5;


    private static readonly Dictionary<int, int[]> ValidTransitions = new()
    {
        [Open] = [InProgress, Cancelled],
        [InProgress] = [ReadyToClose, Cancelled],
        [ReadyToClose] = [Closed, Cancelled],
        [Closed] = [],
        [Cancelled] = []
    };

    public static bool IsValidTransition(int current, int next)
        => ValidTransitions.TryGetValue(current, out var allowed) && allowed.Contains(next);

    public static bool IsTerminal(int statusId)
        => statusId == Closed || statusId == Cancelled;
}