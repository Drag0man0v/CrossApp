namespace Core.DTO;

public record BookDto(
    string Id,
    string Isbn,
    string Title,
    string? Author,
    int Year,
    string Genre);