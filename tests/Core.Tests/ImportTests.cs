using Core.Services;
using Core.Storage;
using Xunit;
 
namespace Core.Tests;
 
public class ImportTests
{
    [Fact] // 7) скасування
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new LendingService(new InMemoryLibraryStore());
 
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }
 
    [Fact] // 8) зіпсований запис: DomainException з номером запису
    public async Task ImportAsync_BadRecord_ThrowsDomainException_WithRecordNumber()
    {
        await TestData.WithTempFileAsync(TestData.SecondRecordBad, async path =>
        {
            var service = new LendingService(new InMemoryLibraryStore());
 
            var ex = await Assert.ThrowsAsync<DomainException>(
                () => service.ImportAsync(new JsonCatalogImporter(), path));
 
            Assert.Contains("Запис 2", ex.Message);
            Assert.IsType<ArgumentException>(ex.InnerException);
        });
    }
 
    [Fact] // 9) ідемпотентність і події
    public async Task ImportAsync_IsIdempotent_AndRaisesEventPerAddedRecord()
    {
        await TestData.WithTempFileAsync(TestData.TwoGoodRecords, async path =>
        {
            var service = new LendingService(new InMemoryLibraryStore());
            int events = 0;
            service.Changed += (_, _) => events++;
 
            int first = await service.ImportAsync(new JsonCatalogImporter(), path);
            int second = await service.ImportAsync(new JsonCatalogImporter(), path);
 
            Assert.Equal(2, first);
            Assert.Equal(0, second);
            Assert.Equal(2, events);
        });
    }
}
