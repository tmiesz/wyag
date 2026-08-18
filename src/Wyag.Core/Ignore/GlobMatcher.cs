using System.Text;
using System.Text.RegularExpressions;

namespace Wyag.Core.Ignore;

public static class GlobMatcher
{
    public static bool IsMatch(string path, string pattern) => BuildRegex(pattern).IsMatch(path);

    private static Regex BuildRegex(string pattern)
    {
        var builder = new StringBuilder("^");
        var i = 0;

        while (i < pattern.Length)
        {
            var c = pattern[i++];

            switch (c)
            {
                case '*':
                    builder.Append(".*");
                    break;

                case '?':
                    builder.Append('.');
                    break;

                case '[':
                    var end = i;
                    if (end < pattern.Length && pattern[end] == '!') end++;
                    if (end < pattern.Length && pattern[end] == ']') end++;
                    while (end < pattern.Length && pattern[end] != ']') end++;

                    if (end >= pattern.Length)
                    {
                        builder.Append("\\[");
                    }
                    else
                    {
                        var setContent = pattern[i..end];
                        if (setContent.StartsWith('!'))
                            setContent = "^" + setContent[1..];
                        builder.Append('[').Append(setContent).Append(']');
                        i = end + 1;
                    }
                    break;

                default:
                    builder.Append(Regex.Escape(c.ToString()));
                    break;
            }
        }

        builder.Append('$');
        return new Regex(builder.ToString(), RegexOptions.Singleline);
    }
}
