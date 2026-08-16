namespace Wyag.Core.Objects;

public interface ITagService
{
    void Create(GitRepository repo, string name, string reference, bool createTagObject);
}
