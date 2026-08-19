using System.Buffers.Binary;
using System.Text;

namespace Wyag.Core.Index;

public static class GitIndexSerializer
{
    public static byte[] Serialize(GitIndex index)
    {
        using var buffer = new MemoryStream();

        buffer.Write(Encoding.ASCII.GetBytes("DIRC"));
        WriteU32(buffer, index.Version);
        WriteU32(buffer, (uint)index.Entries.Count);

        foreach (var entry in index.Entries)
        {
            var (ctimeSeconds, ctimeNanoseconds) = ToUnixTime(entry.CTime);
            var (mtimeSeconds, mtimeNanoseconds) = ToUnixTime(entry.MTime);

            WriteU32(buffer, ctimeSeconds);
            WriteU32(buffer, ctimeNanoseconds);
            WriteU32(buffer, mtimeSeconds);
            WriteU32(buffer, mtimeNanoseconds);
            WriteU32(buffer, entry.Dev);
            WriteU32(buffer, entry.Ino);
            WriteU32(buffer, (entry.ModeType << 12) | entry.ModePerms);
            WriteU32(buffer, entry.Uid);
            WriteU32(buffer, entry.Gid);
            WriteU32(buffer, entry.FileSize);
            buffer.Write(Convert.FromHexString(entry.Sha));

            var nameBytes = Encoding.UTF8.GetBytes(entry.Name);
            var nameLength = (uint)Math.Min(nameBytes.Length, 0xFFF);

            var flags = (ushort)((entry.FlagAssumeValid ? 0x8000u : 0u)
                | (((uint)entry.FlagStage & 0b11) << 12)
                | (nameLength & 0x0FFF));

            WriteU16(buffer, flags);
            buffer.Write(nameBytes);
            buffer.WriteByte(0);

            var consumed = 62 + nameBytes.Length + 1;
            var padded = 8 * ((consumed + 7) / 8);
            for (var i = consumed; i < padded; i++)
                buffer.WriteByte(0);
        }

        return buffer.ToArray();
    }

    private static void WriteU16(Stream stream, ushort value)
    {
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
        stream.Write(bytes);
    }

    private static void WriteU32(Stream stream, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
        stream.Write(bytes);
    }

    private static (uint ctimeSeconds, uint ctimeNanoseconds) ToUnixTime(DateTimeOffset value)
    {
        var seconds = value.ToUnixTimeSeconds();
        var wholeSecond = DateTimeOffset.FromUnixTimeSeconds(seconds);
        var nanoseconds = (uint)((value - wholeSecond).Ticks * 100);
        return ((uint)seconds, nanoseconds);
    }
}
