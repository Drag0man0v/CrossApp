using Core.Abstractions;
using Core.Domain;
 
// обгортка для демонстрації скасування: уповільнює імпорт, щоб встигнути натиснути Ctrl+C
internal sealed class DelayedImporter(ICatalogImporter inner, int delayMs) : ICatalogImporter
{
    public async Task<IReadOnlyList<BookCopy>> LoadAsync(string path, CancellationToken ct = default)
    {
        await Task.Delay(delayMs, ct);
        return await inner.LoadAsync(path, ct);
    }
}
