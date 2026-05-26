namespace PrisonApp.Models;

public enum HierarchyLevel
{
    Undefined,
    Muzhiki,
    Kozly,
    Blatni,
    Opushcheni
}

public static class HierarchyLevelExtensions
{
    public static string ToDisplayName(this HierarchyLevel level)
    {
        return level switch
        {
            HierarchyLevel.Undefined => "не визначено",
            HierarchyLevel.Muzhiki => "мужики",
            HierarchyLevel.Kozly => "козли",
            HierarchyLevel.Blatni => "блатні",
            HierarchyLevel.Opushcheni => "опущені",
            _ => "не визначено"
        };
    }
}
