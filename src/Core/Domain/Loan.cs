
using Core.DTO;

namespace Core.Domain;

public sealed class Loan
{
    public string Id { get; }
    public BookCopy Copy { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }
    public LoanStatus Status { get; private set; }

    public bool IsClosed
    {
        get { return Status == LoanStatus.Closed; }
    }

    private Loan(string id, BookCopy copy, string readerId,
        DateTime issuedOn, DateTime? returnedOn, LoanStatus status)
    {
        Id = id;
        Copy = copy;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        ReturnedOn = returnedOn;
        Status = status;
    }

    private static Loan Create(string id, BookCopy copy, string readerId,
        DateTime issuedOn, DateTime? returnedOn, LoanStatus status)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id не може бути порожнім");

        if (copy == null)
            throw new ArgumentNullException(nameof(copy));

        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("ReaderId не може бути порожнім");

        if (issuedOn == default)
            throw new ArgumentException("Дата видачі не задана");

        if (!Enum.IsDefined(status))
            throw new ArgumentException("Невідомий статус");

        if (returnedOn != null && returnedOn.Value < issuedOn)
            throw new ArgumentException("Дата повернення неправильна");

        if (status == LoanStatus.Closed && returnedOn == null)
            throw new ArgumentException("Немає дати повернення");

        if (status != LoanStatus.Closed && returnedOn != null)
            throw new ArgumentException("Неправильний статус видачі");

        return new Loan(id.Trim(), copy, readerId.Trim(),
            issuedOn, returnedOn, status);
    }

    public static Loan Open(string id, BookCopy copy,
        string readerId, DateTime issuedOn)
    {
        Loan loan = Create(id, copy, readerId,
            issuedOn, null, LoanStatus.Open);

        copy.Issue();

        return loan;
    }


public void Close(DateTime returnedOn)
{
    if (!IsAllowed(LoanStatus.Closed))
        throw new InvalidOperationException("Видачу не можна закрити");

    if (returnedOn < IssuedOn)
        throw new ArgumentException("Неправильна дата повернення");

    Copy.Return();

    ReturnedOn = returnedOn;
    Status = LoanStatus.Closed;
}
    

    public void MarkLost()
    {
    if (!IsAllowed(LoanStatus.Lost))
        throw new InvalidOperationException("Не можна позначити втраченою");

    Status = LoanStatus.Lost;
    }

    public LoanDto ToDto()
    {
        return new LoanDto(Id, Copy.Id, ReaderId,
            IssuedOn, ReturnedOn, Status);
    }

    public static Loan FromDto(LoanDto dto, BookCopy copy)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (copy == null)
            throw new ArgumentNullException(nameof(copy));

        if (copy.Id != dto.CopyId)
            throw new ArgumentException("Неправильний примірник книги");

        return Create(dto.Id, copy, dto.ReaderId,
            dto.IssuedOn, dto.ReturnedOn, dto.Status);
    }

    public override string ToString()
    {
        string result = Id + ": примірник " + Copy.Id +
            ", читач " + ReaderId + ", " + Status;

        if (ReturnedOn != null)
        {
            result += ", повернено " + ReturnedOn.Value.ToShortDateString();
        }

        return result;
    }

    
private bool IsAllowed(LoanStatus newStatus)
{
    return Status switch
    {
        LoanStatus.Open => newStatus == LoanStatus.Closed ||
                           newStatus == LoanStatus.Lost,

        LoanStatus.Closed => false,

        LoanStatus.Lost => false,

        _ => false
    };
}

}