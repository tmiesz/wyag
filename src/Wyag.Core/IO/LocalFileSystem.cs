namespace Wyag.Core.IO;

public sealed class LocalFileSystem : IFileSystem
{
    public string Combine(params string[] paths) => Path.Combine(paths);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public IEnumerable<string> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path);

    public IEnumerable<string> EnumerateFiles(string path) => Directory.EnumerateFiles(path);

    public bool FileExists(string path) => File.Exists(path);

    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

    public string GetFileName(string path) => Path.GetFileName(path);

    public string GetFullPath(string path) => Path.GetFullPath(path);

    public Stream OpenRead(string path) => File.OpenRead(path);

    public Stream OpenWrite(string path) => File.OpenWrite(path);

    public byte[] ReadBytes(string path) => File.ReadAllBytes(path);

    public string ReadText(string path) => File.ReadAllText(path);

    public void WriteBytes(string path, byte[] content) => File.WriteAllBytes(path, content);

    public void WriteText(string path, string content) => File.WriteAllText(path, content);
}
