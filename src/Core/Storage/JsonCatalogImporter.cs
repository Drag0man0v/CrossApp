using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.DTO;
 
namespace Core.Storage;
 
public sealed class JsonCatalogImporter : ICatalogImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };
 
    public async Task<IReadOnlyList<BookCopy>> LoadAsync(string path, CancellationToken ct = default)
    {
        // без цього заздалегідь скасований токен не дійшов би до await:
        // FileStream кинув би FileNotFoundException раніше
        ct.ThrowIfCancellationRequested();
 
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.Read, 4096, useAsync: true);
 
        List<JsonElement> elements;
        try
        {
            elements = await JsonSerializer.DeserializeAsync<List<JsonElement>>(
                stream, Options, ct)
                ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");
        }
        catch (JsonException ex)
        {
            throw new DomainException($"Файл {Path.GetFileName(path)} не є коректним JSON: {ex.Message}", ex);
        }
 
        var result = new List<BookCopy>(elements.Count);
        for (var i = 0; i < elements.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            result.Add(ToCopy(elements[i], i + 1));
        }
        return result;
    }
 
    private static BookCopy ToCopy(JsonElement element, int number)
    {
        try
        {
            BookDto? dto = element.Deserialize<BookDto>(Options);
            if (dto is null)
                throw new DomainException($"Запис {number}: порожнє значення.");
            return BookCopy.FromDto(dto);
        }
        catch (JsonException ex)
        {
            throw new DomainException($"Запис {number}: {ex.Message}", ex);
        }
        catch (ArgumentException ex)
        {
            throw new DomainException($"Запис {number}: {ex.Message}", ex);
        }
    }
}
