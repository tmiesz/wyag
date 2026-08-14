using System.Security.Cryptography;
using System.Text;
using Wyag.Core.Compression;
using Wyag.Core.Exceptions;
using Wyag.Core.IO;

namespace Wyag.Core.Objects;

// <summary>
// Reads and writes Git's loose objects.
// </summary>
public interface IObjectStore
{
    GitObject Read(GitRepository repo, string sha);
    string Write(GitObject obj, GitRepository? repo);
    string Hash(Stream input, string format, GitRepository? repo);
}

public sealed class ObjectStore(IFileSystem fs) : IObjectStore
{
    private static readonly Dictionary<string, Func<byte[], GitObject>> ObjectConstructors = new()
    {
        ["blob"] = data => new GitBlob(data),
        ["commit"] = data => new GitCommit(data),
    };

    public string Hash(Stream input, string format, GitRepository? repo)
    {
        using var buffer = new MemoryStream();
        input.CopyTo(buffer);
        var data = buffer.ToArray();

        if (!ObjectConstructors.TryGetValue(format, out var construct))
            throw new GitException($"Unknown type {format}");

        var obj = construct(data);
        return Write(obj, repo);
    }

    public GitObject Read(GitRepository repo, string sha)
    {
        var path = repo.Path("objects", sha[..2], sha[2..]);

        if (!fs.FileExists(path))
            throw new GitException($"No such object {sha}");

        var raw = Zlib.Decompress(fs.ReadBytes(path));

        var spaceIndex = Array.IndexOf(raw, (byte)' ');
        var format = Encoding.ASCII.GetString(raw, 0, spaceIndex);

        var nullIndex = Array.IndexOf(raw, (byte)0, spaceIndex);
        var sizeText = Encoding.ASCII.GetString(raw, spaceIndex + 1, nullIndex - spaceIndex);
        var declaredSize = int.Parse(sizeText);
        var actualSize = raw.Length - nullIndex - 1;

        if (declaredSize != actualSize)
            throw new GitException($"Malformed object {sha}: bad length");

        if (!ObjectConstructors.TryGetValue(format, out var construct))
            throw new GitException($"Unknown type {format} for object {sha}");

        var content = raw[(nullIndex + 1)..];
        return construct(content);
    }

    public string Write(GitObject obj, GitRepository? repo)
    {
        var data = obj.Serialize();
        var header = Encoding.ASCII.GetBytes($"{obj.Format} {data.Length}\0");
        var result = new byte[header.Length + data.Length];
        header.CopyTo(result, 0);
        data.CopyTo(result, header.Length);

        var sha = Convert.ToHexString(SHA1.HashData(result)).ToLowerInvariant();

        if (repo is not null)
        {
            var path = repo.File(mkdir: true, "objects", sha[..2], sha[2..])
                ?? throw new GitException($"Could not create object path for {sha}");

            if (!fs.FileExists(path))
                fs.WriteBytes(path, Zlib.Compress(result));
        }

        return sha;
    }
}
