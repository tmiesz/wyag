using Wyag.Core.Exceptions;
using Wyag.Core.IO;
using Wyag.Core.Objects;

namespace Wyag.Core.Commands;

public sealed class LsTreeCommand(IFileSystem fs, IObjectStore os, IObjectResolver or)
    : ICommand
{
    public string Name => "ls-tree";

    public string HelpText => "Pretty-print a tree object.";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var recursive = args.Contains("-r");
        var treeRef = args.FirstOrDefault(a => a != "-r");

        if (treeRef is null)
        {
            Console.Error.WriteLine("usage: wyag ls-tree [-r] TREE");
            return Task.FromResult(1);
        }

        var repo = GitRepository.Find(fs)!;

        Print(repo, treeRef, recursive, prefix: "");
        return Task.FromResult(0);
    }

    private void Print(GitRepository repo, string reference, bool recursive, string prefix)
    {
        var sha = or.Find(repo, reference, "tree");
        var tree = (GitTree)os.Read(repo, sha);

        foreach (var item in tree.Items)
        {
            var type = item.Mode[..2] switch
            {
                "04" => "tree",
                "10" => "blob",
                "12" => "blob",
                "16" => "commit",
                _ => throw new GitException($"Weird tree leaf mode {item.Mode}")
            };

            var fullPath = fs.Combine(prefix, item.Path);

            if (recursive && type == "tree")
                Print(repo, item.Sha, recursive, fullPath);
            else
                Console.WriteLine($"{item.Mode} {type} {item.Sha}\t{fullPath}");
        }

    }
}
