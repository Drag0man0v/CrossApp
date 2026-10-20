
using Core.Domain;
using Core.DTO;

Console.WriteLine("Сценарій 1: успіх");

BookCopy copy = BookCopy.Create(
    "C-001", "978-966-03-1234-5", "Кобзар",
    "Т. Шевченко", 1840, "Поезія");

Console.WriteLine(copy);

Loan loan = Loan.Open(
    "L-001", copy, "R-001", new DateTime(2026, 10, 1));

Console.WriteLine(copy);
Console.WriteLine(loan);

loan.Close(new DateTime(2026, 10, 15));

Console.WriteLine(copy);
Console.WriteLine(loan);


Console.WriteLine();
Console.WriteLine("Сценарій 2: порушення інваріантів");

BookCopy copy2 = BookCopy.Create(
    "C-002", "978-966-03-9999-9", "Лісова пісня",
    "Леся Українка", 1911, "Драма");

Loan loan2 = Loan.Open(
    "L-002", copy2, "R-001", new DateTime(2026, 10, 1));

TryDo("Повторна видача", () =>
    Loan.Open("L-003", copy2, "R-002", new DateTime(2026, 10, 2)));

TryDo("Порожній ISBN", () =>
    BookCopy.Create("C-003", "", "Назва", null, 2000, "Проза"));

TryDo("Порожній жанр", () =>
    BookCopy.Create("C-003", "978-3", "Назва", null, 2000, ""));

TryDo("Від'ємний рік", () =>
    BookCopy.Create("C-004", "978-4", "Назва", null, -5, "Проза"));

TryDo("Рік у майбутньому", () =>
    BookCopy.Create("C-004", "978-4", "Назва", null, 3000, "Проза"));

TryDo("Порожній читач", () =>
    Loan.Open("L-004",
        BookCopy.Create("C-005", "978-5", "Назва", null, 2000, "Проза"),
        " ", new DateTime(2026, 10, 2)));

TryDo("Повернення раніше видачі", () =>
    loan2.Close(new DateTime(2026, 9, 1)));

BookDto badBook = new BookDto(
    "C-006", "", "Назва", null, 2000, "Проза");

TryDo("FromDto: порожній ISBN", () =>
    BookCopy.FromDto(badBook));

LoanDto badLoan = new LoanDto(
    "L-005", "C-002", "R-001",
    new DateTime(2026, 10, 10),
    new DateTime(2026, 10, 1));

TryDo("FromDto: неправильна дата", () =>
    Loan.FromDto(badLoan, copy2));

LoanDto wrongCopy = new LoanDto(
    "L-006", "C-999", "R-001",
    new DateTime(2026, 10, 10), null);

TryDo("FromDto: чужий примірник", () =>
    Loan.FromDto(wrongCopy, copy2));

Console.WriteLine("Стан після помилок:");
Console.WriteLine(copy2);
Console.WriteLine(loan2);

loan2.Close(new DateTime(2026, 10, 10));

TryDo("Повторне закриття видачі", () =>
    loan2.Close(new DateTime(2026, 10, 11)));

TryDo("Повернення книги, яка вже на полиці", () =>
    copy2.Return());


static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine(title + ": помилка не виникла!");
    }
    catch (Exception ex)
    {
        Console.WriteLine(title + ": " + ex.Message);
    }
}
