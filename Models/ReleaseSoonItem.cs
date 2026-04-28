namespace PrisonApp.Models;

public sealed class ReleaseSoonItem
{
    public required string FullName { get; init; }

    public DateTime ReleaseDate { get; init; }
}
