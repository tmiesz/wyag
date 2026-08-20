using Wyag.Core.IO;
using Wyag.Core.Objects;

namespace Wyag.Core.Commands;

public sealed class CommitCommand(IFileSystem fs, ICommitService cs) : ICommand
{
    public string Name => "commit";

    public string HelpText => "Record changes to the repository.";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        string? message = null;

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "-m" && i + 1 < args.Length)
                message = args[++i];
        }

        if (message is null)
        {
            Console.Error.WriteLine("usage: wyag commit -m MESSAGE");
            return Task.FromResult(1);
        }

        var repo = GitRepository.Find(fs)!;

        var sha = cs.Commit(repo, message);
        Console.WriteLine(sha);

        return Task.FromResult(0);
    }
}
