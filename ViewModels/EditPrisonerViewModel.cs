using System.Collections;
using System.ComponentModel;
using PrisonApp.Infrastructure;
using PrisonApp.Models;

namespace PrisonApp.ViewModels;

public sealed class EditPrisonerViewModel : ViewModelBase, INotifyDataErrorInfo
{
    private readonly Guid _editingId;
    private readonly bool _isEditMode;
    private readonly Dictionary<string, List<string>> _errors = new();

    private string _lastName = string.Empty;
    private string _firstName = string.Empty;
    private string _middleName = string.Empty;
    private DateTime _birthDate = DateTime.Today;
    private string _criminalArticle = string.Empty;
    private string _sentenceYearsText = string.Empty;
    private bool _isLifeSentence;
    private DateTime _arrestDate = DateTime.Today;
    private string _cellNumberText = string.Empty;
    private HierarchyLevel _selectedHierarchyLevel = HierarchyLevel.Undefined;
    private string _relativesInfo = string.Empty;
    private string _characterNotes = string.Empty;

    public EditPrisonerViewModel(Prisoner? prisoner = null)
    {
        _isEditMode = prisoner is not null;
        _editingId = prisoner?.Id ?? Guid.NewGuid();
        HierarchyOptions =
        [
            HierarchyLevel.Undefined,
            HierarchyLevel.Muzhiki,
            HierarchyLevel.Kozly,
            HierarchyLevel.Blatni,
            HierarchyLevel.Opushcheni
        ];

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => RequestClose?.Invoke(this, false));

