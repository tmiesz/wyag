using Wyag.Core.Index;
using Wyag.Core.IO;

namespace Wyag.Core.Commands;

public sealed class AddCommand(IFileSystem fs, IStagingService ss) : ICommand
{
    public string Name => "add";

    public string HelpText => "Add file contents to the index";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("usage: wyag add FILE");
            return Task.FromResult(1);
        }

        var repo = GitRepository.Find(fs)!;

        ss.Add(repo, args);
        return Task.FromResult(0);
    }
}
