using Wyag.Core.IO;
using Wyag.Core.Objects;

namespace Wyag.Core.Commands;

public class LogCommand(IFileSystem fs, IObjectStore os, IObjectResolver or) : ICommand
{
    public string Name => "log";

    public string HelpText => "Display history of a given commit.";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var commitRef = args.Length > 0 ? args[0] : "HEAD";

        var repo = GitRepository.Find(fs)!;

        var sha = or.Find(repo, commitRef);

        Console.WriteLine("digraph wyaglog{");
        Console.WriteLine("  node[shape=rect]");
        WriteGraphviz(repo, sha, []);
        Console.WriteLine("}");

        return Task.FromResult(0);
    }

    private void WriteGraphviz(GitRepository repo, string sha, HashSet<string> seen)
    {
        if (!seen.Add(sha))
            return;

        var commit = (GitCommit)os.Read(repo, sha);

        var message = commit.Kvlm.Message.Trim()
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");

        var newlineIndex = message.IndexOf('\n');
        if (newlineIndex >= 0)
            message = message[..newlineIndex];

        Console.WriteLine($"   c_{sha} [label=\"{sha[..7]}: {message}\"]");

        if (!commit.Kvlm.Has("parent"))
            return;

        foreach (var parent in commit.Kvlm.Get("parent"))
        {
            Console.WriteLine($"  c_{sha} -> c_{parent};");
            WriteGraphviz(repo, parent, seen);
        }
    }
}
