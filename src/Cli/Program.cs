using Core.Domain;
using Core.DTO;
using Core.Import;


Console.WriteLine("Успішний сценарій");

BookCopy copy = BookCopy.Create(
    "C-001", "978-966-03-1234-5", "Кобзар",
    "Т. Шевченко", 1840, "Поезія");

Console.WriteLine(copy);

Loan loan = Loan.Open(
    "L-001", copy, "R-001", new DateTime(2026, 10, 1));

Console.WriteLine(copy);
Console.WriteLine(loan);

Console.WriteLine("\nПорушення інваріантів");

TryDo("Порожній ISBN", () =>
    BookCopy.Create("C-002", "", "Назва", null, 2000, "Проза"));

TryDo("Порожній жанр", () =>
    BookCopy.Create("C-003", "978-3", "Назва", null, 2000, ""));

TryDo("Від'ємний рік", () =>
    BookCopy.Create("C-004", "978-4", "Назва", null, -5, "Проза"));

TryDo("Порожній читач", () =>
    Loan.Open("L-004",
        BookCopy.Create("C-005", "978-5", "Назва", null, 2000, "Проза"),
        " ", new DateTime(2026, 10, 2)));

TryDo("FromDto: порожній ISBN", () =>
    BookCopy.FromDto(new BookDto("C-006", "", "Назва", null, 2000, "Проза")));

TryDo("Повторна видача", () =>
    Loan.Open("L-002", copy, "R-002", new DateTime(2026, 10, 2)));

TryDo("Повернення раніше видачі", () =>
    loan.Close(new DateTime(2026, 9, 30)));

Console.WriteLine("\nСтан після відмов - примірник досі виданий:");
Console.WriteLine(copy);
Console.WriteLine(loan);

Console.WriteLine("\nУспішне повернення");

loan.Close(new DateTime(2026, 10, 15));

Console.WriteLine(copy);
Console.WriteLine(loan);

TryDo("Повторне закриття", () =>
    loan.Close(new DateTime(2026, 10, 16)));

Console.WriteLine("\nПеретворення в DTO і назад");

var dto = copy.ToDto();
BookCopy restored = BookCopy.FromDto(dto);

Console.WriteLine(restored);

// copy.IsIssued = false;


Console.WriteLine("\nДодаткове 1: Імпорт даних");

string path = "data/sample.csv";

try
{
    ImportResult<BookDto> imported = BookCsvImporter.Load(path);
    DomainImportResult result = CopyMapper.FromImportResult(imported);

    Console.WriteLine("Імпортовано книг: " + result.Copies.Count);
    Console.WriteLine("Кількість помилок: " + result.Errors.Count);

    Console.WriteLine("\nСписок книг:");
    foreach (BookCopy book in result.Copies)
        Console.WriteLine(book);

    Console.WriteLine("\nПомилки:");
    foreach (string error in result.Errors)
        Console.WriteLine(error);
}
catch (Exception ex)
{
    Console.WriteLine("Помилка читання файлу: " + ex.Message);
}

Console.WriteLine("\nДодаткове 2: Ліміт видач");

LoanService service = new LoanService();

for (int i = 1; i <= 5; i++)
{
    BookCopy c = BookCopy.Create(
        "C-" + i, "978-" + i, "Книга " + i, "Автор", 2000 + i, "Проза");

    Loan l = service.IssueCopy("L-" + i, c, "R-001", new DateTime(2026, 10, 1));
    Console.WriteLine("Створено видачу: " + l);
}

Console.WriteLine("Відкритих видач: " + service.Loans.Count);

TryDo("Шоста видача читачу", () =>
    service.IssueCopy(
        "L-6",
        BookCopy.Create("C-6", "978-6", "Книга 6", "Автор", 2006, "Проза"),
        "R-001",
        new DateTime(2026, 10, 2)));

Console.WriteLine("\n=== Додаткове 3: Статуси видач ===");

BookCopy book1 = BookCopy.Create(
    "C-10", "978-10", "Кобзар", "Т. Шевченко", 1840, "Поезія");

Loan loan1 = Loan.Open("L-10", book1, "R-002", new DateTime(2026, 10, 1));
Console.WriteLine("Після видачі: " + loan1);

loan1.Close(new DateTime(2026, 10, 15));
Console.WriteLine("Після повернення: " + loan1);

TryDo("Повторне закриття", () =>
    loan1.Close(new DateTime(2026, 10, 16)));

BookCopy book2 = BookCopy.Create(
    "C-11", "978-11", "Лісова пісня", "Леся Українка", 1911, "Драма");

Loan loan2 = Loan.Open("L-11", book2, "R-003", new DateTime(2026, 10, 3));
Console.WriteLine("\nНова видача: " + loan2);

loan2.MarkLost();
Console.WriteLine("Після втрати: " + loan2);

TryDo("Повернення втраченої книги", () =>
    loan2.Close(new DateTime(2026, 10, 20)));

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"{title}: очікуваний виняток не виник!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{title}: {ex.GetType().Name} - {ex.Message}");
    }
}