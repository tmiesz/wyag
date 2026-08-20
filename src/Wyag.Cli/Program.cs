using Microsoft.Extensions.DependencyInjection;
using Wyag.Core.Commands;
using Wyag.Core.Exceptions;
using Wyag.Core.Ignore;
using Wyag.Core.Index;
using Wyag.Core.IO;
using Wyag.Core.Objects;
using Wyag.Core.Refs;
using Wyag.Core.Status;

var services = new ServiceCollection();

services.AddSingleton<IFileSystem, LocalFileSystem>();
services.AddSingleton<IObjectStore, ObjectStore>();
services.AddSingleton<IObjectResolver, GitObjectResolver>();
services.AddSingleton<IRefStore, RefStore>();
services.AddSingleton<ITagService, TagService>();
services.AddSingleton<IIndexStore, IndexStore>();
services.AddSingleton<IGitIgnoreService, GitIgnoreService>();
services.AddSingleton<IBranchService, BranchService>();
services.AddSingleton<IRepositoryStatusService, RepositoryStatusService>();
services.AddSingleton<IStagingService, StagingService>();
services.AddSingleton<ITreeBuilder, TreeBuilder>();
services.AddSingleton<IAuthorIdentityProvider, AuthorIdentityProvider>();
services.AddSingleton<ICommitService, CommitService>();

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

services.AddSingleton<ICommand>(sp => new ShowRefCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IRefStore>()));

services.AddSingleton<ICommand>(sp => new TagCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IRefStore>(),
            sp.GetRequiredService<ITagService>()));

services.AddSingleton<ICommand>(sp => new RevParseCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IObjectResolver>()));

services.AddSingleton<ICommand>(sp => new LsFilesCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IIndexStore>()));

services.AddSingleton<ICommand>(sp => new CheckIgnoreCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IGitIgnoreService>()));

services.AddSingleton<ICommand>(sp => new StatusCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IRepositoryStatusService>()));

services.AddSingleton<ICommand>(sp => new RmCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IStagingService>()));

services.AddSingleton<ICommand>(sp => new AddCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IStagingService>()));

services.AddSingleton<ICommand>(sp => new CommitCommand(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<ICommitService>()));

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
try
{
    return await command.ExecuteAsync(rest, CancellationToken.None);
}
catch (GitException ex)
{
    Console.Error.WriteLine($"wyag: {ex.Message}");
    return 1;
}
