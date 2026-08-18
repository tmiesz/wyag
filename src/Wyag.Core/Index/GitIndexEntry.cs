namespace Wyag.Core.Index;

public sealed record GitIndexEntry(
        DateTimeOffset CTime,
        DateTimeOffset MTime,
        uint Dev,
        uint Ino,
        uint ModeType,
        uint ModePerms,
        uint Uid,
        uint Gid,
        uint FileSize,
        string Sha,
        bool FlagAssumeValid,
        int FlagStage,
        string Name);
