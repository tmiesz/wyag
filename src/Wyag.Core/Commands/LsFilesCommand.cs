using Wyag.Core.Index;
using Wyag.Core.IO;

namespace Wyag.Core.Commands;

public sealed class LsFilesCommand(IFileSystem fs, IIndexStore indexStore) : ICommand
{
    public string Name => "ls-files";

    public string HelpText => "List all the stage files";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var verbose = args.Contains("--verbose") || args.Contains("-v");

        var repo = GitRepository.Find(fs)!;

        var index = indexStore.Read(repo);

        if (verbose)
            Console.WriteLine($"Index file format v{index.Version}, containing {index.Entries.Count} entries.");

        foreach (var entry in index.Entries)
        {
            Console.WriteLine(entry.Name);

            if (!verbose)
                continue;

            var entryType = entry.ModeType switch
            {
                0b1000 => "regular file",
                0b1010 => "symlink",
                0b1110 => "git link",
                _ => "unknown"
            };

            Console.WriteLine($" {entryType} with perms: {Convert.ToString(entry.ModePerms, 8)}");
            Console.WriteLine($" on blob: {entry.Sha}");
            Console.WriteLine($" created: {entry.CTime}, modified: {entry.MTime}");
            Console.WriteLine($" device: {entry.Dev}, inode: {entry.Ino}");
            Console.WriteLine($" user: {entry.Uid}, group: {entry.Gid}");
            Console.WriteLine($" flags: stage={entry.FlagStage} assume_valid={entry.FlagAssumeValid}");
        }
        
        return Task.FromResult(0);
    }
}
