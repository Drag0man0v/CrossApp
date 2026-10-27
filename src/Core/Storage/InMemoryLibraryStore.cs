using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryLibraryStore : ILibraryStore
{
    private readonly Dictionary<string, BookCopy> _books;

    private readonly Dictionary<string, Loan> _loans;

    public InMemoryLibraryStore(IEnumerable<BookCopy>? initialBooks = null)
    {
        _books = new Dictionary<string, BookCopy>(
            StringComparer.OrdinalIgnoreCase
        );

        _loans = new Dictionary<string, Loan>(
            StringComparer.OrdinalIgnoreCase
        );

        if (initialBooks != null)
        {
            foreach (BookCopy book in initialBooks)
            {
                AddBook(book);
            }
        }
    }

    public IReadOnlyList<BookCopy> ListBooks()
    {
        return _books.Values.ToList();
    }

    public BookCopy? GetBookById(string id)
    {
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

        if (_books.ContainsKey(book.Id))
        {
            throw new InvalidOperationException(
                "Примірник з ID " + book.Id + " уже існує."
            );
        }

        _books.Add(book.Id, book);
    }

    public IReadOnlyList<Loan> ListLoans()
    {
        return _loans.Values.ToList();
    }

    public Loan? GetLoanById(string id)
    {
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

        if (_loans.ContainsKey(loan.Id))
        {
            throw new InvalidOperationException(
                "Видача з ID " + loan.Id + " уже існує."
            );
        }

        _loans.Add(loan.Id, loan);
    }

    public void SaveChanges()
    {
        // Дані вже зберігаються в пам'яті.
    }
}