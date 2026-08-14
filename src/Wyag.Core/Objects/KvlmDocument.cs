using System.Text;

namespace Wyag.Core.Objects;

/// <summary>
/// Key-value list with message
/// </summary>
public sealed class KvlmDocument
{
    private readonly List<string> _keyOrder = [];
    private readonly Dictionary<string, List<string>> _fields = new(StringComparer.Ordinal);

    public string Message { get; set; } = string.Empty;

    public void Add(string key, string value)
    {
        if (!_fields.TryGetValue(key, out var values))
        {
            values = [];
            _fields[key] = values;
            _keyOrder.Add(key);
        }
        values.Add(value);
    }

    public IReadOnlyList<string> Get(string key) =>
        _fields.TryGetValue(key, out var values) ? values : [];

    public string? GetSingle(string key) => Get(key) is { Count: > 0 } list ? list[0] : null;
    public bool Has(string key) => _fields.ContainsKey(key);
    public IReadOnlyList<string> Keys => _keyOrder;

    public static KvlmDocument Parse(byte[] raw)
    {
        var doc = new KvlmDocument();
        ParseFrom(raw, 0, doc);
        return doc;
    }

    private static void ParseFrom(byte[] raw, int start, KvlmDocument doc)
    {
        var spaceIndex = IndexOf(raw, (byte)' ', start);
        var newlineIndex = IndexOf(raw, (byte)'\n', start);

        if (spaceIndex < 0 || newlineIndex < spaceIndex)
        {
            doc.Message = Encoding.UTF8.GetString(raw, start + 1, raw.Length - start - 1);
            return;
        }

        var key = Encoding.ASCII.GetString(raw, start, spaceIndex - start);
        var end = start;
        while (true)
        {
            end = IndexOf(raw, (byte)'\n', end + 1);
            if (raw[end + 1] != (byte)' ') break;
        }

        var rawValue = Encoding.UTF8.GetString(raw, spaceIndex + 1, end - spaceIndex - 1);
        doc.Add(key, rawValue.Replace("\n ", "\n"));

        ParseFrom(raw, end + 1, doc);
    }

    private static int IndexOf(byte[] data, byte value, int start)
    {
        for (var i = start; i < data.Length; i++)
        {
            if (data[i] == value) return i;
        }
        return -1;
    }

    public byte[] Serialize()
    {
        var builder = new StringBuilder();

        foreach (var key in _keyOrder)
        {
            foreach (var value in _fields[key])
            {
                builder.Append(key).Append(' ').Append(value.Replace("\n", "\n ")).Append('\n');
            }
        }

        builder.Append('\n').Append(Message);
        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}
