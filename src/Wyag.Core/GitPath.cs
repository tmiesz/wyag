namespace Wyag.Core;

public static class GitPath
{
    public static string GetParentDirectory(string logicalPath)
    {
        var lastSlash = logicalPath.LastIndexOf('/');
        return lastSlash < 0 ? "" : logicalPath[..lastSlash];
    }

    public static string GetFileName(string logicalPath)
    {
        var lastSlash = logicalPath.LastIndexOf('/');
        return lastSlash < 0 ? logicalPath : logicalPath[(lastSlash + 1)..];
    }

    public static bool IsRooted(string logicalPath) => logicalPath.StartsWith('/');
}
