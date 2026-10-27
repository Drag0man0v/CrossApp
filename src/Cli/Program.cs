using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");

string dataPath = Path.Combine(
    AppContext.BaseDirectory,
    "data",
    "library.json"
);

ILibraryStore store;

if (useFile)
{
    store = new FileLibraryStore(dataPath);
}
else
{
    store = new InMemoryLibraryStore(SampleData.Books());
}

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