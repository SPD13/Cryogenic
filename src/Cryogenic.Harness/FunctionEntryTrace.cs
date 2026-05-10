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
    /// DS:SI on hit. <paramref name="ExtraReads"/> lists additional linear
    /// addresses to log as named u16 LE values (e.g. for the per-resource
    /// state byte at [DS:0xDC00] — pass `0xDC00 + (DS_BASE_PARAGRAPH<<4)`).
    /// </summary>
    public sealed record Entry(
        uint LinearAddress,
        string Label,
        int DsSiBytes,
        IReadOnlyList<(string Name, uint Linear, int Bytes)>? ExtraReads = null);

    private readonly EmulatorBreakpointsManager _bpm;
    private readonly State _state;
    private readonly IMemory _memory;
    private readonly StreamWriter _writer;
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
        if (e.ExtraReads is { Count: > 0 } reads) {
            foreach ((string name, uint linear, int bytes) in reads) {
                int n = Math.Min(bytes, 16);
                Span<byte> buf = stackalloc byte[16];
                for (int i = 0; i < n; i++) buf[i] = _memory.UInt8[linear + (uint)i];
                _writer.Write($",\"{Escape(name)}\":\"{Convert.ToHexString(buf[..n])}\"");
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

    public void Dispose() {
        Stop();
        _writer.Dispose();
    }
}
