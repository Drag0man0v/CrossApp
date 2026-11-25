using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;
using Core.Events;
using Core.Import;
using Core.Notifications;
using Core.Reports;
using System.Diagnostics;
using System.Reflection;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

bool useFile = args.Contains("--file");

string envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

AppOptions options = config.Get<AppOptions>() ?? new AppOptions();


string? legacyPath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
StorageOptions storage = new StorageOptions
{
    DataPath = string.IsNullOrWhiteSpace(legacyPath) ? options.Storage.DataPath : legacyPath,
    FileName = options.Storage.FileName
};

string dataPath = storage.ResolveFullPath(AppContext.BaseDirectory);
string? apiKey = config["Integration:ApiKey"];

void PrintConfig()
{
    Console.WriteLine($"Середовище : {envName}, рівень логу: {options.Logging.MinimumLevel}");
    Console.WriteLine($"Файл даних : {dataPath}");
    Console.WriteLine(apiKey is null ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {apiKey.Length}");
}

bool extra = args.Contains("--extra");

ILibraryStore store = extra
    ? StoreFactory.Create(args, dataPath)
    : (useFile
        ? new FileLibraryStore(dataPath)
        : new InMemoryLibraryStore(SampleData.Books()));

LendingService service = new LendingService(store, loanDays: options.Library.LoanDays);

string? command = args.FirstOrDefault(a => !a.StartsWith("--"));

if (command != null)
{
    
    void SeedLoans()
    {
        DateTime now = DateTime.Today;

        service.IssueCopy("C-001", "R-001", now.AddDays(-30)); // прострочена
        service.IssueCopy("C-002", "R-001", now.AddDays(-20)); // прострочена
        service.IssueCopy("C-003", "R-001", now.AddDays(-5));
        service.IssueCopy("C-004", "R-002", now.AddDays(-10));
        service.IssueCopy("C-005", "R-002", now.AddDays(-40)); // прострочена
        service.IssueCopy("C-006", "R-003", now.AddDays(-3));
        service.IssueCopy("C-007", "R-003", now.AddDays(-25)); // прострочена
        service.IssueCopy("C-008", "R-004", now.AddDays(-2));
        service.IssueCopy("C-012", "R-005", now.AddDays(-1));

        // уже повернені видачі
        Loan l1 = service.IssueCopy("C-009", "R-001", now.AddDays(-40));
        service.ReturnCopy(l1.Id, now.AddDays(-30));
        Loan l2 = service.IssueCopy("C-010", "R-002", now.AddDays(-35));
        service.ReturnCopy(l2.Id, now.AddDays(-28));
        Loan l3 = service.IssueCopy("C-011", "R-004", now.AddDays(-20));
        service.ReturnCopy(l3.Id, now.AddDays(-15));
    }

    if (service.AllLoans().Count == 0 && service.FindBook("C-001") != null)
    {
        SeedLoans();
    }

    var books = service.AllBooks();
    var loans = service.AllLoans();
    DateTime today = DateTime.Today;

    try
    {
    switch (command)
    {
        case "config":
            PrintConfig();
            break;

        case "export":
            PrintConfig();
            string exportPath = Path.Combine(Path.GetDirectoryName(dataPath)!, "export.json");
            int exported = BookJsonExporter.Save(books, exportPath);
            Console.WriteLine($"Експортовано {exported} записів у {exportPath}");
            Console.WriteLine($"Базова директорія: {AppContext.BaseDirectory}");
            break;

        case "report-top":
            foreach (var r in ReportService.TopReaders(loans, 5))
                Console.WriteLine($" {r.ReaderId,-8} видач: {r.Loans,3}");
            break;

        case "report-overdue":
            foreach (var o in ReportService.Overdue(loans, today, service.LoanDays))
                Console.WriteLine($" {o.CopyId,-6} {o.Title,-24} {o.ReaderId,-6} до {o.DueDate:dd.MM.yyyy}  прострочено на {o.DaysOverdue,3} дн.");
            break;

        case "report-genres":
            foreach (var g in ReportService.ByGenre(books))
                Console.WriteLine($" {g.Genre,-10} примірників: {g.Copies,3}   видано: {g.Issued,3}");
            break;

        case "report-sum":
            var t = ReportService.Totals(books);
            Console.WriteLine($" примірників {t.Copies}, назв {t.Titles}, видано {t.Issued}, середнє примірників на назву {t.AvgCopiesPerTitle:F2}");
            break;

        case "report-lazy":
            var source = books.ToList();
            var query = source.Where(b => !b.IsIssued);                 // запит побудовано, не виконано
            Console.WriteLine($"Книг на полиці до змін: {query.Count()}");
            source.Add(BookCopy.Create("tmp-001", "978-999", "Тимчасова книга", "Автор", 2020, "Проза"));
            Console.WriteLine($"Книг на полиці після змін: {query.Count()}");   // число інше
            var snapshot = query.ToList();                              // результат зафіксовано
            source.Clear();
            Console.WriteLine($"У знімку лишилось: {snapshot.Count}");   // не змінюється
            break;

        case "report-empty":
            var none = new List<BookCopy>();
            var noLoans = new List<Loan>();
            Console.WriteLine($"TopReaders: {ReportService.TopReaders(noLoans, 5).Count}, Overdue: {ReportService.Overdue(noLoans, today, service.LoanDays).Count}, ByGenre: {ReportService.ByGenre(none).Count}");
            var e0 = ReportService.Totals(none);
            Console.WriteLine($"Totals: {e0.Copies}, {e0.Titles}, {e0.Issued}, {e0.AvgCopiesPerTitle:F2}");
            break;

        case "report-extra":
            Console.WriteLine("Join: видання x видачі");
            foreach (var x in ReportService.LoanLines(books, loans))
                Console.WriteLine($" {x.Title,-24} {x.ReaderId,-6} {x.IssuedOn:dd.MM.yyyy}");

            Console.WriteLine("GroupJoin + TopBy: найчастіше видавані примірники");
            foreach (var a in ReportService.TopActiveCopies(books, loans, 3))
                Console.WriteLine($" {a.CopyId,-6} {a.Title,-24} видач: {a.Loans,2}");

            Console.WriteLine("TopBy: топ-3 жанри");
            foreach (var s in ReportService.TopGenres(books, 3))
                Console.WriteLine($" {s.Genre,-10} примірників: {s.Copies,3}");

            Console.WriteLine("Синтаксис запитів (ByGenreQuery):");
            foreach (var q in ReportService.ByGenreQuery(books))
                Console.WriteLine($" {q.Genre,-10} примірників: {q.Copies,3}   видано: {q.Issued,3}");
            break;

        case "report-bench":
            var rnd = new Random(42);
            string[] genres = { "Роман", "Повість", "Поезія", "Драма", "Казка" };
            var big = new List<BookCopy>();
            for (int i = 0; i < 100_000; i++)
            {
                big.Add(BookCopy.Create("G-" + i, "978-" + (i % 5000), "Книга " + i, null,
                    1900 + rnd.Next(0, 120), genres[rnd.Next(genres.Length)]));
            }

            ReportService.ByGenre(big); 

            var sw = Stopwatch.StartNew();
            ReportService.ByGenre(big.Where(x => x.Year > 1950));
            sw.Stop();
            Console.WriteLine($"Без ToList усередині ланцюжка: {sw.ElapsedMilliseconds} мс");

            sw.Restart();
            ReportService.ByGenre(big.Where(x => x.Year > 1950).ToList());
            sw.Stop();
            Console.WriteLine($"З ToList усередині ланцюжка:  {sw.ElapsedMilliseconds} мс");
            break;

        case "import":
        {
            string importPath = args.Where(a => !a.StartsWith("--")).Skip(1).FirstOrDefault()
                                ?? Path.Combine(AppContext.BaseDirectory, "data", "books.json");
            int importDelayMs = int.TryParse(
                args.FirstOrDefault(a => a.StartsWith("--delay="))?.Substring(8), out int importDms) ? importDms : 0;
 
            using var importCts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, ea) => { ea.Cancel = true; importCts.Cancel(); };
            // для демонстрації таймауту: importCts.CancelAfter(TimeSpan.FromSeconds(2));
 
            ICatalogImporter importSource = new JsonCatalogImporter();
            if (importDelayMs > 0) importSource = new DelayedImporter(importSource, importDelayMs);
 
            try
            {
                int importAdded = await service.ImportAsync(importSource, importPath, importCts.Token);
                Console.WriteLine($"Імпортовано записів: {importAdded}");
            }
            catch (OperationCanceledException) { Console.WriteLine("Імпорт скасовано."); }
            catch (DomainException ex) { Console.WriteLine($"Дані відхилено: {ex.Message}"); }
            catch (FileNotFoundException) { Console.WriteLine($"Файл не знайдено: {importPath}"); }
            catch (IOException ex) { Console.WriteLine($"Помилка читання файлу: {ex.Message}"); }
            break;
        }

        default:
            Console.WriteLine("config | export | import | report-top | report-overdue | report-genres | report-sum | report-lazy | report-empty | report-extra | report-bench");
            break;
    }
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine("Помилка: " + ex.Message);
    }

    return;
}

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


