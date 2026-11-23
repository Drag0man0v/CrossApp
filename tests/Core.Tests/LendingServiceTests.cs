using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;
 
namespace Core.Tests;
 
public class LendingServiceTests
{
    [Fact] // 1) бізнес-правило: відмова і незмінний стан
    public void IssueCopy_WhenAlreadyIssued_ThrowsDomainException_AndKeepsState()
    {
        var store = new InMemoryLibraryStore();
        var service = new LendingService(store);
        BookCopy book = service.AddBook("978-1", "Кобзар", "Шевченко", 1840, "Поезія");
        service.IssueCopy(book.Id, "R-1", DateTime.Today);
 
        Assert.Throws<DomainException>(() => service.IssueCopy(book.Id, "R-2", DateTime.Today));
 
        Assert.Single(store.ListLoans());
        Assert.True(book.IsIssued);
    }
 
    [Fact] // 2) повернення невиданого примірника
    public void Return_NotIssuedCopy_ThrowsDomainException()
    {
        BookCopy copy = TestData.Items()[0];
 
        Assert.Throws<DomainException>(() => copy.Return());
        Assert.False(copy.IsIssued);
    }
 
    [Fact] // 3) некоректний аргумент - це ArgumentException, а не DomainException
    public void Create_WithEmptyIsbn_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => BookCopy.Create("T-9", " ", "Назва", null, 2000, "Роман"));
    }
 
    [Fact] // 4) подія як спосіб перевірки
    public void AddBook_RaisesChangedExactlyOnce()
    {
        var service = new LendingService(new InMemoryLibraryStore());
        var events = new List<LibraryChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);
 
        service.AddBook("978-2", "Гвинт", "Автор", 2000, "Проза");
 
        Assert.Single(events);
        Assert.Equal(LibraryChangeKind.BookAdded, events[0].Kind);
    }
}
