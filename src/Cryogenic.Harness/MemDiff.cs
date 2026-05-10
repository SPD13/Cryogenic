namespace Cryogenic.Harness;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Compute and serialise a byte-level diff between two RAM snapshots. Differences
/// are grouped into runs (consecutive changed addresses) so the output is compact
/// even when an opcode rewrites a few hundred bytes scattered across globals.
/// </summary>
/// <remarks>
/// Goal: at a glance, the user can see <em>which globals the invoked function
/// touched</em>. This is the highest insight/cost ratio of any trace layer —
/// 1 MiB of RAM compared in microseconds, output is usually tens of small runs.
/// </remarks>
public static class MemDiff {
    /// <summary>How far apart two changed bytes can be before we split into separate runs.</summary>
    private const int MergeGap = 4;

    /// <summary>Per-run cap on the byte preview written into the JSON.</summary>
    private const int PreviewBytesPerRun = 64;

    public static MemDiffResult Compute(byte[] before, byte[] after) {
        if (before.Length != after.Length) {
            throw new ArgumentException($"length mismatch: before={before.Length}, after={after.Length}");
        }

        List<MemDiffRun> runs = new();
        int i = 0;
        int n = before.Length;
        while (i < n) {
            if (before[i] == after[i]) {
                i++;
                continue;
            }
            int start = i;
            int lastDiff = i;
            i++;
            while (i < n && (before[i] != after[i] || i - lastDiff <= MergeGap)) {
                if (before[i] != after[i]) {
                    lastDiff = i;
                }
                i++;
            }
            int end = lastDiff + 1;
            runs.Add(MakeRun(start, end, before, after));
        }

        long totalChanged = 0;
        foreach (MemDiffRun r in runs) {
            totalChanged += r.ChangedByteCount;
        }
        return new MemDiffResult {
            RamSizeBytes = n,
            ChangedRegionCount = runs.Count,
            ChangedByteCount = totalChanged,
            Runs = runs,
        };
    }

    public static void Save(string outputDir, MemDiffResult diff) {
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(
            Path.Combine(outputDir, "memdiff.json"),
            JsonSerializer.Serialize(diff, JsonOpts));
    }

    private static MemDiffRun MakeRun(int start, int end, byte[] before, byte[] after) {
        int len = end - start;
        int preview = Math.Min(len, PreviewBytesPerRun);
        int changed = 0;
        for (int j = 0; j < len; j++) {
            if (before[start + j] != after[start + j]) {
                changed++;
            }
        }
        return new MemDiffRun {
            Linear = (uint)start,
            LinearHex = $"0x{start:X5}",
            SegOff = $"{start >> 4:X4}:{start & 0xF:X4}",
            Length = len,
            ChangedByteCount = changed,
            BeforeHex = ToHex(before, start, preview),
            AfterHex = ToHex(after, start, preview),
            Truncated = len > preview,
        };
    }

    private static string ToHex(byte[] src, int start, int len) {
        Span<char> buf = stackalloc char[len * 2];
        for (int i = 0; i < len; i++) {
            byte b = src[start + i];
            buf[i * 2] = "0123456789abcdef"[b >> 4];
            buf[i * 2 + 1] = "0123456789abcdef"[b & 0xF];
        }
        return new string(buf);
    }

    private static readonly JsonSerializerOptions JsonOpts = new() {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };
}

public sealed class MemDiffResult {
    public int RamSizeBytes { get; set; }
    public int ChangedRegionCount { get; set; }
    public long ChangedByteCount { get; set; }
    public List<MemDiffRun> Runs { get; set; } = new();
}

public sealed class MemDiffRun {
    public uint Linear { get; set; }
    public string LinearHex { get; set; } = string.Empty;
    public string SegOff { get; set; } = string.Empty;
    public int Length { get; set; }
    public int ChangedByteCount { get; set; }
    public string BeforeHex { get; set; } = string.Empty;
    public string AfterHex { get; set; } = string.Empty;
    public bool Truncated { get; set; }
}
