using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.DTO;

namespace Core.Storage;

public sealed class FileLibraryStore : ILibraryStore
{
    private readonly string _path;

    private readonly Dictionary<string, BookCopy> _books;

    private readonly Dictionary<string, Loan> _loans;

    private bool _loaded;

    public FileLibraryStore(string path)
    {
        _path = Path.GetFullPath(path);

        _books = new Dictionary<string, BookCopy>(
            StringComparer.OrdinalIgnoreCase
        );

        _loans = new Dictionary<string, Loan>(
            StringComparer.OrdinalIgnoreCase
        );

        _loaded = false;
    }

    private void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);

            LibraryDataDto? data =
                JsonSerializer.Deserialize<LibraryDataDto>(json);

            if (data != null)
            {
                foreach (BookDto bookDto in data.Books)
                {
                    BookCopy book = BookCopy.FromDto(bookDto);

                    if (_books.ContainsKey(book.Id))
                    {
                        throw new InvalidOperationException(
                            "У файлі повторюється ID книги " + book.Id
                        );
                    }

                    _books.Add(book.Id, book);
                }

                foreach (LoanDto loanDto in data.Loans)
                {
                    if (!_books.ContainsKey(loanDto.CopyId))
                    {
                        throw new InvalidOperationException(
                            "Не знайдено примірник для видачі " + loanDto.Id
                        );
                    }

                    BookCopy book = _books[loanDto.CopyId];

                    Loan loan = Loan.FromDto(loanDto, book);

                    if (_loans.ContainsKey(loan.Id))
                    {
                        throw new InvalidOperationException(
                            "У файлі повторюється ID видачі " + loan.Id
                        );
                    }

                    _loans.Add(loan.Id, loan);
                }
            }
        }

        _loaded = true;
    }

    public IReadOnlyList<BookCopy> ListBooks()
    {
        EnsureLoaded();

        return _books.Values.ToList();
    }

    public BookCopy? GetBookById(string id)
    {
        EnsureLoaded();

        if (_books.ContainsKey(id))
        {
            return _books[id];
        }

        return null;
    }

    public void AddBook(BookCopy book)
    {
        if (book == null)
        {
            throw new ArgumentNullException(nameof(book));
        }

        EnsureLoaded();

        if (_books.ContainsKey(book.Id))
        {
            throw new InvalidOperationException(
                "Примірник з ID " + book.Id + " уже існує."
            );
        }

        _books.Add(book.Id, book);

        SaveChanges();
    }

    public IReadOnlyList<Loan> ListLoans()
    {
        EnsureLoaded();

        return _loans.Values.ToList();
    }

    public Loan? GetLoanById(string id)
    {
        EnsureLoaded();

        if (_loans.ContainsKey(id))
        {
            return _loans[id];
        }

        return null;
    }

    public void AddLoan(Loan loan)
    {
        if (loan == null)
        {
            throw new ArgumentNullException(nameof(loan));
        }

        EnsureLoaded();

        if (_loans.ContainsKey(loan.Id))
        {
            throw new InvalidOperationException(
                "Видача з ID " + loan.Id + " уже існує."
            );
        }

        _loans.Add(loan.Id, loan);

        SaveChanges();
    }

    public void SaveChanges()
    {
        EnsureLoaded();

        string? directory = Path.GetDirectoryName(_path);

        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        LibraryDataDto data = new LibraryDataDto();

        foreach (BookCopy book in _books.Values)
        {
            data.Books.Add(book.ToDto());
        }

        foreach (Loan loan in _loans.Values)
        {
            data.Loans.Add(loan.ToDto());
        }

        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;

        string json = JsonSerializer.Serialize(data, options);

        File.WriteAllText(_path, json);
    }
}