namespace Wyag.Core.Config;

/// <summary>
/// Minimal INI-style configuration document.
/// </summary>
public sealed class GitConfig
{
    private readonly Dictionary<string, Dictionary<string, string>> _sections =
        new(StringComparer.OrdinalIgnoreCase);

    public void AddSection(string name) =>
        _sections.TryAdd(name, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

    public void Set(string section, string key, string value)
    {
        if (!_sections.TryGetValue(section, out var values))
        {
            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _sections[section] = values;
        }
        values[key] = value;
    }

    public string? Get(string section, string key) =>
        _sections.TryGetValue(section, out var values) && values.TryGetValue(key, out var value)
            ? value
            : null;

    public bool HasSection(string section) => _sections.ContainsKey(section);

    public void Write(TextWriter writer)
    {
        foreach (var (section, values) in _sections)
        {
            writer.WriteLine($"[{section}]");
            foreach (var (key, value) in values)
            {
                writer.WriteLine($"\t{key} = {value}");
            }
        }
    }

    public static GitConfig Read(TextReader reader)
    {
        var config = new GitConfig();
        string? currentSection = null;
        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
                continue;

            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                currentSection = trimmed[1..^1].Trim();
                config.AddSection(currentSection);
                continue;
            }

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex < 0 || currentSection is null)
                continue;

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();

            config.Set(currentSection, key, value);
        }

        return config;
    }
}


