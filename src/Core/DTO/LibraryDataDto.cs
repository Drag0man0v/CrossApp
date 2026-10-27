namespace Core.DTO;

public class LibraryDataDto
{
    public List<BookDto> Books { get; set; } = new List<BookDto>();

    public List<LoanDto> Loans { get; set; } = new List<LoanDto>();
}