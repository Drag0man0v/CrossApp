using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class LendingService
{
    private const int MaxOpenLoans = 5;

    private readonly ILibraryStore _store;

    public LendingService(ILibraryStore store)
    {
        if (store == null)
        {
            throw new ArgumentNullException(nameof(store));
        }

        _store = store;
    }

    public BookCopy AddBook(
        string isbn,
        string title,
        string? author,
        int year,
        string genre)
    {
        string id = Guid.NewGuid().ToString();

        BookCopy book = BookCopy.Create(
            id,
            isbn,
            title,
            author,
            year,
            genre
        );

        _store.AddBook(book);

        return book;
    }

    public Loan IssueCopy(
        string bookId,
        string readerId,
        DateTime issuedOn)
    {
        BookCopy? book = _store.GetBookById(bookId);

        if (book == null)
        {
            throw new InvalidOperationException(
                "Примірник з ID " + bookId + " не знайдено."
            );
        }

        if (string.IsNullOrWhiteSpace(readerId))
        {
            throw new ArgumentException("Читач не вказаний.");
        }

        string reader = readerId.Trim();

        int count = 0;

        foreach (Loan loan in _store.ListLoans())
        {
            if (loan.ReaderId == reader && loan.Status == LoanStatus.Open)
            {
                count++;
            }
        }

        if (count >= MaxOpenLoans)
        {
            throw new InvalidOperationException(
                "Читач уже має 5 відкритих видач."
            );
        }

        string loanId = Guid.NewGuid().ToString();

        Loan newLoan = Loan.Open(
            loanId,
            book,
            reader,
            issuedOn
        );

        _store.AddLoan(newLoan);

        return newLoan;
    }

    public void ReturnCopy(string loanId, DateTime returnedOn)
    {
        Loan? loan = _store.GetLoanById(loanId);

        if (loan == null)
        {
            throw new InvalidOperationException(
                "Видачу з ID " + loanId + " не знайдено."
            );
        }

        loan.Close(returnedOn);

        _store.SaveChanges();
    }

    public IReadOnlyList<BookCopy> AllBooks()
    {
        return _store.ListBooks();
    }

    public IReadOnlyList<Loan> AllLoans()
    {
        return _store.ListLoans();
    }

    public BookCopy? FindBook(string id)
    {
        return _store.GetBookById(id);
    }

    public Loan? FindLoan(string id)
    {
        return _store.GetLoanById(id);
    }
}