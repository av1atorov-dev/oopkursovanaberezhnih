namespace PrisonApp.Models;

public sealed class ExportColumnOption : ObservableObject
{
    private bool _isSelected = true;

    public string DisplayName { get; init; } = string.Empty;

    public string PropertyKey { get; init; } = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
