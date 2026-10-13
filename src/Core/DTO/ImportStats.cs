using System.Globalization;

namespace Core.DTO;

public sealed record ImportStats(int Total, int Accepted, int Skipped)
{
    public double ErrorPercent => Total == 0 ? 0 : Skipped * 100.0 / Total;
    
     public static ImportStats FromCounts(int accepted, int skipped) =>
        new(accepted + skipped, accepted, skipped);
    public static ImportStats From<T>(ImportResult<T> result) =>
        new(result.Items.Count + result.Errors.Count,
            result.Items.Count,
            result.Errors.Count);

    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture,
            $"Усього: {Total}, прийнято: {Accepted}, пропущено: {Skipped}, помилок: {ErrorPercent:F1}%");
}