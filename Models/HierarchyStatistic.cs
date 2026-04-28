namespace PrisonApp.Models;

public sealed class HierarchyStatistic
{
    public required string Hierarchy { get; init; }

    public int Count { get; init; }

    public string PercentageDisplay { get; init; } = "0 %";
}
