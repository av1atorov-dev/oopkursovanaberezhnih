namespace PrisonApp.Models;

public sealed class HierarchyFilterOption
{
    public HierarchyLevel? Level { get; init; }

    public required string DisplayText { get; init; }
}
