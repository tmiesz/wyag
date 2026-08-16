using Wyag.Core.IO;
using Wyag.Core.Refs;

namespace Wyag.Core.Commands;

public sealed class ShowRefCommand(IFileSystem fs, IRefStore rs) : ICommand
{
    public string Name => "show-ref";

    public string HelpText => "List references.";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var repo = GitRepository.Find(fs)!;

        var refs = rs.List(repo);
        RefPrinter.Print(refs, withHash: true, prefix: "refs");

        return Task.FromResult(0);
    }
}