        if (prisoner is not null)
        {
            LastName = prisoner.LastName;
            FirstName = prisoner.FirstName;
            MiddleName = prisoner.MiddleName;
            BirthDate = prisoner.BirthDate;
            CriminalArticle = prisoner.CriminalArticle;
            IsLifeSentence = prisoner.IsLifeSentence;
            SentenceYearsText = prisoner.IsLifeSentence ? string.Empty : prisoner.SentenceYears?.ToString() ?? string.Empty;
            ArrestDate = prisoner.ArrestDate;
            CellNumberText = prisoner.CellNumber.ToString();
            SelectedHierarchyLevel = prisoner.HierarchyLevel;
            RelativesInfo = prisoner.RelativesInfo;
            CharacterNotes = prisoner.CharacterNotes;
        }
        else
        {
            BirthDate = DateTime.Today.AddYears(-18);
            ArrestDate = DateTime.Today;
            SelectedHierarchyLevel = HierarchyLevel.Undefined;
        }
    }

    public event EventHandler<bool?>? RequestClose;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public RelayCommand SaveCommand { get; }

    public RelayCommand CancelCommand { get; }

    public IReadOnlyList<HierarchyLevel> HierarchyOptions { get; }

    public Prisoner? SavedPrisoner { get; private set; }

    public string WindowTitle => _isEditMode
        ? $"Редагування: {LastName} {FirstName}".TrimEnd(':', ' ')
        : "Додавання ув'язненого";

    public bool HasErrors => _errors.Count > 0;

    public string LastName
    {
        get => _lastName;
        set
        {
            if (SetProperty(ref _lastName, value))
            {
                ValidateLastName();
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    public string FirstName
    {
        get => _firstName;
        set
        {
            if (SetProperty(ref _firstName, value))
            {
                ValidateFirstName();
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    public string MiddleName
    {
        get => _middleName;
        set
        {
            if (SetProperty(ref _middleName, value))
            {
                ValidateMiddleName();
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    public DateTime BirthDate
    {
        get => _birthDate;
        set
        {
            if (SetProperty(ref _birthDate, value.Date))
            {
                ValidateBirthDate();
                ValidateArrestDate();
            }
        }
    }

    public string CriminalArticle
    {
        get => _criminalArticle;
        set
        {
            if (SetProperty(ref _criminalArticle, value))
            {
                ValidateCriminalArticle();
            }
        }
    }

    public string SentenceYearsText
    {
        get => _sentenceYearsText;
        set
        {
            if (SetProperty(ref _sentenceYearsText, value))
            {
                ValidateSentenceYears();
                OnPropertyChanged(nameof(ReleaseDateText));
            }
        }
    }

    public bool IsLifeSentence
    {
        get => _isLifeSentence;
        set
        {
            if (SetProperty(ref _isLifeSentence, value))
            {
                if (value)
                {
                    SentenceYearsText = string.Empty;
                    ClearErrors(nameof(SentenceYearsText));
                }

                ValidateSentenceYears();
                OnPropertyChanged(nameof(IsSentenceYearsEnabled));
                OnPropertyChanged(nameof(ReleaseDateText));
            }
        }
    }

    public DateTime ArrestDate
    {
        get => _arrestDate;
        set
        {
            if (SetProperty(ref _arrestDate, value.Date))
            {
                ValidateArrestDate();
                OnPropertyChanged(nameof(ReleaseDateText));
            }
        }
    }

    public string CellNumberText
    {
        get => _cellNumberText;
        set
        {
            if (SetProperty(ref _cellNumberText, value))
            {
                ValidateCellNumber();
            }
        }
    }

    public HierarchyLevel SelectedHierarchyLevel
    {
        get => _selectedHierarchyLevel;
        set => SetProperty(ref _selectedHierarchyLevel, value);
    }

    public string RelativesInfo
    {
        get => _relativesInfo;
        set
        {
            if (SetProperty(ref _relativesInfo, value))
            {
                ValidateRelativesInfo();
            }
        }
    }

    public string CharacterNotes
    {
        get => _characterNotes;
        set
        {
            if (SetProperty(ref _characterNotes, value))
            {
                ValidateCharacterNotes();
            }
        }
    }

    public bool IsSentenceYearsEnabled => !IsLifeSentence;

    public string ReleaseDateText
    {
        get
        {
            if (IsLifeSentence)
            {
                return "Довічно";
            }

            if (!int.TryParse(SentenceYearsText, out var years) || years < 1 || years > 25)
            {
                return "Вкажіть коректний термін";
            }

            return ArrestDate.AddYears(years).ToString("dd.MM.yyyy");
        }
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return _errors.SelectMany(pair => pair.Value).ToList();
        }

        return _errors.TryGetValue(propertyName, out var errors)
            ? errors
            : Enumerable.Empty<string>();
    }

    private void Save()
    {
        ValidateAll();

        if (HasErrors)
        {
            return;
        }

        SavedPrisoner = new Prisoner
        {
            Id = _editingId,
            LastName = LastName.Trim(),
            FirstName = FirstName.Trim(),
            MiddleName = MiddleName.Trim(),
            BirthDate = BirthDate,
            CriminalArticle = CriminalArticle.Trim(),
            SentenceYears = IsLifeSentence ? null : int.Parse(SentenceYearsText),
            IsLifeSentence = IsLifeSentence,
            ArrestDate = ArrestDate,
            CellNumber = int.Parse(CellNumberText),
            HierarchyLevel = SelectedHierarchyLevel,
            RelativesInfo = RelativesInfo.Trim(),
            CharacterNotes = CharacterNotes.Trim()
        };

        RequestClose?.Invoke(this, true);
    }

    private void ValidateAll()
    {
        ValidateLastName();
        ValidateFirstName();
        ValidateMiddleName();
        ValidateBirthDate();
        ValidateCriminalArticle();
        ValidateSentenceYears();
        ValidateArrestDate();
        ValidateCellNumber();
        ValidateRelativesInfo();
        ValidateCharacterNotes();
    }

    private void ValidateLastName()
    {
        SetErrors(nameof(LastName), ValidateRequiredText(LastName, "Прізвище", 100));
    }

    private void ValidateFirstName()
    {
        SetErrors(nameof(FirstName), ValidateRequiredText(FirstName, "Ім'я", 100));
    }

    private void ValidateMiddleName()
    {
        SetErrors(nameof(MiddleName), ValidateOptionalText(MiddleName, "По батькові", 100));
    }

    private void ValidateBirthDate()
    {
        var errors = new List<string>();

        if (BirthDate > DateTime.Today)
        {
            errors.Add("Дата народження не може бути пізніше поточної дати.");
        }

        if (BirthDate > ArrestDate)
        {
            errors.Add("Дата народження не може бути пізніше дати взяття під варту.");
        }

        SetErrors(nameof(BirthDate), errors);
    }

    private void ValidateCriminalArticle()
    {
        SetErrors(nameof(CriminalArticle), ValidateRequiredText(CriminalArticle, "Стаття КК", 50));
    }

    private void ValidateSentenceYears()
    {
        var errors = new List<string>();

        if (!IsLifeSentence)
        {
            if (string.IsNullOrWhiteSpace(SentenceYearsText))
            {
                errors.Add("Вкажіть термін покарання.");
            }
            else if (!int.TryParse(SentenceYearsText, out var years))
            {
                errors.Add("Термін покарання має бути цілим числом.");
            }
            else if (years is < 1 or > 25)
            {
                errors.Add("Термін покарання має бути в межах від 1 до 25 років.");
            }
        }

        SetErrors(nameof(SentenceYearsText), errors);
    }

    private void ValidateArrestDate()
    {
        var errors = new List<string>();

        if (ArrestDate > DateTime.Today)
        {
            errors.Add("Дата взяття під варту не може бути пізніше поточної дати.");
        }

        if (ArrestDate < BirthDate)
        {
            errors.Add("Дата взяття під варту не може бути раніше дати народження.");
        }

        SetErrors(nameof(ArrestDate), errors);
    }

    private void ValidateCellNumber()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(CellNumberText))
        {
            errors.Add("Вкажіть номер камери.");
        }
        else if (!int.TryParse(CellNumberText, out var cellNumber))
        {
            errors.Add("Номер камери має бути цілим числом.");
        }
        else if (cellNumber is < 1 or > 999)
        {
            errors.Add("Номер камери має бути в межах від 1 до 999.");
        }

        SetErrors(nameof(CellNumberText), errors);
    }

    private void ValidateRelativesInfo()
    {
        SetErrors(nameof(RelativesInfo), ValidateOptionalText(RelativesInfo, "Відомості про родичів", 500));
    }

    private void ValidateCharacterNotes()
    {
        SetErrors(nameof(CharacterNotes), ValidateOptionalText(CharacterNotes, "Особливості характеру", 500));
    }

    private static List<string> ValidateRequiredText(string value, string fieldName, int maxLength)
    {
        var errors = new List<string>();
        var trimmedValue = value.Trim();

        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            errors.Add($"Поле «{fieldName}» є обов'язковим.");
        }
        else if (trimmedValue.Length > maxLength)
        {
            errors.Add($"Поле «{fieldName}» не може містити більше {maxLength} символів.");
        }

        return errors;
    }

    private static List<string> ValidateOptionalText(string value, string fieldName, int maxLength)
    {
        var errors = new List<string>();

        if (value.Trim().Length > maxLength)
        {
            errors.Add($"Поле «{fieldName}» не може містити більше {maxLength} символів.");
        }

        return errors;
    }

    private void SetErrors(string propertyName, List<string> errors)
    {
        if (errors.Count == 0)
        {
            ClearErrors(propertyName);
            return;
        }

        _errors[propertyName] = errors;
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
