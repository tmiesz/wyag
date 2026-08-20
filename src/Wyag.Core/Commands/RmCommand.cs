using Wyag.Core.Index;
using Wyag.Core.IO;

namespace Wyag.Core.Commands;

public sealed class RmCommand(IFileSystem fs, IStagingService ss) : ICommand
{
    public string Name => "rm";

    public string HelpText => "Remove files from the working tree and the index.";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("usage: wyag rm FILE");
            return Task.FromResult(1);
        }

        var repo = GitRepository.Find(fs)!;

        ss.Remove(repo, args);
        return Task.FromResult(0);
    }
}
