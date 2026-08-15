namespace Wyag.Core.Refs;

public sealed class RefPrinter
{
    public static void Print(SortedDictionary<string, object> refs, bool withHash, string prefix = "")
    {
        var fullPrefix = prefix.Length > 0 ? prefix + "/" : prefix;

        foreach (var (name, value) in refs)
        {
            if (value is string sha)
            {
                Console.WriteLine(withHash ? $"{sha} {fullPrefix}{name}" : $"{fullPrefix}{name}");
            }
            else
            {
                Print((SortedDictionary<string, object>)value, withHash, $"{fullPrefix}{name}");
            }
        }
    }
}
