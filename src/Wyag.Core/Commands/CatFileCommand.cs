using Wyag.Core.IO;
using Wyag.Core.Objects;

namespace Wyag.Core.Commands;

public sealed class CatFileCommand(IFileSystem fs, IObjectStore os, IObjectResolver or) : ICommand
{
    private static readonly HashSet<string> ValidTypes = ["blob", "commit", "tag", "tree"];

    public string Name => "cat-file";

    public string HelpText => "Provide content of repository objects";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 2 || !ValidTypes.Contains(args[0]))
        {
            Console.Error.WriteLine("usage: wyag cat-file (blob|commit|tag|tree) OBJEC");
            return Task.FromResult(1);
        }

        var format = args[0];
        var objectName = args[1];

        var repo = GitRepository.Find(fs)!;

        var sha = or.Find(repo, objectName, format);
        var obj = os.Read(repo, sha);

        using var stdout = Console.OpenStandardOutput();
        var data = obj.Serialize();
        stdout.Write(data, 0, data.Length);

        return Task.FromResult(0);
    }
}

