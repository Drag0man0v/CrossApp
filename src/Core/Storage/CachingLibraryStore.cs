using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingLibraryStore : ILibraryStore
{
    private readonly ILibraryStore _inner;

    private readonly Dictionary<string, BookCopy?> _bookById;
    private readonly Dictionary<string, Loan?> _loanById;

    private IReadOnlyList<BookCopy>? _books;
    private IReadOnlyList<Loan>? _loans;

    public int Hits { get; private set; }

    public CachingLibraryStore(ILibraryStore inner)
    {
        if (inner == null)
        {
            throw new ArgumentNullException(nameof(inner));
        }

        _inner = inner;

        _bookById = new Dictionary<string, BookCopy?>(
            StringComparer.OrdinalIgnoreCase
        );

        _loanById = new Dictionary<string, Loan?>(
            StringComparer.OrdinalIgnoreCase
        );
    }

    public IReadOnlyList<BookCopy> ListBooks()
    {
        if (_books != null)
        {
            Hits++;
            return _books;
        }

        _books = _inner.ListBooks();
        return _books;
    }

    public BookCopy? GetBookById(string id)
    {
        if (_bookById.TryGetValue(id, out BookCopy? cached))
        {
            Hits++;
            return cached;
        }

        BookCopy? book = _inner.GetBookById(id);
        _bookById[id] = book;
        return book;
    }

    public void AddBook(BookCopy book)
    {
        _inner.AddBook(book);
        Invalidate();
    }

    public IReadOnlyList<Loan> ListLoans()
    {
        if (_loans != null)
        {
            Hits++;
            return _loans;
        }

        _loans = _inner.ListLoans();
        return _loans;
    }

    public Loan? GetLoanById(string id)
    {
        if (_loanById.TryGetValue(id, out Loan? cached))
        {
            Hits++;
            return cached;
        }

        Loan? loan = _inner.GetLoanById(id);
        _loanById[id] = loan;
        return loan;
    }

    public void AddLoan(Loan loan)
    {
        _inner.AddLoan(loan);
        Invalidate();
    }

    public void SaveChanges()
    {
        _inner.SaveChanges();
        Invalidate();
    }

    private void Invalidate()
    {
        _books = null;
        _loans = null;
        _bookById.Clear();
        _loanById.Clear();
    }
}