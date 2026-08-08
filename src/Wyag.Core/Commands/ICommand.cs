namespace Wyag.Core.Commands;

// <summary>
// Git subcommand
// </summary>
public interface ICommand
{
    string Name { get; }
    string HelpText { get; }
    Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken);
}
