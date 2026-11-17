namespace Core.Reports;

public sealed record ReaderLoans(string ReaderId, int Loans);

public sealed record OverdueLine(
    string CopyId, string Title, string ReaderId, DateTime DueDate, int DaysOverdue);

public sealed record GenreSummary(string Genre, int Copies, int Issued);

public sealed record BookLoanLine(string Title, string ReaderId, DateTime IssuedOn);
public sealed record BookActivity(string CopyId, string Title, int Loans);

