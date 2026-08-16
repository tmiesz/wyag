namespace Wyag.Core.Objects;

public sealed class GitTag : GitCommit
{
    public override string Format => "tag";

    public GitTag() { }
    public GitTag(byte[] data) : base(data) { }
}
