namespace Wyag.Core.Commands;

public sealed class NotYetImplemented(string name) : ICommand
{
    public string Name => name;

    public string HelpText => $"'{Name}' is not yet implemented.";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        Console.Error.WriteLine($"wyag: '{Name}' is not yet implemented.");
        return Task.FromResult(1);
    }
}
