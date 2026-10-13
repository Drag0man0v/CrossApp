using System.Globalization;
using Core.DTO;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var books = new List<BookDto>();
        var readers = new List<ReaderDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            switch (ParseLine(line))
            {
                case ParseBook book:
                    books.Add(book.Value);
                    break;
                case ParseReader reader:
                    readers.Add(reader.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new MixedImportResult(books, readers, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["B", _, "", _, _, _, _] or ["B", _, _, "", _, _, _] or ["B", _, _, _, _, _, ""]
                => new ParseFailed("ISBN, назва або жанр порожні"),
  
            ["B", _, _, _, _, var year, _] when !int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)
                => new ParseFailed($"рік '{year}' не є цілим числом"),

            ["B", _, _, _,_, var year, _] when int.Parse(year, CultureInfo.InvariantCulture) is < 0 or > 2026
                => new ParseFailed($"рік '{year}' поза допустимими межами"),

            ["B", var id, var isbn, var title, var author, var year, var genre]
                => new ParseBook(new BookDto(
                    id, isbn, title,
                    author == "" ? null : author,
                    int.Parse(year, CultureInfo.InvariantCulture),
                    genre)),

            ["R", _, "", _, _]
                => new ParseFailed("ім'я читача порожнє"),

            ["R", var id, var name, var email, var phone]
                => new ParseReader(new ReaderDto(
                    id, name, email,
                    phone == "" ? null : phone)),
            
            [var prefix, ..] when prefix is not ("B" or "R")
                => new ParseFailed($"невідомий префікс '{prefix}'"),

            ["B" or "R", ..]
                => new ParseFailed($"неправильна кількість колонок: {parts.Length}"),

            _ => new ParseFailed("порожній рядок")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseBook(BookDto Value) : ParseOutcome;
    private sealed record ParseReader(ReaderDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}