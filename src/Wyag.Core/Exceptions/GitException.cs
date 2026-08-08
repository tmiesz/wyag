namespace Wyag.Core.Exceptions;

public sealed class GitException : Exception
{
    public GitException(string message) : base(message) { }
    public GitException(string message, Exception innerException) : base(message, innerException) { }
}
