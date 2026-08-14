using Wyag.Core.IO;
using Wyag.Core.Objects;

namespace Wyag.Core.Commands;

public class HashObjectCommand(IFileSystem fs, IObjectStore os) : ICommand
{
    public string Name => "hash-object";

    public string HelpText => "Compute object ID and optionally creates a blob from a file";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var format = "blob";
        var write = false;
        string? path = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-t":
                    format = args[++i];
                    break;
                case "-w":
                    write = true;
                    break;
                default:
                    path = args[i];
                    break;
            }
        }

        if (path is null)
        {
            Console.Error.WriteLine("usage: wyag hash-object [-w] [-t type] FILE");
            return Task.FromResult(1);
        }
        
        var repo = write ? GitRepository.Find(fs) : null;

        using var stream = fs.OpenRead(path);
        var sha = os.Hash(stream, format, repo);
        Console.WriteLine(sha);

        return Task.FromResult(0);
    }
}
