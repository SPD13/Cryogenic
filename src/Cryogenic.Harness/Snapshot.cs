namespace Cryogenic.Harness;

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Read/write a checkpoint snapshot consisting of:
///   * `ram.bin` — raw conventional-memory image (1 MiB).
///   * `state.json` — CPU general/segment registers + flags.
///   * `meta.json` — provenance (checkpoint address, timestamp, RAM SHA-256).
///
/// Phase 1 only writes; restore arrives in phase 2 alongside the direct invoker.
/// We deliberately keep the format stupid-simple so an external tool (or a future
/// version of the harness on a newer Spice86) can read it without coupling.
/// </summary>
public static class Snapshot {
    public const uint RamSize = 0x100000; // 1 MiB conventional memory

    public static SnapshotMeta Save(string outputDirectory, IMemory memory, State state, SegmentedAddress checkpoint) {
        Directory.CreateDirectory(outputDirectory);

        byte[] ram = memory.ReadRam(RamSize, 0);

        string ramPath = Path.Combine(outputDirectory, "ram.bin");
        File.WriteAllBytes(ramPath, ram);

        string ramSha;
        using (SHA256 sha = SHA256.Create()) {
            ramSha = Convert.ToHexString(sha.ComputeHash(ram)).ToLowerInvariant();
        }

        CpuStateDto dto = CpuStateDto.From(state);
        string statePath = Path.Combine(outputDirectory, "state.json");
        File.WriteAllText(statePath, JsonSerializer.Serialize(dto, JsonOpts));

        SnapshotMeta meta = new() {
            CapturedAtUtc = DateTime.UtcNow.ToString("O"),
            CheckpointSegment = checkpoint.Segment,
            CheckpointOffset = checkpoint.Offset,
            CheckpointLinear = checkpoint.Linear,
            RamSizeBytes = RamSize,
            RamSha256 = ramSha,
            SnapshotFormatVersion = 1,
        };
        File.WriteAllText(Path.Combine(outputDirectory, "meta.json"), JsonSerializer.Serialize(meta, JsonOpts));
        return meta;
    }

    private static readonly JsonSerializerOptions JsonOpts = new() {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };
}

public sealed class SnapshotMeta {
    public string CapturedAtUtc { get; set; } = string.Empty;
    public ushort CheckpointSegment { get; set; }
    public ushort CheckpointOffset { get; set; }
    public uint CheckpointLinear { get; set; }
    public uint RamSizeBytes { get; set; }
    public string RamSha256 { get; set; } = string.Empty;
    public int SnapshotFormatVersion { get; set; }
}

/// <summary>
/// Plain-old-data mirror of the registers + flags we care about. Mirrors the
/// fields exposed by <see cref="State"/> so the file is human-readable and
/// trivially restorable in phase 2.
/// </summary>
public sealed class CpuStateDto {
    public uint EAX { get; set; }
    public uint EBX { get; set; }
    public uint ECX { get; set; }
    public uint EDX { get; set; }
    public uint ESI { get; set; }
    public uint EDI { get; set; }
    public uint EBP { get; set; }
    public uint ESP { get; set; }
    public ushort CS { get; set; }
    public ushort DS { get; set; }
    public ushort ES { get; set; }
    public ushort FS { get; set; }
    public ushort GS { get; set; }
    public ushort SS { get; set; }
    public ushort IP { get; set; }
    public ushort Flags { get; set; }
    public long Cycles { get; set; }

    public static CpuStateDto From(State s) => new() {
        EAX = s.EAX, EBX = s.EBX, ECX = s.ECX, EDX = s.EDX,
        ESI = s.ESI, EDI = s.EDI, EBP = s.EBP, ESP = s.ESP,
        CS = s.CS, DS = s.DS, ES = s.ES, FS = s.FS, GS = s.GS, SS = s.SS,
        IP = s.IP,
        Flags = (ushort)s.Flags.FlagRegister,
        Cycles = s.Cycles,
    };
}
