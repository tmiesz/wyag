namespace Wyag.Core.Objects;

public static class GitTimestamp
{
    public static string Format(DateTimeOffset time)
    {
        var offsetMinutes = (int)time.Offset.TotalMinutes;
        var sign = offsetMinutes < 0 ? '-' : '+';
        var absoluteMinutes = Math.Abs(offsetMinutes);
        var offset = $"{sign}{absoluteMinutes / 60:D2}{absoluteMinutes % 60:D2}";
        return $"{time.ToUnixTimeSeconds()} {offset}";
    }
}
