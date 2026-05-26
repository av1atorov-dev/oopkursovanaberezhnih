using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PrisonApp.Models;

namespace PrisonApp.Services;

public sealed class JsonDataService
{
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonDataService()
    {
        _dataFilePath = Path.Combine(AppContext.BaseDirectory, "data.json");
    }

    public List<Prisoner> Load()
    {
        if (!File.Exists(_dataFilePath))
        {
            var demoPrisoners = CreateDemoPrisoners();
            Save(demoPrisoners);
            return demoPrisoners;
        }

        var prisoners = ReadFromFile(_dataFilePath);

        if (prisoners.Count > 0)
        {
            return prisoners;
        }

        var fallbackPrisoners = CreateDemoPrisoners();
        Save(fallbackPrisoners);
        return fallbackPrisoners;
    }

    public void Save(IEnumerable<Prisoner> prisoners)
    {
        WriteToFile(_dataFilePath, prisoners);
    }

    public List<Prisoner> Import(string filePath)
    {
        return ReadFromFile(filePath);
    }

    public void Export(IEnumerable<Prisoner> prisoners, string filePath)
    {
        WriteToFile(filePath, prisoners);
    }

    private List<Prisoner> ReadFromFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var prisoners = JsonSerializer.Deserialize<List<Prisoner>>(stream, _serializerOptions);
        return prisoners ?? [];
    }

    private void WriteToFile(string filePath, IEnumerable<Prisoner> prisoners)
    {
        using var stream = File.Create(filePath);
        JsonSerializer.Serialize(stream, prisoners, _serializerOptions);
    }

    private static List<Prisoner> CreateDemoPrisoners()
    {
        return
        [
            new Prisoner
            {
                Id = Guid.NewGuid(),
                LastName = "Іваненко",
                FirstName = "Петро",
                MiddleName = "Олексійович",
                BirthDate = new DateTime(1985, 4, 12),
                CriminalArticle = "ст. 115 ч. 1",
                SentenceYears = 12,
                IsLifeSentence = false,
                ArrestDate = new DateTime(2018, 3, 14),
                CellNumber = 101,
                HierarchyLevel = HierarchyLevel.Muzhiki,
                RelativesInfo = "Мати проживає у м. Харків, підтримує листування.",
                CharacterNotes = "Спокійний, дисциплінований, конфліктів не провокує."
            },
            new Prisoner
            {
                Id = Guid.NewGuid(),
                LastName = "Коваленко",
                FirstName = "Сергій",
                MiddleName = "Миколайович",
                BirthDate = new DateTime(1979, 11, 3),
                CriminalArticle = "ст. 187 ч. 4",
                SentenceYears = 18,
                IsLifeSentence = false,
                ArrestDate = new DateTime(2012, 9, 28),
                CellNumber = 102,
                HierarchyLevel = HierarchyLevel.Blatni,
                RelativesInfo = "Дружина та син, дозволені короткострокові побачення.",
                CharacterNotes = "Комунікабельний, схильний до неформального лідерства."
            },
            new Prisoner
            {
                Id = Guid.NewGuid(),
                LastName = "Мельник",
                FirstName = "Олег",
                MiddleName = "Васильович",
                BirthDate = new DateTime(1968, 1, 21),
                CriminalArticle = "ст. 93",
                SentenceYears = null,
                IsLifeSentence = true,
                ArrestDate = new DateTime(2001, 7, 9),
                CellNumber = 103,
                HierarchyLevel = HierarchyLevel.Kozly,
                RelativesInfo = "Контакт підтримує лише сестра.",
                CharacterNotes = "Замкнений, потребує посиленого психологічного супроводу."
            }
        ];
    }
}
