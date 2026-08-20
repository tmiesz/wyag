using Wyag.Core.Index;

namespace Wyag.Core.Objects;

public interface ITreeBuilder
{
    string BuildTreeFromIndex(GitRepository repo, GitIndex index);
}
