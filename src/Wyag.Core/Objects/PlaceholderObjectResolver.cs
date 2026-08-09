namespace Wyag.Core.Objects;

public class PlaceholderObjectResolver : IObjectResolver
{
    public string Find(GitRepository repo, string name, string? format = null, bool follow = true) => name;
}
