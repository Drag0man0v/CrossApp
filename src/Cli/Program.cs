using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");

string dataPath = Path.Combine("library.json");

bool extra = args.Contains("--extra");

ILibraryStore store = extra
    ? StoreFactory.Create(args)
    : (useFile
        ? new FileLibraryStore(dataPath)
        : new InMemoryLibraryStore(SampleData.Books()));

LendingService service = new LendingService(store);

Console.WriteLine("Сховище: " + store.GetType().Name);

if (useFile)
{
    Console.WriteLine("Файл: " + dataPath);
}

Console.WriteLine();
Console.WriteLine("Книги до додавання:");

foreach (BookCopy book in service.AllBooks())
{
    Console.WriteLine(book);
}

Console.WriteLine();
Console.WriteLine("Видачі з попередніх запусків:");

foreach (Loan loan in service.AllLoans())
{
    Console.WriteLine(loan);
}

Console.WriteLine();

BookCopy newBook = service.AddBook(
    "978-100",
    "Нова книга",
    "Автор",
    2020,
    "Проза"
);

Console.WriteLine("Додано книгу:");
Console.WriteLine(newBook);

Loan newLoan = service.IssueCopy(
    newBook.Id,
    "R-001",
    DateTime.Now
);

Console.WriteLine();
Console.WriteLine("Після видачі:");
Console.WriteLine(newBook);
Console.WriteLine(newLoan);

Console.WriteLine();
Console.WriteLine("Пошук книги за ID:");

BookCopy? foundBook = service.FindBook(newBook.Id);

if (foundBook != null)
{
    Console.WriteLine(foundBook);
}
else
{
    Console.WriteLine("Книгу не знайдено.");
}

Console.WriteLine();
Console.WriteLine("Пошук видачі за ID:");

Loan? foundLoan = service.FindLoan(newLoan.Id);

if (foundLoan != null)
{
    Console.WriteLine(foundLoan);
}
else
{
    Console.WriteLine("Видачу не знайдено.");
}

service.ReturnCopy(newLoan.Id, DateTime.Now);

Console.WriteLine();
Console.WriteLine("Після повернення:");
Console.WriteLine(newBook);
Console.WriteLine(newLoan);

Console.WriteLine();
Console.WriteLine("Перевірка помилки:");

try
{
    service.IssueCopy(
        "НЕІСНУЮЧИЙ-ID",
        "R-001",
        DateTime.Now
    );
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("Помилка: " + ex.Message);
}

if (extra)
{
    Console.WriteLine();
    Console.WriteLine("Додаткове завдання");

    Console.WriteLine();
    Console.WriteLine("Пошук: жанр Роман:");

    IReadOnlyList<BookCopy> novels = service.SearchBooks(
        b => string.Equals(b.Genre, "Роман", StringComparison.OrdinalIgnoreCase)
    );

    foreach (BookCopy b in novels)
    {
        Console.WriteLine(b);
    }

    Console.WriteLine("Знайдено: " + novels.Count);

    Console.WriteLine();
    Console.WriteLine("Пошук: книги до 1900 року на полиці:");

    IReadOnlyList<BookCopy> old = service.SearchBooks(
        b => b.Year < 1900 && !b.IsIssued
    );

    foreach (BookCopy b in old)
    {
        Console.WriteLine(b);
    }

    Console.WriteLine("Знайдено: " + old.Count);

    if (store is CachingLibraryStore cache)
    {
        int before = cache.Hits;

        service.AllBooks();
        service.AllBooks();

        Console.WriteLine();
        Console.WriteLine("Звернень до кешу за два виклики AllBooks: " + (cache.Hits - before));
    }
}