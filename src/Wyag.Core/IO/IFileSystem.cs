namespace Wyag.Core.IO;

// <summary>
// Abstraction over file system access.
// </summary>
public interface IFileSystem
{
    bool DirectoryExists(string path);
    bool FileExists(string path);
    void CreateDirectory(string path);

    IEnumerable<string> EnumerateFiles(string path);
    IEnumerable<string> EnumerateDirectories(string path);

    Stream OpenRead(string path);
    Stream OpenWrite(string path);

    string ReadText(string path);
    void WriteText(string path, string content);

    byte[] ReadBytes(string path);
    void WriteBytes(string path, byte[] content);

    string Combine(params string[] paths);
    string GetFullPath(string path);
    string? GetFileName(string path);
    string? GetDirectoryName(string path);

    void DeleteFile(string path);
    FileStat GetFileInfo(string path);
    string GetRelativePath(string relativeTo, string path);
}

public readonly record struct FileStat(long Size, DateTimeOffset CreationTime, DateTimeOffset LastWriteTime);
