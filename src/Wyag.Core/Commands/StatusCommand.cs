using Wyag.Core.IO;
using Wyag.Core.Status;

namespace Wyag.Core.Commands;

public sealed class StatusCommand(IFileSystem fs, IRepositoryStatusService statusService) : ICommand
{
    public string Name => "status";

    public string HelpText => "Show the working tree status.";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var repo = GitRepository.Find(fs)!;
        var status = statusService.GetStatus(repo);

        Console.WriteLine(status.ActiveBranch is not null
                ? $"On branch {status.ActiveBranch}."
                : $"HEAD detached at {status.DetachedHeadSha}");
        Console.WriteLine();

        Console.WriteLine("Changes to be commited:");
        foreach (var change in status.StagedChanges)
            Console.WriteLine($" {Label(change.Kind)}{change.Path}");
        Console.WriteLine();

        Console.WriteLine("Changes not staged for commit:");
        foreach (var change in status.UnstagedChanges)
            Console.WriteLine($" {Label(change.Kind)}{change.Path}");
        Console.WriteLine();

        Console.WriteLine("Untracked files:");
        foreach (var path in status.UntrackedFiles)
            Console.WriteLine($" {path}");

        return Task.FromResult(0);
    }

    private static string Label(ChangeKind kind) => kind switch
    {
        ChangeKind.Modified => "modified: ",
        ChangeKind.Deleted => "deleted: ",
        ChangeKind.Added => "added: ",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
