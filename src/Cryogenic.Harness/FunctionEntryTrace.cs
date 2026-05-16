namespace Cryogenic.Harness;

using System.Text;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Emulator.VM.Breakpoint;

/// <summary>
/// Sparse function-entry trace. Like <see cref="InstructionTrace"/> but only fires
/// at the addresses you explicitly list (one breakpoint per address, no per-byte
/// scan). Each hit emits an ndjson record with cycles + regs + a configurable
/// excerpt of memory at <c>DS:SI</c> so we can see the bytes the engine is about
/// to consume (e.g. the first words of an LZ-decoded HNM payload).
///
/// Designed for the "trace" mode: boot the game, let it run normally, log when
/// known dispatch points fire. Combined with a max-cycles budget that calls
/// <see cref="Action"/> when exhausted, this is enough to capture the actual
/// runtime call sequence for cinematic playback without doing a full instruction
/// trace.
/// </summary>
public sealed class FunctionEntryTrace : IDisposable {
    /// <summary>
    /// One traced address. <paramref name="DsSiBytes"/> excerpts the bytes at
    /// DS:SI on hit. <paramref name="CsSiBytes"/> excerpts at CS:SI (use this
    /// for cs1:0x13C8 / cs1:0x3D83 style sites that read a list out of CS).
    /// <paramref name="ExtraReads"/> lists additional linear addresses to log
    /// as named u16 LE values (e.g. for the per-resource state byte at
    /// [DS:0xDC00] — pass `0xDC00 + (DS_BASE_PARAGRAPH&lt;&lt;4)`).
    /// <paramref name="SegRegReads"/> captures register-relative excerpts
    /// (e.g. read 32 bytes from CS:[BP] to log a dispatcher's resume target).
    /// </summary>
    public sealed record Entry(
        uint LinearAddress,
        string Label,
        int DsSiBytes,
        IReadOnlyList<(string Name, uint Linear, int Bytes)>? ExtraReads = null,
        int CsSiBytes = 0,
        IReadOnlyList<SegRegRead>? SegRegReads = null);

    /// <summary>
    /// A register-relative memory read. <paramref name="Segment"/> and
    /// <paramref name="Offset"/> name the registers (e.g. "CS" + "BP") and
    /// <paramref name="Bytes"/> is the excerpt length (capped at 64).
    /// </summary>
    public sealed record SegRegRead(string Name, string Segment, string Offset, int Bytes);

    private readonly EmulatorBreakpointsManager _bpm;
    private readonly State _state;
    private readonly IMemory _memory;
    private readonly StreamWriter _writer;
    private readonly string _traceOutputPath;
    private readonly List<AddressBreakPoint> _breakpoints = new();
    private long _events;
    private long _cycleCap = -1;
    private Action? _onCap;

    public long EventsWritten => _events;

