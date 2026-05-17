namespace Cryogenic.Services;

using Serilog;

using Spice86.Core.CLI;
using Spice86.Shared.Emulator.Memory;

using System;
using System.IO;

/// <summary>
/// Managed DOS-filesystem shim for the resource layer. Owns the host
/// <c>DUNE.DAT</c> archive and a read cursor so the engine's DUNE.DAT I/O
/// primitives (<c>cs1:0xF2D6</c> seek, <c>cs1:0xF2EA</c> read) can be served
/// from C# instead of bottoming out in emulated DOS INT&#160;21.
/// </summary>
/// <remarks>
/// The engine opens exactly one resource file — <c>DUNE.DAT</c> — and drives
/// it via a strict locate→seek→read sequence (Tech/01). There is a single
/// logical handle (<c>ss:0xDBBA</c>), so a singleton cursor is faithful.
/// <para>
/// The asm open path (<c>cs1:0xE675</c>) is intentionally left to emulated
/// DOS: it still opens the host file and stashes a DOS handle the engine never
/// inspects beyond passing it back to seek/read — which this service now
/// services independently. The bytes delivered are byte-identical to the
/// emulated-DOS path; only the I/O mechanism changes.
/// </para>
/// <para>
/// If the host <c>DUNE.DAT</c> cannot be located or fails validation,
/// <see cref="IsAvailable"/> stays <c>false</c> and the caller leaves the asm
/// (emulated-DOS) seek/read in place — the shim never regresses a working game.
/// </para>
/// </remarks>
public sealed class DuneDatService {
    /// <summary>Exact on-disk size of the shipped Dune CD v3.7 DUNE.DAT (Tech/01).</summary>
    private const long ExpectedSize = 397_794_384L;

    /// <summary>Declared slot count u16 LE at offset 0 of a valid DUNE.DAT (Tech/01).</summary>
    private const ushort ExpectedSlotCount = 0x0A3D; // 2621

    private readonly FileStream? _stream;
    private readonly byte[] _copyBuffer = new byte[0x10000];

    /// <summary>Absolute byte offset for the next read (DOS file-pointer equivalent).</summary>
    private long _cursor;

    /// <summary><c>true</c> when a valid host DUNE.DAT was opened and the shim is live.</summary>
    public bool IsAvailable => _stream != null;

    /// <summary>Resolved host path of the DUNE.DAT in use (for logging/diagnostics).</summary>
    public string? ResolvedPath { get; }

    /// <summary>
    /// Locates and validates the host DUNE.DAT, opening a shared read-only
    /// stream when found. Never throws — failure simply leaves the shim
    /// unavailable so the emulated-DOS path keeps serving the game.
    /// </summary>
    public DuneDatService(Configuration configuration) {
        try {
            string? path = ResolvePath(configuration);
            if (path == null) {
                Log.Warning("DuneDatService: host DUNE.DAT not found; resource I/O stays on emulated DOS.");
                return;
            }

            var info = new FileInfo(path);
            if (info.Length != ExpectedSize) {
                Log.Warning("DuneDatService: {Path} size {Size} != expected {Expected}; shim disabled.",
                    path, info.Length, ExpectedSize);
                return;
            }

            FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            int lo = fs.ReadByte();
            int hi = fs.ReadByte();
            ushort slotCount = (ushort)(lo | (hi << 8));
            if (slotCount != ExpectedSlotCount) {
                Log.Warning("DuneDatService: {Path} slot count 0x{Count:X4} != 0x{Expected:X4}; shim disabled.",
                    path, slotCount, ExpectedSlotCount);
                fs.Dispose();
                return;
            }

            _stream = fs;
            _cursor = 0;
            ResolvedPath = path;
            Log.Information("DuneDatService: serving resource I/O from host {Path} ({Size} bytes).",
                path, info.Length);
        } catch (Exception ex) {
            Log.Warning(ex, "DuneDatService: initialization failed; resource I/O stays on emulated DOS.");
        }
    }

    /// <summary>
    /// DOS <c>LSEEK</c> (INT&#160;21 AX=4200) equivalent: sets the absolute
    /// read position. Returns the resulting position (DOS reports it in DX:AX).
    /// </summary>
    public uint Seek(uint absoluteOffset) {
        _cursor = absoluteOffset;
        return absoluteOffset;
    }

    /// <summary>
    /// DOS read-with-handle (INT&#160;21 AH=3F) equivalent: copies up to
    /// <paramref name="count"/> bytes from the current cursor into emulated
    /// memory starting at the linear address <paramref name="destLinear"/>,
    /// advancing the cursor by the number of bytes actually read. Returns the
    /// byte count (which is less than <paramref name="count"/> only at EOF —
    /// the asm caller turns that into a short-read flag via <c>cmp ax,cx</c>).
    /// </summary>
    public int ReadInto(IMemory memory, uint destLinear, int count) {
        if (_stream == null || count <= 0) {
            return 0;
        }
        if (_cursor != _stream.Position) {
            _stream.Seek(_cursor, SeekOrigin.Begin);
        }
        int total = 0;
        while (total < count) {
            int chunk = Math.Min(_copyBuffer.Length, count - total);
            int n = _stream.Read(_copyBuffer, 0, chunk);
            if (n <= 0) {
                break;
            }
            for (int k = 0; k < n; k++) {
                memory.UInt8[(uint)(destLinear + total + k)] = _copyBuffer[k];
            }
            total += n;
        }
        _cursor += total;
        return total;
    }

    /// <summary>
    /// Resolves the host DUNE.DAT path. Tries, in order: the Spice86 C: drive
    /// mount, the directory of the configured executable, the current working
    /// directory, and the app base directory — each checked directly and via a
    /// bounded walk up its parents. Matching is case-insensitive.
    /// </summary>
    private static string? ResolvePath(Configuration configuration) {
        foreach (string? seed in EnumerateSeedDirectories(configuration)) {
            if (string.IsNullOrEmpty(seed)) {
                continue;
            }
            string? dir;
            try {
                dir = Directory.Exists(seed) ? seed : Path.GetDirectoryName(seed);
            } catch {
                continue;
            }
            for (int depth = 0; depth < 4 && !string.IsNullOrEmpty(dir); depth++) {
                string? hit = FindDuneDatIn(dir!);
                if (hit != null) {
                    return hit;
                }
                try {
                    dir = Path.GetDirectoryName(dir);
                } catch {
                    break;
                }
            }
        }
        return null;
    }

    private static System.Collections.Generic.IEnumerable<string?> EnumerateSeedDirectories(
        Configuration configuration) {
        yield return SafeGet(() => configuration.CDrive);
        yield return SafeGet(() => configuration.Exe);
        yield return SafeGet(() => configuration.RecordedDataDirectory);
        yield return Environment.CurrentDirectory;
        yield return AppContext.BaseDirectory;
    }

    private static string? SafeGet(Func<string?> getter) {
        try {
            return getter();
        } catch {
            return null;
        }
    }

    private static string? FindDuneDatIn(string dir) {
        try {
            foreach (string file in Directory.EnumerateFiles(dir)) {
                if (string.Equals(Path.GetFileName(file), "DUNE.DAT", StringComparison.OrdinalIgnoreCase)) {
                    return file;
                }
            }
        } catch {
            // unreadable directory — skip
        }
        return null;
    }
}
