namespace Core.Events;

public sealed class LibraryChangedEventArgs(
    string copyId, LibraryChangeKind kind, string? readerId, DateTime? dueDate) : EventArgs
{
    public string CopyId { get; } = copyId;
    public LibraryChangeKind Kind { get; } = kind;
    public string? ReaderId { get; } = readerId;   // null для BookAdded
    public DateTime? DueDate { get; } = dueDate;   // лише для CopyIssued
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}