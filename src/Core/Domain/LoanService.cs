
namespace Core.Domain;

public sealed class LoanService
{
    private const int MaxOpenLoans = 5;

    private readonly List<Loan> _loans = new List<Loan>();

    public IReadOnlyList<Loan> Loans
    {
        get { return _loans.AsReadOnly(); }
    }

    public Loan IssueCopy(string loanId, BookCopy copy, string readerId, DateTime on)
    {
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Читач не вказаний");

        string reader = readerId.Trim();
        int count = 0;

        foreach (Loan loan in _loans)
        {
            if (loan.ReaderId == reader && loan.Status == LoanStatus.Open)
            {
                count++;
            }
        }

        if (count >= MaxOpenLoans)
        {
            throw new InvalidOperationException("Читач уже має 5 відкритих видач");
        }

        Loan newLoan = Loan.Open(loanId, copy, reader, on);

        _loans.Add(newLoan);

        return newLoan;
    }
}
