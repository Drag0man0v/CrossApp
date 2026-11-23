using Core.Domain;
 
namespace Core.Tests;
 
internal static class TestData
{
    // маленький фіксований набір: Роман x2, Поезія x1, Драма x1
    public static List<BookCopy> Items() => new()
    {
        BookCopy.Create("T-1", "978-1", "Кобзар", "Тарас Шевченко", 1840, "Поезія"),
        BookCopy.Create("T-2", "978-2", "Захар Беркут", "Іван Франко", 1883, "Роман"),
        BookCopy.Create("T-3", "978-3", "Місто", "Валер'ян Підмогильний", 1928, "Роман"),
        BookCopy.Create("T-4", "978-4", "Лісова пісня", "Леся Українка", 1911, "Драма"),
    };
 
    public const string TwoGoodRecords = """
        [
          { "id": "X-1", "isbn": "978-10", "title": "Книга один", "author": "А", "year": 2001, "genre": "Роман" },
          { "id": "X-2", "isbn": "978-11", "title": "Книга два", "author": null, "year": 2002, "genre": "Драма" }
        ]
        """;
 
    public const string SecondRecordBad = """
        [
          { "id": "Y-1", "isbn": "978-20", "title": "Нормальна", "author": "А", "year": 2001, "genre": "Роман" },
          { "id": "Y-2", "isbn": "978-21", "title": "", "author": "Б", "year": 2002, "genre": "Роман" }
        ]
        """;
 
    // тимчасовий файл, який завжди видаляється у finally
    public static async Task WithTempFileAsync(string json, Func<string, Task> action)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        await File.WriteAllTextAsync(path, json);
        try
        {
            await action(path);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
