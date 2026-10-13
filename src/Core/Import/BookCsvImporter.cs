namespace Core.Import;

using System.Globalization;
using Core.DTO;

public static class BookCsvImporter
{
    // Роздільник — крапка з комою: не конфліктує з комою в назвах.
    private const char Separator = ';';

    public static ImportResult<BookDto> Load(string path)
    {
        var items = new List<BookDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue; // рядок заголовків

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<BookDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 6 }
                => new ParseFailed($"очікую 6 колонок, отримав {parts.Length}"),

            [_, "", _, _, _, _] or [_, _, "", _, _, _] or [_, _, _, _, _, ""]
                => new ParseFailed("ISBN, назва або жанр порожні"),

            [_, _, _, _, var year, _] when !int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)
                => new ParseFailed($"рік '{year}' не є цілим числом"),

            [_, _, _, _, var year, _] when int.Parse(year, CultureInfo.InvariantCulture) is < 0 or > 2026
                => new ParseFailed($"рік '{year}' поза допустимими межами"),

            [var id, var isbn, var title, var author, var year, var genre]
                => new ParseOk(new BookDto(
                    id, isbn, title,
                    author == "" ? null : author,
                    int.Parse(year, CultureInfo.InvariantCulture),
                    genre)),

            _ => new ParseFailed($"Шось непонятне м начудив: {line}"),
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(BookDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}