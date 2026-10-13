namespace Core.DTO;

public sealed record ImportResult<T>(
  IReadOnlyList<T> Items,
  IReadOnlyList<string> Errors);