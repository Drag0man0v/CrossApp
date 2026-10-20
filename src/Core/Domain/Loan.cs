using Core.DTO;

namespace Core.Domain;

public sealed class Loan
{
    public string Id { get; }
    public BookCopy Copy { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }

    public bool IsClosed => ReturnedOn is not null;

    private Loan(string id, BookCopy copy, string readerId, DateTime issuedOn)
    {
        Id = id;
        Copy = copy;
        ReaderId = readerId;
        IssuedOn = issuedOn;
    }

    public static Loan Create(string id, BookCopy copy, string readerId, DateTime issuedOn, DateTime? returnedOn)
    {
       if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));
        ArgumentNullException.ThrowIfNull(copy);
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Читач обов'язковий", nameof(readerId));
        if (issuedOn == default)
            throw new ArgumentOutOfRangeException(nameof(issuedOn), issuedOn,
                "Дата видачі не задана");
       if (returnedOn != null && returnedOn.Value < issuedOn)
            throw new ArgumentOutOfRangeException(nameof(returnedOn), returnedOn.Value, $"Дата повернення не може бути раніше дати видачі ({issuedOn:d})");
        return new Loan(id.Trim(), copy, readerId.Trim(), issuedOn);
    }

    public static Loan Open(string id, BookCopy copy, string readerId, DateTime issuedOn)
        {
            // Create перевіряє всі аргументи ДО зміни примірника.
            var loan = Create(id, copy, readerId, issuedOn, null);
            copy.Issue();
            return loan;
        }

     public void Close(DateTime returnedOn)
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Видача {Id} вже закрита {ReturnedOn:d}");
        if (returnedOn < IssuedOn)
            throw new ArgumentOutOfRangeException(nameof(returnedOn), returnedOn,
                $"Дата повернення не може бути раніше дати видачі ({IssuedOn:d})");

        // Зміна стану в одному місці, після всіх перевірок.
        Copy.Return();
        ReturnedOn = returnedOn;
    }

    public LoanDto ToDto()
    {
    LoanDto dto = new LoanDto(
        Id,
        Copy.Id,
        ReaderId,
        IssuedOn,
        ReturnedOn
    );

    return dto;
    }

    public static Loan FromDto(LoanDto dto, BookCopy copy)
    {
    Loan loan = Create(
        dto.Id,
        copy,
        dto.ReaderId,
        dto.IssuedOn,
        dto.ReturnedOn
    );

    return loan;
    }



    public override string ToString()
{
    string status;

    if (IsClosed)
    {
        status = $"повернено {ReturnedOn:d}";
    }
    else
    {
        status = "не повернено";
    }

    return $"{Id}: примірник {Copy.Id}, читач {ReaderId}, {status}";
}


}