using Wyag.Core.Exceptions;
using Wyag.Core.IO;
using Wyag.Core.Objects;

namespace Wyag.Core.Commands;

public sealed class RevParseCommand(IFileSystem fs, IObjectResolver or) : ICommand
{
    public string Name => "rev-parse";

    public string HelpText => "Parse revision (or other object) identifiers";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        string? format = null;
        string? name = null;

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--wyag-type")
                format = args[++i];
            else
                name = args[i];
        }

        if (name is null)
        {
            Console.Error.WriteLine("usage: wyag rev-parse [--wyag-type TYPE] NAME");
        }

        var repo = GitRepository.Find(fs)!;

        try
        {
            Console.WriteLine(or.Find(repo, name!, format));

        }
        catch (GitException)
        {
            Console.WriteLine("None");
        }

        return Task.FromResult(0);
    }
}
