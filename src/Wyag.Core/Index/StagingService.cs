using Wyag.Core.Exceptions;
using Wyag.Core.IO;
using Wyag.Core.Objects;

namespace Wyag.Core.Index;

public sealed class StagingService(IFileSystem fs, IObjectStore os, IIndexStore indexStore)
    : IStagingService
{
    public void Add(GitRepository repo, IReadOnlyList<string> paths)
    {
        var absolutePaths = paths.Select(fs.GetFullPath).ToList();

        foreach (var path in absolutePaths.Where(path => !fs.FileExists(path)))
            throw new GitException($"Not a file, or does not exist: {path}");

        Remove(repo, absolutePaths, deleteFromDisk: false, skipMissing: true);

        var index = indexStore.Read(repo);
        var entries = new List<GitIndexEntry>(index.Entries);

        foreach (var path in absolutePaths)
        {
            var sha = os.Write(new GitBlob(fs.ReadBytes(path)), repo);
            var relativeName = fs.GetRelativePath(repo.Worktree, path);
            var stat = fs.GetFileInfo(path);

            entries.Add(new GitIndexEntry(
                CTime: stat.CreationTime,
                MTime: stat.LastWriteTime,
                Dev: 0,
                Ino: 0,
                ModeType: 0b1000,
                ModePerms: Convert.ToUInt32("644", 8),
                Uid: 0,
                Gid: 0,
                FileSize: (uint)stat.Size,
                Sha: sha,
                FlagAssumeValid: false,
                FlagStage: 0,
                Name: relativeName));
        }

        indexStore.Write(repo, new GitIndex { Version = index.Version, Entries = entries });
    }

    public void Remove(GitRepository repo, IReadOnlyList<string> paths, bool deleteFromDisk = true, bool skipMissing = false)
    {
        var index = indexStore.Read(repo);

        var remaining = paths.Select(fs.GetFullPath).ToHashSet(StringComparer.Ordinal);
        var kept = new List<GitIndexEntry>();
        var toDelete = new List<string>();

        foreach (var entry in index.Entries)
        {
            var fullPath = fs.GetFullPath(fs.Combine(repo.Worktree, entry.Name));

            if (remaining.Remove(fullPath))
            {
                toDelete.Add(fullPath);
            }
            else
            {
                kept.Add(entry);
            }
        }

        if (remaining.Count > 0 && !skipMissing)
            throw new GitException($"Cannot remove paths not in the index: {string.Join(", ", remaining)}");

        if (deleteFromDisk)
        {
            foreach (var path in toDelete.Where(fs.FileExists))
                fs.DeleteFile(path);
        }

        indexStore.Write(repo, new GitIndex { Version = index.Version, Entries = kept });
    }
}
