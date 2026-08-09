namespace Wyag.Core.Objects;

public sealed class GitBlob : GitObject
{
    public byte[] BlobData { get; private set; } = null!;

    public override string Format => "blob";

    public GitBlob() { }
    public GitBlob(byte[] data) : base(data) { }

    public override void Deserialize(byte[] data) => BlobData = data;

    public override byte[] Serialize() => BlobData;

    protected override void Init() => BlobData = [];
}
