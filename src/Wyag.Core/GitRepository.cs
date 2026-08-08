using Wyag.Core.Config;
using Wyag.Core.Exceptions;
using Wyag.Core.IO;

namespace Wyag.Core;

public sealed class GitRepository
{
    private readonly IFileSystem _fs;

    public string Worktree { get; }
    public string GitDir { get; }
    public GitConfig Config { get; }

    public GitRepository(string path, IFileSystem fs, bool force = false)
    {
        _fs = fs;
        Worktree = path;
        GitDir = fs.Combine(path, ".git");

        if (!force && !_fs.DirectoryExists(GitDir))
            throw new GitException($"Not a Git repository {path}");

        Config = new GitConfig();

        var configPath = Path();
        if (_fs.FileExists(configPath))
        {
            using var reader = new StreamReader(_fs.OpenRead(configPath));
            Config = GitConfig.Read(reader);
        }
        else if (!force)
        {
            throw new GitException("Configuration file missing");
        }

        if (!force)
        {
            var versionText = Config.Get("core", "repositoryformatversion");
            if (versionText is null || !int.TryParse(versionText, out var version) || version != 0)
                throw new GitException($"Unsupported repositoryformatversion: {versionText}");
        }
    }

    public string Path(params string[] parts) => _fs.Combine([GitDir, .. parts]);

    public string? File(bool mkdir, params string[] parts)
    {
        if (parts.Length == 0)
            return Path();

        var dirParts = parts[..^1];
        return Dir(mkdir, dirParts) is not null ? Path(parts) : null;
    }

    public string? Dir(bool mkdir, params string[] parts)
    {
        var path = Path(parts);

        if (_fs.DirectoryExists(path))
            return path;

        if (_fs.FileExists(path))
            throw new GitException($"Not a directory {path}");

        if (mkdir)
        {
            _fs.CreateDirectory(path);
            return path;
        }

        return null;
    }

    public static GitRepository Create(string path, IFileSystem fs)
    {
        var repo = new GitRepository(path, fs, force: true);

        if (fs.DirectoryExists(repo.Worktree) || fs.FileExists(repo.Worktree))
        {
            if (!fs.DirectoryExists(repo.Worktree))
                throw new GitException($"{path} is not a directory!");

            if (fs.DirectoryExists(repo.GitDir) &&
                    (fs.EnumerateFiles(repo.GitDir).Any() || fs.EnumerateDirectories(repo.GitDir).Any()))
                throw new GitException($"{path} is not empty!");
        }
        else
        {
            fs.CreateDirectory(repo.Worktree);
        }

        _ = repo.Dir(mkdir: true, "branches")
            ?? throw new GitException("Could not create .git/branches");
        _ = repo.Dir(mkdir: true, "objects")
            ?? throw new GitException("Could not create .git/objects");
        _ = repo.Dir(mkdir: true, "refs", "tags")
            ?? throw new GitException("Could not create .git/refs/tags");
        _ = repo.Dir(mkdir: true, "refs", "tags")
            ?? throw new GitException("Could not create .git/refs/heads");

        // .git/description
        var descriptionPath = repo.File(mkdir: true, "description")
            ?? throw new GitException("Could not create .git/description");
        fs.WriteText(descriptionPath,
                "Unnamed repository; edit this file 'description' to name the repository");

        // .git/HEAD
        var headPath = repo.File(mkdir: true, "HEAD")
            ?? throw new GitException("Could not create .git/HEAD");
        fs.WriteText(headPath, "ref: refs/heads/main\n");

        // .git/config
        var configPath = repo.File(mkdir: true, "config")
            ?? throw new GitException("Could not create .git/config");
        using (var writer = new StreamWriter(fs.OpenWrite(configPath)))
        {
            DefaultConfig().Write(writer);
        }

        return repo;
    }

    public static GitConfig DefaultConfig()
    {
        var config = new GitConfig();
        config.AddSection("core");
        config.Set("core", "repositoryformatversion", "0");
        config.Set("core", "filemode", "true");
        config.Set("core", "bare", "false");
        return config;
    }

    public static GitRepository? Find(IFileSystem fs, string path = ".", bool required = true)
    {
        var fullPath = fs.GetFullPath(path);

        if (fs.DirectoryExists(fs.Combine(fullPath, ".git")))
            return new GitRepository(fullPath, fs);

        var parent = fs.GetFullPath(fs.Combine(fullPath, ".."));

        if (parent == fullPath)
        {
            if (required)
                throw new GitException("No git directory");
            return null;
        }

        return Find(fs, parent, required);
    }
}
