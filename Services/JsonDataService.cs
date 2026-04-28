using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PrisonApp.Models;

namespace PrisonApp.Services;

public sealed class JsonDataService : IDataService
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

    public async Task<List<Prisoner>> LoadAsync()
    {
        if (!File.Exists(_dataFilePath))
        {
            var demoPrisoners = CreateDemoPrisoners();
            await SaveAsync(demoPrisoners);
            return demoPrisoners;
        }

        await using var stream = File.OpenRead(_dataFilePath);
        var prisoners = await JsonSerializer.DeserializeAsync<List<Prisoner>>(stream, _serializerOptions);

        if (prisoners is not { Count: > 0 })
        {
            var demoPrisoners = CreateDemoPrisoners();
            await SaveAsync(demoPrisoners);
            return demoPrisoners;
        }

        return prisoners;
    }

    public async Task SaveAsync(IEnumerable<Prisoner> prisoners)
    {
        await using var stream = File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(stream, prisoners, _serializerOptions);
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
