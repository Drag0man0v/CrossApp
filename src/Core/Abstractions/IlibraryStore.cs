using Core.Domain;

namespace Core.Abstractions;

public interface ILibraryStore
{
    IReadOnlyList<BookCopy> ListBooks();

    BookCopy? GetBookById(string id);

    void AddBook(BookCopy book);

    IReadOnlyList<Loan> ListLoans();

    Loan? GetLoanById(string id);

    void AddLoan(Loan loan);

    void SaveChanges();
}