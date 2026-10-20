
using Core.Domain;
using Core.DTO;
using Core.Import;


Console.WriteLine("Завдання 1: Імпорт даних");

string path = "data/sample.csv";

try
{
    ImportResult<BookDto> imported = BookCsvImporter.Load(path);

    DomainImportResult result = CopyMapper.FromImportResult(imported);

    Console.WriteLine("Імпортовано книг: " + result.Copies.Count);
    Console.WriteLine("Кількість помилок: " + result.Errors.Count);

    Console.WriteLine("\nСписок книг:");

    foreach (BookCopy book in result.Copies)
    {
        Console.WriteLine(book);
    }

    Console.WriteLine("\nПомилки:");

    foreach (string error in result.Errors)
    {
        Console.WriteLine(error);
    }
}
catch (Exception ex)
{
    Console.WriteLine("Помилка читання файлу: " + ex.Message);
}


Console.WriteLine("\nЗавдання 2: Ліміт видач");

LoanService service = new LoanService();

for (int i = 1; i <= 5; i++)
{
    BookCopy copy = BookCopy.Create(
        "C-" + i,
        "978-" + i,
        "Книга " + i,
        "Автор",
        2000 + i,
        "Проза"
    );

    Loan loan = service.IssueCopy(
        "L-" + i,
        copy,
        "R-001",
        new DateTime(2026, 10, 1)
    );

    Console.WriteLine("Створено видачу: " + loan);
}

Console.WriteLine("Відкритих видач: " + service.Loans.Count);

try
{
    BookCopy copy6 = BookCopy.Create(
        "C-6",
        "978-6",
        "Книга 6",
        "Автор",
        2006,
        "Проза"
    );

    service.IssueCopy(
        "L-6",
        copy6,
        "R-001",
        new DateTime(2026, 10, 2)
    );
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("Шоста видача заборонена: " + ex.Message);
}


Console.WriteLine("\nЗавдання 3: Статуси видач");

// Створюємо першу книгу
BookCopy book1 = BookCopy.Create(
    "C-10",
    "978-10",
    "Кобзар",
    "Т. Шевченко",
    1840,
    "Поезія"
);

// Відкриваємо видачу
Loan loan1 = Loan.Open(
    "L-10",
    book1,
    "R-002",
    new DateTime(2026, 10, 1)
);

Console.WriteLine("Після видачі: " + loan1);

// Повертаємо книгу
loan1.Close(new DateTime(2026, 10, 15));

Console.WriteLine("Після повернення: " + loan1);

// Пробуємо закрити повторно
try
{
    loan1.Close(new DateTime(2026, 10, 16));
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("Повторне закриття заборонено: " + ex.Message);
}


// Створюємо другу книгу
BookCopy book2 = BookCopy.Create(
    "C-11",
    "978-11",
    "Лісова пісня",
    "Леся Українка",
    1911,
    "Драма"
);

// Відкриваємо другу видачу
Loan loan2 = Loan.Open(
    "L-11",
    book2,
    "R-003",
    new DateTime(2026, 10, 3)
);

Console.WriteLine("\nНова видача: " + loan2);

// Позначаємо книгу втраченою
loan2.MarkLost();

Console.WriteLine("Після втрати: " + loan2);

// Пробуємо повернути втрачену книгу
try
{
    loan2.Close(new DateTime(2026, 10, 20));
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("Повернення втраченої книги заборонено: " + ex.Message);
}
