using System.Buffers.Binary;
using System.Text;
using Wyag.Core.Exceptions;

namespace Wyag.Core.Index;

public sealed class GitIndexParser
{
    public static GitIndex Parse(byte[] raw)
    {
        var header = raw.AsSpan(0, 12);

        var signature = Encoding.ASCII.GetString(header[..4]);
        if (signature != "DIRC")
            throw new GitException($"Invalid index signature {signature}");

        var version = BinaryPrimitives.ReadUInt32BigEndian(header.Slice(4, 4));
        if (version != 2)
            throw new GitException($"wyag onl supports index file version 2, not {version}");

        var count = BinaryPrimitives.ReadUInt32BigEndian(header.Slice(8, 4));

        var content = raw.AsSpan(12);
        var entries = new List<GitIndexEntry>();
        var position = 0;

        for (var i = 0; i < count; i++)
        {
            var entry = content[position..];

            var ctimeSeconds = ReadU32(entry, 0);
            var ctimeNanoseconds = ReadU32(entry, 4);
            var mtimeSeconds = ReadU32(entry, 8);
            var mtimeNanoseconds = ReadU32(entry, 12);
            var dev = ReadU32(entry, 16);
            var ino = ReadU32(entry, 20);
            var mode = ReadU32(entry, 24);
            var uid = ReadU32(entry, 28);
            var gid = ReadU32(entry, 32);
            var fileSize = ReadU32(entry, 36);
            var sha = Convert.ToHexString(entry.Slice(40, 20)).ToLowerInvariant();
            var flags = BinaryPrimitives.ReadUInt16BigEndian(entry.Slice(60, 2));

            var modeType = mode >> 12;
            if (modeType is not (0b1000 or 0b1010 or 0b1110))
                throw new GitException($"Invalid index entry mode type {Convert.ToString(modeType, 2)}");
            var modePerms = mode & 0b0_0000_0001_1111_1111;

            var flagExtended = (flags & 0b0100_0000_0000_0000) != 0;
            if (flagExtended)
                throw new GitException("wyag does not support extended index entry flags.");

            var flagAssumeValid = (flags & 0b1000_0000_0000_0000) != 0;
            var flagStage = (flags >> 12) & 0b0000_0000_0000_0011;
            var nameLength = flags & 0b0000_1111_1111_1111;

            const int nameStart = 62;
            string name;
            int consumed;

            if (nameLength < 0xFFF)
            {
                name = Encoding.UTF8.GetString(entry.Slice(nameStart, nameLength));
                consumed = nameStart + nameLength + 1;
            }
            else
            {
                var nullIndex = entry[nameStart..].IndexOf((byte)0);
                name = Encoding.UTF8.GetString(entry.Slice(nameStart, nullIndex));
                consumed = nameStart + nullIndex + 1;
            }

            entries.Add(new GitIndexEntry(
                        FromUnixTime(ctimeSeconds, ctimeNanoseconds),
                        FromUnixTime(mtimeSeconds, mtimeNanoseconds),
                        dev, ino, modeType, modePerms, uid, gid, fileSize, sha,
                        flagAssumeValid, flagStage, name));

            var newPosition = position + consumed;
            position = 8 * ((newPosition + 7) / 8);
        }

        return new GitIndex { Version = version, Entries = entries };
    }

    private static uint ReadU32(Span<byte> data, int offset) =>
        BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, 4));

    private static DateTimeOffset FromUnixTime(uint seconds, uint nanoseconds) =>
        DateTimeOffset.FromUnixTimeSeconds(seconds).AddTicks(nanoseconds / 100);
}
