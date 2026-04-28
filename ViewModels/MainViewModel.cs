using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using PrisonApp.Infrastructure;
using PrisonApp.Models;
using PrisonApp.Services;

namespace PrisonApp.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IDataService _dataService;
    private string _searchText = string.Empty;
    private CameraFilterOption? _selectedCameraFilter;
    private HierarchyFilterOption? _selectedHierarchyFilter;
    private DateTime? _releaseDateFrom;
    private DateTime? _releaseDateTo;
    private Prisoner? _selectedPrisoner;
    private bool _isEmptyStateVisible;
    private string _displayedCountText = "Відображено записів: 0";

    public MainViewModel(IDataService dataService)
    {
        _dataService = dataService;

        AllPrisoners = [];
        CameraFilterOptions = [];
        HierarchyFilterOptions = [];
        FilteredPrisoners = CollectionViewSource.GetDefaultView(AllPrisoners);
        FilteredPrisoners.Filter = FilterPrisoner;

        AddCommand = new RelayCommand(() => AddRequested?.Invoke(this, EventArgs.Empty));
        EditCommand = new RelayCommand(
            () =>
            {
                if (SelectedPrisoner is not null)
                {
                    EditRequested?.Invoke(this, SelectedPrisoner);
                }
            },
            () => SelectedPrisoner is not null);
        DeleteCommand = new RelayCommand(
            () =>
            {
                if (SelectedPrisoner is not null)
                {
                    DeleteRequested?.Invoke(this, SelectedPrisoner);
                }
            },
            () => SelectedPrisoner is not null);
        ShowStatisticsCommand = new RelayCommand(() => StatisticsRequested?.Invoke(this, EventArgs.Empty), () => AllPrisoners.Count > 0);
        ResetFiltersCommand = new RelayCommand(ResetFilters);
    }

    public event EventHandler? AddRequested;

    public event EventHandler<Prisoner>? EditRequested;

    public event EventHandler<Prisoner>? DeleteRequested;

    public event EventHandler? StatisticsRequested;

    public event EventHandler<string>? ErrorOccurred;

    public ObservableCollection<Prisoner> AllPrisoners { get; }

    public ObservableCollection<CameraFilterOption> CameraFilterOptions { get; }

    public ObservableCollection<HierarchyFilterOption> HierarchyFilterOptions { get; }

    public ICollectionView FilteredPrisoners { get; }

    public RelayCommand AddCommand { get; }

    public RelayCommand EditCommand { get; }

    public RelayCommand DeleteCommand { get; }

    public RelayCommand ShowStatisticsCommand { get; }

    public RelayCommand ResetFiltersCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RefreshFilteredView();
            }
        }
    }

    public CameraFilterOption? SelectedCameraFilter
    {
        get => _selectedCameraFilter;
        set
        {
            if (SetProperty(ref _selectedCameraFilter, value))
            {
                RefreshFilteredView();
            }
        }
    }

    public HierarchyFilterOption? SelectedHierarchyFilter
    {
        get => _selectedHierarchyFilter;
        set
        {
            if (SetProperty(ref _selectedHierarchyFilter, value))
            {
                RefreshFilteredView();
            }
        }
    }

    public DateTime? ReleaseDateFrom
    {
        get => _releaseDateFrom;
        set
        {
            if (SetProperty(ref _releaseDateFrom, value?.Date))
            {
                RefreshFilteredView();
            }
        }
    }

    public DateTime? ReleaseDateTo
    {
        get => _releaseDateTo;
        set
        {
            if (SetProperty(ref _releaseDateTo, value?.Date))
            {
                RefreshFilteredView();
            }
        }
    }

    public Prisoner? SelectedPrisoner
    {
        get => _selectedPrisoner;
        set
        {
            if (SetProperty(ref _selectedPrisoner, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsEmptyStateVisible
    {
        get => _isEmptyStateVisible;
        private set => SetProperty(ref _isEmptyStateVisible, value);
    }

    public string DisplayedCountText
    {
        get => _displayedCountText;
        private set => SetProperty(ref _displayedCountText, value);
    }

    public async Task InitializeAsync()
    {
        try
        {
            var loadedPrisoners = await _dataService.LoadAsync();
            AllPrisoners.Clear();

            foreach (var prisoner in loadedPrisoners.OrderBy(prisoner => prisoner.LastName).ThenBy(prisoner => prisoner.FirstName))
            {
                AllPrisoners.Add(prisoner);
            }

            BuildFilterOptions();
            RefreshFilteredView();
            ShowStatisticsCommand.RaiseCanExecuteChanged();
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Не вдалося завантажити дані: {ex.Message}");
        }
    }

    public async Task AddPrisonerAsync(Prisoner prisoner)
    {
        AllPrisoners.Add(prisoner);
        SortUnderlyingCollection();
        await PersistAsync();
    }

    public async Task UpdatePrisonerAsync(Prisoner updatedPrisoner)
    {
        var existingPrisoner = AllPrisoners.FirstOrDefault(prisoner => prisoner.Id == updatedPrisoner.Id);

        if (existingPrisoner is null)
        {
            return;
        }

        existingPrisoner.LastName = updatedPrisoner.LastName;
        existingPrisoner.FirstName = updatedPrisoner.FirstName;
        existingPrisoner.MiddleName = updatedPrisoner.MiddleName;
        existingPrisoner.BirthDate = updatedPrisoner.BirthDate;
        existingPrisoner.CriminalArticle = updatedPrisoner.CriminalArticle;
        existingPrisoner.SentenceYears = updatedPrisoner.SentenceYears;
        existingPrisoner.IsLifeSentence = updatedPrisoner.IsLifeSentence;
        existingPrisoner.ArrestDate = updatedPrisoner.ArrestDate;
        existingPrisoner.CellNumber = updatedPrisoner.CellNumber;
        existingPrisoner.HierarchyLevel = updatedPrisoner.HierarchyLevel;
        existingPrisoner.RelativesInfo = updatedPrisoner.RelativesInfo;
        existingPrisoner.CharacterNotes = updatedPrisoner.CharacterNotes;

        SortUnderlyingCollection();
        await PersistAsync();
    }

    public async Task DeletePrisonerAsync(Prisoner prisoner)
    {
        AllPrisoners.Remove(prisoner);
        SelectedPrisoner = null;
        await PersistAsync();
    }

    public StatisticsViewModel CreateStatisticsViewModel()
    {
        return new StatisticsViewModel(AllPrisoners);
    }

    public void RefreshSequenceNumbers()
    {
        var visiblePrisoners = FilteredPrisoners.Cast<Prisoner>().ToList();

        for (var index = 0; index < visiblePrisoners.Count; index++)
        {
            visiblePrisoners[index].SequenceNumber = index + 1;
        }

        IsEmptyStateVisible = visiblePrisoners.Count == 0;
        DisplayedCountText = $"Відображено записів: {visiblePrisoners.Count}";
    }

    private async Task PersistAsync()
    {
        try
        {
            BuildFilterOptions();
            RefreshFilteredView();
            ShowStatisticsCommand.RaiseCanExecuteChanged();
            await _dataService.SaveAsync(AllPrisoners);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Не вдалося зберегти дані: {ex.Message}");
        }
    }

    private void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedCameraFilter = CameraFilterOptions.FirstOrDefault();
        SelectedHierarchyFilter = HierarchyFilterOptions.FirstOrDefault();
        ReleaseDateFrom = null;
        ReleaseDateTo = null;
        RefreshFilteredView();
    }

    private bool FilterPrisoner(object item)
    {
        if (item is not Prisoner prisoner)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchValue = SearchText.Trim();
            var matchesSearch =
                prisoner.LastName.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                prisoner.FirstName.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                prisoner.MiddleName.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                prisoner.CriminalArticle.Contains(searchValue, StringComparison.OrdinalIgnoreCase);

            if (!matchesSearch)
            {
                return false;
            }
        }

        if (SelectedCameraFilter?.CellNumber is int cellNumber && prisoner.CellNumber != cellNumber)
        {
            return false;
        }

        if (SelectedHierarchyFilter?.Level is HierarchyLevel hierarchyLevel && prisoner.HierarchyLevel != hierarchyLevel)
        {
            return false;
        }

        if (ReleaseDateFrom is not null || ReleaseDateTo is not null)
        {
            if (prisoner.ReleaseDate is null)
            {
                return false;
            }

            if (ReleaseDateFrom is not null && prisoner.ReleaseDate.Value.Date < ReleaseDateFrom.Value.Date)
            {
                return false;
            }

            if (ReleaseDateTo is not null && prisoner.ReleaseDate.Value.Date > ReleaseDateTo.Value.Date)
            {
                return false;
            }
        }

        return true;
    }

    private void BuildFilterOptions()
    {
        var previouslySelectedCamera = SelectedCameraFilter?.CellNumber;
        var previouslySelectedHierarchy = SelectedHierarchyFilter?.Level;

        CameraFilterOptions.Clear();
        CameraFilterOptions.Add(new CameraFilterOption { DisplayText = "Усі", CellNumber = null });
        foreach (var cellNumber in AllPrisoners.Select(prisoner => prisoner.CellNumber).Distinct().OrderBy(number => number))
        {
            CameraFilterOptions.Add(new CameraFilterOption
            {
                CellNumber = cellNumber,
                DisplayText = cellNumber.ToString()
            });
        }

        HierarchyFilterOptions.Clear();
        HierarchyFilterOptions.Add(new HierarchyFilterOption { DisplayText = "Усі", Level = null });
        foreach (var level in Enum.GetValues<HierarchyLevel>())
        {
            HierarchyFilterOptions.Add(new HierarchyFilterOption
            {
                Level = level,
                DisplayText = level.ToDisplayName()
            });
        }

        SelectedCameraFilter = CameraFilterOptions.FirstOrDefault(option => option.CellNumber == previouslySelectedCamera)
                               ?? CameraFilterOptions.FirstOrDefault();
        SelectedHierarchyFilter = HierarchyFilterOptions.FirstOrDefault(option => option.Level == previouslySelectedHierarchy)
                                  ?? HierarchyFilterOptions.FirstOrDefault();
    }

    private void SortUnderlyingCollection()
    {
        var orderedPrisoners = AllPrisoners
            .OrderBy(prisoner => prisoner.LastName)
            .ThenBy(prisoner => prisoner.FirstName)
            .ThenBy(prisoner => prisoner.MiddleName)
            .ToList();

        AllPrisoners.Clear();
        foreach (var prisoner in orderedPrisoners)
        {
            AllPrisoners.Add(prisoner);
        }
    }

    private void RefreshFilteredView()
    {
        FilteredPrisoners.Refresh();
        RefreshSequenceNumbers();
    }
}
