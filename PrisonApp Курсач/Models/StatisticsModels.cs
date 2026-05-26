namespace PrisonApp.Models;

public sealed class ArticleStatistic
{
    public required string Article { get; init; }

    public int Count { get; init; }
}

public sealed class CellStatistic
{
    public int CellNumber { get; init; }

    public int Count { get; init; }

    public string OccupancyDisplay => $"{Count}/4 ({Count / 4.0:P0})";
}

public sealed class HierarchyStatistic
{
    public required string Hierarchy { get; init; }

    public int Count { get; init; }

    public string PercentageDisplay { get; init; } = "0 %";
}

public sealed class ReleaseSoonItem
{
    public required string FullName { get; init; }

    public DateTime ReleaseDate { get; init; }
}
