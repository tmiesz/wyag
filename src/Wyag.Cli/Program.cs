using Microsoft.Extensions.DependencyInjection;
using Wyag.Core.Commands;
using Wyag.Core.IO;

var services = new ServiceCollection();

services.AddSingleton<IFileSystem, LocalFileSystem>();

services.AddSingleton<ICommand>(sp => new InitCommand(sp.GetRequiredService<IFileSystem>()));

string[] plannedCommands =
[
    "add", "cat-file", "check-ignore", "checkout", "commit",
    "hash-object", "log", "ls-files", "ls-tree",
    "rev-prase", "rm", "show-ref", "status", "tag"
];

foreach (var name in plannedCommands)
{
    services.AddSingleton<ICommand>(new NotYetImplemented(name));
}

await using var provider = services.BuildServiceProvider();

var commands = provider.GetServices<ICommand>()
    .ToDictionary(c => c.Name, StringComparer.Ordinal);

if (args.Length == 0)
{
    Console.Error.WriteLine("usage: wyag COMMAND [ARGS...]");
    return 1;
}

var commandName = args[0];
var rest = args.Skip(1).ToArray();

if (!commands.TryGetValue(commandName, out var command))
{
    Console.Error.WriteLine($"wyag: '{commandName}' is not a wyag command.");
    return 1;
}

return await command.ExecuteAsync(rest, CancellationToken.None);
