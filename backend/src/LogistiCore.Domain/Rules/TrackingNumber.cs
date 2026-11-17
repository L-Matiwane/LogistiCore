namespace LogistiCore.Domain.Rules;

public static class TrackingNumber
{
    /// <summary>For example FCL-20261116-000042. The date is the South African local date.</summary>
    public static string Prefix(DateTime localDate) => $"FCL-{localDate:yyyyMMdd}-";

    public static string Format(DateTime localDate, int sequence) => $"{Prefix(localDate)}{sequence:000000}";

    public static bool TryGetSequence(string trackingNo, out int sequence)
    {
        sequence = 0;
        var lastDash = trackingNo.LastIndexOf('-');
        return lastDash >= 0 && int.TryParse(trackingNo[(lastDash + 1)..], out sequence);
    }
}
