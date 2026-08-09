namespace Wyag.Core.Objects;

// <summary>
// Base class for every Git object.
// </summary>
public abstract class GitObject
{
    public abstract string Format { get; }

    protected GitObject() => Init();

    protected GitObject(byte[] data) => Deserialize(data);

    public abstract byte[] Serialize();

    public abstract void Deserialize(byte[] data);

    protected virtual void Init() { }
}

