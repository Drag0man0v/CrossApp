using Core.Domain;
using Core.Reports;

namespace Core.Reports;


public static class ReportService
{
    // топ-N читачів
    public static IReadOnlyList<ReaderLoans> TopReaders(IEnumerable<Loan> source, int n) =>
        source.GroupBy(l => l.ReaderId, StringComparer.Ordinal)
            .Select(g => new ReaderLoans(g.Key, g.Count()))
            .OrderByDescending(r => r.Loans).ThenBy(r => r.ReaderId, StringComparer.Ordinal)
            .Take(n)
            .ToList();

    //проср*ні видачі
    public static IReadOnlyList<OverdueLine> Overdue(
        IEnumerable<Loan> source, DateTime today, int loanDays) =>
        source.Where(l => l.Status == LoanStatus.Open
                          && l.IssuedOn.AddDays(loanDays).Date < today.Date)
            .OrderBy(l => l.IssuedOn).ThenBy(l => l.Copy.Id, StringComparer.Ordinal)
            .Select(l => new OverdueLine(
                l.Copy.Id,
                l.Copy.Title,
                l.ReaderId,
                l.IssuedOn.AddDays(loanDays),
                (today.Date - l.IssuedOn.AddDays(loanDays).Date).Days))
            .ToList();

    // за жанром
    public static IReadOnlyList<GenreSummary> ByGenre(IEnumerable<BookCopy> source) =>
        source.GroupBy(b => b.Genre, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GenreSummary(g.Key, g.Count(), g.Count(b => b.IsIssued)))
            .OrderByDescending(s => s.Copies).ThenBy(s => s.Genre, StringComparer.Ordinal)
            .ToList();

    // агрегати з обробкою порожньої послідовності
    public static (int Copies, int Titles, int Issued, double AvgCopiesPerTitle) Totals(
        IEnumerable<BookCopy> source)
    {
        var list = source.ToList();

        if (list.Count == 0)
        {
            return (0, 0, 0, 0d);
        }

        var perTitle = list.GroupBy(b => b.Isbn, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Count())
            .ToList();

        return (list.Count, perTitle.Count, list.Count(b => b.IsIssued), perTitle.Average());
    }

    public static IReadOnlyList<BookLoanLine> LoanLines(
      IEnumerable<BookCopy> books, IEnumerable<Loan> loans) =>
      books.Join(loans,
            book => book.Id,
            loan => loan.Copy.Id,
            (book, loan) => new BookLoanLine(book.Title, loan.ReaderId, loan.IssuedOn))
        .OrderBy(x => x.IssuedOn)
        .ThenBy(x => x.Title, StringComparer.Ordinal)
        .ThenBy(x => x.ReaderId, StringComparer.Ordinal)
        .ToList();
     public static IReadOnlyList<BookActivity> Activity(
        IEnumerable<BookCopy> books, IEnumerable<Loan> loans) =>
        books.GroupJoin(loans,
                book => book.Id,
                loan => loan.Copy.Id,
                (book, bookLoans) => new BookActivity(book.Id, book.Title, bookLoans.Count()))
            .ToList();
      public static IReadOnlyList<BookActivity> TopActiveCopies(
        IEnumerable<BookCopy> books, IEnumerable<Loan> loans, int n) =>
        Activity(books, loans)
            .OrderBy(a => a.CopyId, StringComparer.Ordinal)   
            .TopBy(a => a.Loans, n)
            .ToList();

    public static IReadOnlyList<GenreSummary> TopGenres(IEnumerable<BookCopy> books, int n) =>
        ByGenre(books).TopBy(s => s.Copies, n).ToList();

    public static IReadOnlyList<GenreSummary> ByGenreQuery(IEnumerable<BookCopy> source) =>
        (from book in source
         group book by book.Genre into g
         orderby g.Count() descending, g.Key
         select new GenreSummary(g.Key, g.Count(), g.Count(b => b.IsIssued))
        ).ToList();

}