using Microsoft.Extensions.DependencyInjection;
using Wyag.Core.Commands;
using Wyag.Core.IO;
using Wyag.Core.Objects;

var services = new ServiceCollection();

services.AddSingleton<IFileSystem, LocalFileSystem>();
services.AddSingleton<IObjectStore, ObjectStore>();
services.AddSingleton<IObjectResolver, PlaceholderObjectResolver>();

services.AddSingleton<ICommand>(sp => new InitCommand(
            sp.GetRequiredService<IFileSystem>()));

services.AddSingleton<ICommand>(sp => new CatFileCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IObjectStore>(),
            sp.GetRequiredService<IObjectResolver>()));

services.AddSingleton<ICommand>(sp => new HashObjectCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IObjectStore>()));

services.AddSingleton<ICommand>(sp => new LogCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IObjectStore>(),
            sp.GetRequiredService<IObjectResolver>()));

services.AddSingleton<ICommand>(sp => new LsTreeCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IObjectStore>(),
            sp.GetRequiredService<IObjectResolver>()));

services.AddSingleton<ICommand>(sp => new CheckoutCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IObjectStore>(),
            sp.GetRequiredService<IObjectResolver>()));

string[] plannedCommands =
[
    "add", "check-ignore", "commit",
    "ls-files",
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
