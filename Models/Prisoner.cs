using System.Text.Json.Serialization;
using PrisonApp.Infrastructure;

namespace PrisonApp.Models;

public sealed class Prisoner : ObservableObject
{
    private Guid _id;
    private string _lastName = string.Empty;
    private string _firstName = string.Empty;
    private string _middleName = string.Empty;
    private DateTime _birthDate = DateTime.Today;
    private string _criminalArticle = string.Empty;
    private int? _sentenceYears;
    private bool _isLifeSentence;
    private DateTime _arrestDate = DateTime.Today;
    private int _cellNumber;
    private HierarchyLevel _hierarchyLevel;
    private string _relativesInfo = string.Empty;
    private string _characterNotes = string.Empty;
    private int _sequenceNumber;

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            if (SetProperty(ref _lastName, value))
            {
                OnPropertyChanged(nameof(FullName));
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
                OnPropertyChanged(nameof(FullName));
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
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public DateTime BirthDate
    {
        get => _birthDate;
        set => SetProperty(ref _birthDate, value.Date);
    }

    public string CriminalArticle
    {
        get => _criminalArticle;
        set => SetProperty(ref _criminalArticle, value);
    }

    public int? SentenceYears
    {
        get => _sentenceYears;
        set
        {
            if (SetProperty(ref _sentenceYears, value))
            {
                OnSentenceChanged();
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
                OnSentenceChanged();
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
                OnPropertyChanged(nameof(ReleaseDate));
                OnPropertyChanged(nameof(ReleaseDateDisplay));
            }
        }
    }

    public int CellNumber
    {
        get => _cellNumber;
        set => SetProperty(ref _cellNumber, value);
    }

    public HierarchyLevel HierarchyLevel
    {
        get => _hierarchyLevel;
        set
        {
            if (SetProperty(ref _hierarchyLevel, value))
            {
                OnPropertyChanged(nameof(HierarchyDisplay));
            }
        }
    }

    public string RelativesInfo
    {
        get => _relativesInfo;
        set => SetProperty(ref _relativesInfo, value);
    }

    public string CharacterNotes
    {
        get => _characterNotes;
        set => SetProperty(ref _characterNotes, value);
    }

    [JsonIgnore]
    public int SequenceNumber
    {
        get => _sequenceNumber;
        set => SetProperty(ref _sequenceNumber, value);
    }

    [JsonIgnore]
    public string FullName => string.Join(
        ' ',
        new[] { LastName, FirstName, MiddleName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim()));

    [JsonIgnore]
    public DateTime? ReleaseDate => IsLifeSentence || SentenceYears is null
        ? null
        : ArrestDate.AddYears(SentenceYears.Value);

    [JsonIgnore]
    public string ReleaseDateDisplay => IsLifeSentence
        ? "Довічно"
        : ReleaseDate?.ToString("dd.MM.yyyy") ?? "—";

    [JsonIgnore]
    public string SentenceDisplay => IsLifeSentence
        ? "Довічно"
        : SentenceYears?.ToString() ?? "—";

    [JsonIgnore]
    public string HierarchyDisplay => HierarchyLevel.ToDisplayName();

    public Prisoner Clone()
    {
        return new Prisoner
        {
            Id = Id,
            LastName = LastName,
            FirstName = FirstName,
            MiddleName = MiddleName,
            BirthDate = BirthDate,
            CriminalArticle = CriminalArticle,
            SentenceYears = SentenceYears,
            IsLifeSentence = IsLifeSentence,
            ArrestDate = ArrestDate,
            CellNumber = CellNumber,
            HierarchyLevel = HierarchyLevel,
            RelativesInfo = RelativesInfo,
            CharacterNotes = CharacterNotes,
            SequenceNumber = SequenceNumber
        };
    }

    private void OnSentenceChanged()
    {
        if (IsLifeSentence && SentenceYears is not null)
        {
            _sentenceYears = null;
            OnPropertyChanged(nameof(SentenceYears));
        }

        OnPropertyChanged(nameof(ReleaseDate));
        OnPropertyChanged(nameof(ReleaseDateDisplay));
        OnPropertyChanged(nameof(SentenceDisplay));
    }
}
