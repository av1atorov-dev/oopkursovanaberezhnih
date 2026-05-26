namespace PrisonApp.Models;

public sealed class CameraFilterOption
{
    public int? CellNumber { get; init; }

    public required string DisplayText { get; init; }
}

public sealed class HierarchyFilterOption
{
    public HierarchyLevel? Level { get; init; }

    public required string DisplayText { get; init; }
}
