namespace Wyag.Core.Status;

public interface IRepositoryStatusService
{
    RepositoryStatus GetStatus(GitRepository repo);
}
