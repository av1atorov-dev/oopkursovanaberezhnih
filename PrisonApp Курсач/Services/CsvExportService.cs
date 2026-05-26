using System.IO;
using System.Text;
using PrisonApp.Models;

namespace PrisonApp.Services;

public static class CsvExportService
{
    public static void Export(
        IEnumerable<Prisoner> prisoners,
        IEnumerable<ExportColumnOption> selectedColumns,
        string filePath)
    {
        var columns = selectedColumns.Where(column => column.IsSelected).ToList();

        using var writer = new StreamWriter(filePath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        writer.WriteLine(string.Join(';', columns.Select(column => Escape(column.DisplayName))));

        foreach (var prisoner in prisoners)
        {
            var values = columns.Select(column => Escape(GetValue(prisoner, column.PropertyKey)));
            writer.WriteLine(string.Join(';', values));
        }
    }

    private static string GetValue(Prisoner prisoner, string key) => key switch
    {
        "SequenceNumber" => prisoner.SequenceNumber.ToString(),
        "LastName" => prisoner.LastName,
        "FirstName" => prisoner.FirstName,
        "MiddleName" => prisoner.MiddleName,
        "BirthDate" => prisoner.BirthDate.ToString("dd.MM.yyyy"),
        "CriminalArticle" => prisoner.CriminalArticle,
        "SentenceYears" => prisoner.SentenceDisplay,
        "CellNumber" => prisoner.CellNumber.ToString(),
        "HierarchyLevel" => prisoner.HierarchyDisplay,
        "ArrestDate" => prisoner.ArrestDate.ToString("dd.MM.yyyy"),
        "ReleaseDate" => prisoner.ReleaseDateDisplay,
        "RelativesInfo" => prisoner.RelativesInfo,
        "CharacterNotes" => prisoner.CharacterNotes,
        _ => string.Empty
    };

    private static string Escape(string value)
    {
        var safeValue = value.Replace("\"", "\"\"");
        return $"\"{safeValue}\"";
    }
}
