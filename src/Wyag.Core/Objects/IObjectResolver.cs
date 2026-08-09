namespace Wyag.Core.Objects;

public interface IObjectResolver
{
    string Find(GitRepository repo, string name, string? format = null, bool follow = true);
}
