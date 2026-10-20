using Core.Domain;

namespace Core.DTO;

public record LoanDto(
    string Id,
    string CopyId,
    string ReaderId,
    DateTime IssuedOn,
    DateTime? ReturnedOn,
    LoanStatus Status);