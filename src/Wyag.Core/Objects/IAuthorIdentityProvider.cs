namespace Wyag.Core.Objects;

public interface IAuthorIdentityProvider
{
    string GetIdentity(GitRepository repo);
}
