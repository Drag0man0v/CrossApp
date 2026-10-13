using Core.DTO;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

if (Path.GetExtension(path).ToLowerInvariant() == ".csv" && args.Length > 1 && args[1] == "mixed")
{
    MixedImportResult mixed = MixedCsvImporter.Load(path);

    Console.WriteLine($"Книг: {mixed.Books.Count}, читачів: {mixed.Readers.Count}");
    foreach (BookDto b in mixed.Books.Take(3))
        Console.WriteLine($" {b.Id,-6} {b.Isbn,-18} {b.Title,-24} {b.Year,5} {b.Genre}");
    foreach (ReaderDto r in mixed.Readers.Take(3))
        Console.WriteLine($" {r.Id,-6} {r.Name,-24} {r.Email}");

    PrintErrors(mixed.Errors);
    
    Console.WriteLine(ImportStats.FromCounts(mixed.Books.Count + mixed.Readers.Count, mixed.Errors.Count));
    return 0;   
}

ImportResult<BookDto> result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => BookCsvImporter.Load(path),
    ".json" => BookJsonImporter.Load(path),
    _ => new ImportResult<BookDto>([], [$"непідтримуваний формат файлу '{Path.GetExtension(path)}'"])
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (BookDto b in result.Items.Take(5))
    Console.WriteLine($" {b.Id,-6} {b.Isbn,-18} {b.Title,-24} {b.Year,5} {b.Genre}");

PrintErrors(result.Errors);
Console.WriteLine(ImportStats.From(result));


return 0;

static void PrintErrors(IReadOnlyList<string> errors)
{
    if (errors.Count == 0)
        return;

    Console.WriteLine($"Пропущено: {errors.Count}");
    foreach (string e in errors)
        Console.WriteLine($" ! {e}");
}