    public FunctionEntryTrace(
        EmulatorBreakpointsManager bpm,
        State state,
        IMemory memory,
        string outputPath) {
        _bpm = bpm;
        _state = state;
        _memory = memory;
        _traceOutputPath = outputPath;
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        _writer = new StreamWriter(outputPath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    /// <summary>
    /// Tripwire: when a hit fires after `cap` cycles have elapsed, invoke
    /// `onCap` (typically the helper's "stop trace + Exit()" routine). Spice86
    /// 11.1.0 doesn't expose a CYCLES breakpoint type, so we poll it here.
    /// Cap is in absolute cycle count (i.e. <see cref="State.Cycles"/> at the
    /// time the cap should fire), not a relative delta.
    /// </summary>
    public void SetCycleCap(long cap, Action onCap) {
        _cycleCap = cap;
        _onCap = onCap;
    }

    public void Arm(IEnumerable<Entry> entries) {
        foreach (Entry e in entries) {
            AddressBreakPoint bp = new(
                BreakPointType.CPU_EXECUTION_ADDRESS,
                e.LinearAddress,
                _ => OnHit(e),
                isRemovedOnTrigger: false);
            _bpm.ToggleBreakPoint(bp, on: true);
            _breakpoints.Add(bp);
        }
    }

    public void Stop() {
        foreach (AddressBreakPoint bp in _breakpoints) {
            _bpm.ToggleBreakPoint(bp, on: false);
        }
        _breakpoints.Clear();
        _writer.Flush();
        _writer.Close();
    }

    private void OnHit(Entry e) {
        _writer.Write('{');
        _writer.Write($"\"c\":{_state.Cycles},");
        _writer.Write($"\"label\":\"{Escape(e.Label)}\",");
        _writer.Write($"\"linear\":\"{e.LinearAddress:X5}\",");
        _writer.Write($"\"cs\":\"{_state.CS:X4}\",");
        _writer.Write($"\"ip\":\"{_state.IP:X4}\",");
        _writer.Write($"\"ax\":\"{_state.AX:X4}\",");
        _writer.Write($"\"bx\":\"{_state.BX:X4}\",");
        _writer.Write($"\"cx\":\"{_state.CX:X4}\",");
        _writer.Write($"\"dx\":\"{_state.DX:X4}\",");
        _writer.Write($"\"si\":\"{_state.SI:X4}\",");
        _writer.Write($"\"di\":\"{_state.DI:X4}\",");
        _writer.Write($"\"bp\":\"{_state.BP:X4}\",");
        _writer.Write($"\"sp\":\"{_state.SP:X4}\",");
        _writer.Write($"\"ds\":\"{_state.DS:X4}\",");
        _writer.Write($"\"es\":\"{_state.ES:X4}\",");
        _writer.Write($"\"ss\":\"{_state.SS:X4}\"");
        if (e.DsSiBytes > 0) {
            uint linear = (uint)((_state.DS << 4) + _state.SI);
            int n = e.DsSiBytes;
            Span<byte> buf = stackalloc byte[64];
            if (n > buf.Length) n = buf.Length;
            for (int i = 0; i < n; i++) {
                buf[i] = _memory.UInt8[linear + (uint)i];
            }
            _writer.Write($",\"ds_si\":\"{Convert.ToHexString(buf[..n])}\"");
        }
        if (e.CsSiBytes > 0) {
            uint linear = (uint)((_state.CS << 4) + _state.SI);
            int n = e.CsSiBytes;
            Span<byte> buf = stackalloc byte[64];
            if (n > buf.Length) n = buf.Length;
            for (int i = 0; i < n; i++) {
                buf[i] = _memory.UInt8[linear + (uint)i];
            }
            _writer.Write($",\"cs_si\":\"{Convert.ToHexString(buf[..n])}\"");
        }
        if (e.SegRegReads is { Count: > 0 } segReads) {
            Span<byte> sbuf = stackalloc byte[64];
            foreach (SegRegRead r in segReads) {
                ushort seg = RegWord(r.Segment);
                ushort off = RegWord(r.Offset);
                uint linear = (uint)((seg << 4) + off);
                int n = Math.Min(r.Bytes, 64);
                for (int i = 0; i < n; i++) sbuf[i] = _memory.UInt8[linear + (uint)i];
                _writer.Write($",\"{Escape(r.Name)}\":\"{Convert.ToHexString(sbuf[..n])}\"");
                _writer.Write($",\"{Escape(r.Name)}_addr\":\"{seg:X4}:{off:X4}\"");
            }
        }
        if (e.ExtraReads is { Count: > 0 } reads) {
            // Two output modes per read entry:
            //   • bytes ≤ 16 : inline hex in the JSON record (compact, fast).
            //   • bytes  > 16: writes a sidecar `<label>__<name>__c{cycles}.bin`
            //                  in the same directory as the ndjson, and the
            //                  JSON record carries the filename + length so
            //                  the post-processor can join. This avoids
            //                  blowing up the trace JSON for big dumps
            //                  (e.g. capturing the 1280-byte menu-type
            //                  definition region on a PUSH).
            Span<byte> rbufInline = stackalloc byte[16];
            foreach ((string name, uint linear, int bytes) in reads) {
                if (bytes <= 16) {
                    int n = bytes;
                    for (int i = 0; i < n; i++) rbufInline[i] = _memory.UInt8[linear + (uint)i];
                    _writer.Write($",\"{Escape(name)}\":\"{Convert.ToHexString(rbufInline[..n])}\"");
                } else {
                    byte[] big = new byte[bytes];
                    for (int i = 0; i < bytes; i++) big[i] = _memory.UInt8[linear + (uint)i];
                    string traceDir = Path.GetDirectoryName(_traceOutputPath) ?? ".";
                    string filename = $"{e.Label}__{name}__c{_state.Cycles}.bin";
                    string path = Path.Combine(traceDir, filename);
                    File.WriteAllBytes(path, big);
                    _writer.Write($",\"{Escape(name)}_file\":\"{Escape(filename)}\"");
                    _writer.Write($",\"{Escape(name)}_len\":{bytes}");
                    _writer.Write($",\"{Escape(name)}_addr\":\"{linear:X5}\"");
                }
            }
        }
        _writer.WriteLine('}');
        _events++;
        if (_cycleCap >= 0 && _state.Cycles >= _cycleCap) {
            _onCap?.Invoke();
        }
    }

    private static string Escape(string s) {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    /// <summary>
    /// Resolve a register name (case-insensitive) to its current u16 value.
    /// Supports the segment registers (CS/DS/ES/SS/FS/GS) and the general-
    /// purpose 16-bit regs (AX/BX/CX/DX/SI/DI/BP/SP). Throws for unknown.
    /// </summary>
    private ushort RegWord(string name) {
        return name.ToUpperInvariant() switch {
            "CS" => _state.CS,
            "DS" => _state.DS,
            "ES" => _state.ES,
            "SS" => _state.SS,
            "FS" => _state.FS,
            "GS" => _state.GS,
            "AX" => _state.AX,
            "BX" => _state.BX,
            "CX" => _state.CX,
            "DX" => _state.DX,
            "SI" => _state.SI,
            "DI" => _state.DI,
            "BP" => _state.BP,
            "SP" => _state.SP,
            _ => throw new ArgumentException($"unknown register name '{name}' in trace-fn seg-reg read"),
        };
    }

    public void Dispose() {
        Stop();
        _writer.Dispose();
    }
}
