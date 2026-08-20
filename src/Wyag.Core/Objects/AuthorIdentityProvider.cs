namespace Wyag.Core.Objects;

public sealed class AuthorIdentityProvider : IAuthorIdentityProvider
{
    public string GetIdentity(GitRepository repo)
    {
        var name = repo.Config.Get("user", "name");
        var email = repo.Config.Get("user", "email");

        if (name is not null && email is not null)
            return $"{name} <{email}>";

        return "Wyag <example@domain.com>";
    }
}
