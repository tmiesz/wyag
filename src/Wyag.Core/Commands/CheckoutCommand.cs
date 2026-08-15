using Wyag.Core.Exceptions;
using Wyag.Core.IO;
using Wyag.Core.Objects;

namespace Wyag.Core.Commands;

public sealed class CheckoutCommand(IFileSystem fs, IObjectStore os, IObjectResolver or)
    : ICommand
{
    public string Name => "checkout";

    public string HelpText => "Checkout a commit inside of a directory";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: wyag checkout COMMIT PATH");
            return Task.FromResult(1);
        }

        var commitRef = args[0];
        var targetPath = args[1];

        var repo = GitRepository.Find(fs)!;

        var sha = or.Find(repo, commitRef);
        GitObject obj = os.Read(repo, sha);

        if (obj is GitCommit commit)
        {
            var treeSha = commit.Kvlm.GetSingle("tree")
                ?? throw new GitException($"Commit {sha} has no tree");
            obj = os.Read(repo, treeSha);
        }

        EnsureEmptyDirectory(targetPath);

        CheckoutTree(repo, (GitTree)obj, fs.GetFullPath(targetPath));
        return Task.FromResult(0);
    }

    private void EnsureEmptyDirectory(string path)
    {
        if (fs.DirectoryExists(path))
        {
            if (fs.EnumerateFiles(path).Any() || fs.EnumerateDirectories(path).Any())
                throw new GitException($"Not empty {path}!");
        }
        else if (fs.FileExists(path))
        {
            throw new GitException($"Not a directory {path}!");
        }
        else
        {
            fs.CreateDirectory(path);
        }
    }

    private void CheckoutTree(GitRepository repo, GitTree tree, string path)
    {
        foreach (var item in tree.Items)
        {
            var obj = os.Read(repo, item.Sha);
            var dest = fs.Combine(path, item.Path);

            switch (obj)
            {
                case GitTree subtree:
                    fs.CreateDirectory(dest);
                    CheckoutTree(repo, subtree, dest);
                    break;
                
                case GitBlob blob:
                    //TODO: support symlinks (mode 12****)
                    fs.WriteBytes(dest, blob.BlobData);
                    break;
            }
        }
    }
}
