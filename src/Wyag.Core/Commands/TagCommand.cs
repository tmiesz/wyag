using Wyag.Core.IO;
using Wyag.Core.Objects;
using Wyag.Core.Refs;

namespace Wyag.Core.Commands;

public sealed class TagCommand(IFileSystem fs, IRefStore rs, ITagService ts)
    : ICommand
{
    public string Name => "tag";

    public string HelpText => "List and create tags";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var createTagObject = args.Contains("-a");

        var positional = args.Where(a => a != "-a").ToArray();

        var repo = GitRepository.Find(fs)!;

        if (positional.Length > 0)
        {
            var name = positional[0];
            var objectRef = positional.Length > 1 ? positional[1] : "HEAD";
            ts.Create(repo, name, objectRef, createTagObject);
        }
        else
        {
            var refs = rs.List(repo);
            if (refs.TryGetValue("tags", out var tags))
                RefPrinter.Print((SortedDictionary<string, object>)tags, withHash: false);
        }

        return Task.FromResult(0);
    }
}
