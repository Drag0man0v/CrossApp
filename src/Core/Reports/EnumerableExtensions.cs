namespace Core.Reports;

public static class EnumerableExtensions
{
    public static IEnumerable<T> TopBy<T>(this IEnumerable<T> source, Func<T, int> key, int n) =>
        source.OrderByDescending(key).Take(n);
}



