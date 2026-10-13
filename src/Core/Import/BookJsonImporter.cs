namespace Core.Import;

using System.Text.Json;
using Core.DTO;

public static class BookJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<BookDto> Load(string path)
    {
        var items = new List<BookDto>();
        var errors = new List<string>();

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(File.ReadAllText(path));
        }
        catch (JsonException)
        {
            errors.Add("файл не є коректним JSON");
            return new ImportResult<BookDto>(items, errors);
        }

        using (doc)
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add("очікую масив записів на верхньому рівні");
                return new ImportResult<BookDto>(items, errors);
            }

            int index = 0;
            foreach ( JsonElement element in doc.RootElement.EnumerateArray())
            {
                index++;
                try
                {
                    BookDto? book = element.Deserialize<BookDto>(Options);

                    if (Validate(book) is string problem)
                        errors.Add($"елемент {index}: {problem}");
                    else
                        items.Add(book!);
                }
                catch (JsonException)
                {
                    errors.Add($"елемент {index}: не вдалося прочитати поля (перевір типи значень)");
                }
            }
        }

        return new ImportResult<BookDto>(items, errors);
    }

    private static string? Validate(BookDto? book) => book switch
    {
        null => "порожній запис",
        { } b when string.IsNullOrWhiteSpace(b.Id)
                || string.IsNullOrWhiteSpace(b.Isbn)
                || string.IsNullOrWhiteSpace(b.Title)
                || string.IsNullOrWhiteSpace(b.Genre)
            => "ID, ISBN, назва або жанр порожні",
        { } b when b.Year < 0 || b.Year > DateTime.Now.Year
            => $"рік '{b.Year}' поза допустимими межами",
        _ => null
    };
}