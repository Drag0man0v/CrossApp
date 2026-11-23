namespace Core.Tests;
using Core.Reports;
 
public class ReportServiceTests
{
    [Theory] // 5) звіт на кількох наборах
    [InlineData("Роман", 2)]
    [InlineData("Поезія", 1)]
    public void ByGenre_CountsCopiesPerGenre(string genre, int expected)
    {
        var lines = ReportService.ByGenre(TestData.Items());
 
        Assert.Equal(expected, lines.Single(l => l.Genre == genre).Copies);
    }
 
    [Fact] // 6) детермінований топ-N (при рівності - за назвою жанру)
    public void TopGenres_IsDeterministic()
    {
        var top = ReportService.TopGenres(TestData.Items(), 2).Select(s => s.Genre);
 
        Assert.Equal(new[] { "Роман", "Драма" }, top);
    }
}
