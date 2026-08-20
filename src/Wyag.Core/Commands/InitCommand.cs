using Wyag.Core.IO;

namespace Wyag.Core.Commands;

public class InitCommand(IFileSystem fs) : ICommand
{
    public string Name => "init";

    public string HelpText => "Initialize a new, empty repository";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var path = args.Length > 0 ? args[0] : ".";

        GitRepository.Create(path, fs);
        return Task.FromResult(0);
    }
}
