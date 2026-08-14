namespace Wyag.Core.Objects;

public class GitCommit : GitObject
{
    public KvlmDocument Kvlm { get; protected set; } = null!;

    public override string Format => "commit";

    public GitCommit() { }
    public GitCommit(byte[] data) : base(data) { }

    public override void Deserialize(byte[] data) => Kvlm = KvlmDocument.Parse(data);

    public override byte[] Serialize() => Kvlm.Serialize();

    protected override void Init() => Kvlm = new KvlmDocument();
}
