using PrisonApp.Models;

namespace PrisonApp.ViewModels;

public sealed class StatisticsViewModel : ViewModelBase
{
    public StatisticsViewModel(IEnumerable<Prisoner> prisoners)
    {
        var prisonerList = prisoners.ToList();
        var today = DateTime.Today;

        TotalPrisoners = prisonerList.Count;

        ArticleStatistics = prisonerList
            .GroupBy(prisoner => prisoner.CriminalArticle)
            .Select(group => new ArticleStatistic
            {
                Article = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Article)
            .ToList();

        CellStatistics = prisonerList
            .GroupBy(prisoner => prisoner.CellNumber)
            .Select(group => new CellStatistic
            {
                CellNumber = group.Key,
                Count = group.Count()
            })
            .OrderBy(item => item.CellNumber)
            .ToList();

        ReleasesThisMonth = prisonerList
            .Where(prisoner => prisoner.ReleaseDate is not null
                               && prisoner.ReleaseDate.Value.Year == today.Year
                               && prisoner.ReleaseDate.Value.Month == today.Month)
            .OrderBy(prisoner => prisoner.ReleaseDate)
            .Select(prisoner => new ReleaseSoonItem
            {
                FullName = prisoner.FullName,
                ReleaseDate = prisoner.ReleaseDate!.Value
            })
            .ToList();

        HierarchyStatistics = prisonerList
            .GroupBy(prisoner => prisoner.HierarchyLevel)
            .Select(group => new HierarchyStatistic
            {
                Hierarchy = group.Key.ToDisplayName(),
                Count = group.Count(),
                PercentageDisplay = TotalPrisoners == 0
                    ? "0 %"
                    : $"{group.Count() * 100.0 / TotalPrisoners:F1} %"
            })
            .OrderBy(item => item.Hierarchy)
            .ToList();

        CurrentMonthLabel = today.ToString("MMMM yyyy");
    }

    public int TotalPrisoners { get; }

    public IReadOnlyList<ArticleStatistic> ArticleStatistics { get; }

    public IReadOnlyList<CellStatistic> CellStatistics { get; }

    public IReadOnlyList<ReleaseSoonItem> ReleasesThisMonth { get; }

    public IReadOnlyList<HierarchyStatistic> HierarchyStatistics { get; }

    public string CurrentMonthLabel { get; }
}