Console.WriteLine();
Console.WriteLine("Лабораторна 6");


string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

void OnLibraryChanged(object? sender, LibraryChangedEventArgs e)
{
    string text = e.Kind switch
    {
        LibraryChangeKind.BookAdded => "додано книгу",
        LibraryChangeKind.CopyIssued => $"видано читачу {e.ReaderId} до дати {e.DueDate:dd.MM.yyyy}",
        LibraryChangeKind.CopyReturned => $"повернено читачем {e.ReaderId}",
        _ => "невідома зміна"
    };

    string line = $"{e.At:HH:mm:ss} {e.Kind,-13} {e.CopyId} {text}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}




int changes = 0;

service.Changed += OnLibraryChanged;
service.Changed += (_, _) => changes++;


BookCopy labBook = service.AddBook("978-600", "Книга для лабораторної 6", "Автор", 2021, "Роман");
Loan labLoan = service.IssueCopy(labBook.Id, "R-002", DateTime.Now);
service.ReturnCopy(labLoan.Id, DateTime.Now);
Console.WriteLine($"Подій отримано: {changes}");


Console.WriteLine();
Console.WriteLine("Сценарій відмови:");

try
{
    service.IssueCopy("НЕІСНУЮЧИЙ-ID", "R-002", DateTime.Now);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("Помилка: " + ex.Message);
}

try
{
    service.ReturnCopy(labLoan.Id, DateTime.Now); // видача вже закрита
}
catch (DomainException ex)
{
    Console.WriteLine("Помилка: " + ex.Message);
}


Console.WriteLine($"Подій отримано: {changes} (після відмов не змінилось)");


Console.WriteLine();
service.Changed -= OnLibraryChanged;
service.IssueCopy(labBook.Id, "R-002", DateTime.Now); 
Console.WriteLine($"Подій отримано: {changes} (+1 лише від лямбди-лічильника)");
Console.WriteLine("Стан книги після тихої видачі: " + labBook);



Console.WriteLine();
Console.WriteLine("FindAll: книги на полиці, видані до 1950 року:");

foreach (BookCopy found in service.SearchBooks(p => !p.IsIssued && p.Year < 1950))
{
    Console.WriteLine(found);
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


        Console.WriteLine();
    Console.WriteLine("Додаткове завдання 6: INotificationSink");

    string sinkPath = Path.Combine(AppContext.BaseDirectory, "logs", "sink.log");
    Directory.CreateDirectory(Path.GetDirectoryName(sinkPath)!);

    LendingService consoleService = new LendingService(new InMemoryLibraryStore(), new ConsoleSink());
    consoleService.AddBook("978-700", "Через ConsoleSink", "Автор", 2020, "Проза");

    LendingService fileService = new LendingService(new InMemoryLibraryStore(), new FileSink(sinkPath));
    fileService.AddBook("978-701", "Через FileSink", "Автор", 2020, "Проза");
    Console.WriteLine("FileSink записав у " + sinkPath);

    Console.WriteLine();
    Console.WriteLine("Додаткове завдання 6: Action<string> для імпорту");

    int skipped = 0;
    BookCsvImporter.Load(
        Path.Combine(AppContext.BaseDirectory, "data", "sample.csv"),
        message =>
        {
            skipped++;
            Console.WriteLine("Пропущено: " + message);
        });
    Console.WriteLine("Пропущених рядків: " + skipped);
}