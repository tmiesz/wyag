using Wyag.Core.Exceptions;
using Wyag.Core.IO;

namespace Wyag.Core.Commands;

public class InitCommand(IFileSystem fs) : ICommand
{
    public string Name => "init";

    public string HelpText => "Initialize a new, empty repository";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var path = args.Length > 0 ? args[0] : ".";

        try
        {
            GitRepository.Create(path, fs);
            return Task.FromResult(0);
        }
        catch (GitException ex)
        {
            Console.Error.WriteLine($"wyag: {ex.Message}");
            return Task.FromResult(1);
        }
    }
}
