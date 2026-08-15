namespace Wyag.Core.Objects;

public sealed class GitTree : GitObject
{
    public List<GitTreeLeaf> Items { get; private set; } = null!;

    public override string Format => "tree";

    public GitTree() { }
    public GitTree(byte[] data) : base(data) { }

    public override void Deserialize(byte[] data) => Items = GitTreeParser.Parse(data);
    public override byte[] Serialize() => GitTreeSerializer.Serialize(Items);

    protected override void Init() => Items = [];
}
