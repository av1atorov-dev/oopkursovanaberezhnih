using System.Collections.ObjectModel;
using PrisonApp.Models;

namespace PrisonApp.ViewModels;

public sealed class ExportViewModel : ViewModelBase
{
    public ObservableCollection<ExportColumnOption> Columns { get; } =
    [
        new() { DisplayName = "№", PropertyKey = "SequenceNumber", IsSelected = true },
        new() { DisplayName = "Прізвище", PropertyKey = "LastName", IsSelected = true },
        new() { DisplayName = "Ім'я", PropertyKey = "FirstName", IsSelected = true },
        new() { DisplayName = "По батькові", PropertyKey = "MiddleName", IsSelected = true },
        new() { DisplayName = "Дата народження", PropertyKey = "BirthDate", IsSelected = true },
        new() { DisplayName = "Стаття", PropertyKey = "CriminalArticle", IsSelected = true },
        new() { DisplayName = "Термін", PropertyKey = "SentenceYears", IsSelected = true },
        new() { DisplayName = "Камера", PropertyKey = "CellNumber", IsSelected = true },
        new() { DisplayName = "Ієрархія", PropertyKey = "HierarchyLevel", IsSelected = true },
        new() { DisplayName = "Дата арешту", PropertyKey = "ArrestDate", IsSelected = false },
        new() { DisplayName = "Дата звільнення", PropertyKey = "ReleaseDate", IsSelected = false },
        new() { DisplayName = "Родичі", PropertyKey = "RelativesInfo", IsSelected = false },
        new() { DisplayName = "Нотатки", PropertyKey = "CharacterNotes", IsSelected = false }
    ];

    public bool HasSelection => Columns.Any(column => column.IsSelected);
}
