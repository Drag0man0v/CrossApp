using Core.DTO;

namespace Core.Domain;

public sealed class BookCopy
{
    private bool _isIssued;

    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public string? Author { get; }
    public int Year { get; }
    public string Genre { get; }
    public bool IsIssued
    {
        get { return _isIssued; }
    }

    private BookCopy(string id, string isbn, string title, string? author,
                     int year, string genre)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        Author = author;
        Year = year;
        Genre = genre;
        
    }

    public static BookCopy Create(string id, string isbn, string title,
                                  string? author, int year, string genre)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва книги не може бути порожньою", nameof(title));
        if (string.IsNullOrWhiteSpace(genre))
            throw new ArgumentException("Жанр не може бути порожнім", nameof(genre));

        if (year <= 0)
            throw new ArgumentOutOfRangeException(nameof(year), year,
                "Рік видання має бути додатним числом");

        int maxYear = DateTime.UtcNow.Year;
        if (year > maxYear)
            throw new ArgumentOutOfRangeException(nameof(year), year,
                $"Рік видання має бути до {maxYear}");

        // Автор необов'язковий: порожній рядок вважаємо "автора немає".
        string? normalizedAuthor = string.IsNullOrWhiteSpace(author) ? null : author.Trim();

        return new BookCopy(id.Trim(), isbn.Trim(), title.Trim(),
                            normalizedAuthor, year, genre.Trim());
    }

    public void Issue()
    {
        if (IsIssued)
            throw new InvalidOperationException(
                $"Примірник {Id} вже виданий, повторна видача неможлива");

        _isIssued = true;
    }

    public void Return()
    {
        if (!IsIssued)
            throw new InvalidOperationException(
                $"Примірник {Id} не був виданий, повернення неможливе");

        _isIssued = false;
    }

    public BookDto ToDto()
    {
        BookDto dto = new BookDto(Id, Isbn, Title, Author, Year, Genre, IsIssued);
        return dto;
    }

    public static BookCopy FromDto(BookDto dto)
    {
        BookCopy copy = Create(
            dto.Id,
            dto.Isbn,
            dto.Title,
            dto.Author,
            dto.Year,
            dto.Genre);
            
            if (dto.IsIssued)
            {
            copy.Issue();
            }

        return copy;
    }

    public override string ToString() =>
        $"{Id} [{Isbn}] {Title} — {(IsIssued ? "видано" : "на полиці")}";
}