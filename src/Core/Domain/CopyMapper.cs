
using Core.DTO;

namespace Core.Domain;

public class DomainImportResult
{
    public List<BookCopy> Copies { get; }
    public List<string> Errors { get; }

    public DomainImportResult(List<BookCopy> copies, List<string> errors)
    {
        Copies = copies;
        Errors = errors;
    }
}

public static class CopyMapper
{
    public static DomainImportResult FromImportResult(ImportResult<BookDto> result)
    {
        List<BookCopy> copies = new List<BookCopy>();
        List<string> errors = new List<string>();

        // Зберігаємо помилки, які вже знайшов CSV імпортер
        errors.AddRange(result.Errors);

        int row = 0;

        foreach (BookDto dto in result.Items)
        {
            row++;

            try
            {
                BookCopy copy = BookCopy.FromDto(dto);
                copies.Add(copy);
            }
            catch (ArgumentException ex)
            {
                errors.Add("Елемент " + row + ": " + ex.Message);
            }
        }

        return new DomainImportResult(copies, errors);
    }
}
