using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Domain;

namespace Core.Import;

public static class BookJsonExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static int Save(IEnumerable<BookCopy> books, string path)
    {
        var dtos = books.Select(book => book.ToDto()).ToList();

        string? directory = Path.GetDirectoryName(path);
        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        string tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Options));
        File.Move(tmp, path, overwrite: true);

        return dtos.Count;
    }
}