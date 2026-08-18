using Wyag.Core.Ignore;
using Wyag.Core.IO;

namespace Wyag.Core.Commands;

public sealed class CheckIgnoreCommand(IFileSystem fs, IGitIgnoreService gitIgnoreService) : ICommand
{
    public string Name => "check-ignore";

    public string HelpText => "Check path(s) against ignore rules.";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var repo = GitRepository.Find(fs)!;

        var rules = gitIgnoreService.Load(repo);

        foreach (var path in args)
        {
            if (gitIgnoreService.IsIgnored(rules, path))
                Console.WriteLine(path);
        }

        return Task.FromResult(0);
    }
}
