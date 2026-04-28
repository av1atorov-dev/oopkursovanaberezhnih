namespace PrisonApp.Models;

public sealed class CellStatistic
{
    public int CellNumber { get; init; }

    public int Count { get; init; }

    public string OccupancyDisplay => $"{Count}/4 ({Count / 4.0:P0})";
}
