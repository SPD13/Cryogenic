namespace Cryogenic.Harness;

using System.Text;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Emulator.VM.Breakpoint;

/// <summary>
/// Streams one ndjson record per memory-write to a watched address. Used to
/// find the instruction(s) that patch decoder pointer slots like
/// `DS:0x38FB / 0x38FD` while the engine boots and loads an HNM — the goal is
/// to identify the *real* Block-172 decoder address by observing which value
/// gets written into those slots before a flag3-prone HNM plays.
/// </summary>
public sealed class MemWriteWatch : IDisposable {
    private readonly EmulatorBreakpointsManager _bpm;
    private readonly State _state;
    private readonly IMemory _memory;
    private readonly StreamWriter _writer;
    private readonly List<AddressBreakPoint> _bps = new();
    private long _events;

    public long EventsWritten => _events;

    public MemWriteWatch(
        EmulatorBreakpointsManager bpm, State state, IMemory memory, string outputPath) {
        _bpm = bpm;
        _state = state;
        _memory = memory;
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        _writer = new StreamWriter(outputPath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    /// <summary>Arm a write breakpoint at every linear address in <paramref name="addrs"/>.</summary>
    public void Arm(IEnumerable<uint> addrs) {
        foreach (uint addr in addrs) {
            uint capture = addr; // closure capture
            AddressBreakPoint bp = new(
                BreakPointType.MEMORY_WRITE,
                capture,
                _ => OnWrite(capture),
                isRemovedOnTrigger: false);
            _bpm.ToggleBreakPoint(bp, on: true);
            _bps.Add(bp);
        }
    }

    public void Stop() {
        foreach (AddressBreakPoint bp in _bps) {
            _bpm.ToggleBreakPoint(bp, on: false);
        }
        _bps.Clear();
        _writer.Flush();
        _writer.Close();
    }

    /// <summary>
    /// MEMORY_WRITE fires AFTER the byte has been stored. We dump the surrounding
    /// 8 bytes so far-pointer writes (4-byte) are visible as the new value.
    /// </summary>
    private void OnWrite(uint linear) {
        Span<byte> ctx = stackalloc byte[8];
        for (int i = 0; i < ctx.Length; i++) {
            ctx[i] = _memory.UInt8[linear + (uint)i];
        }
        _writer.Write('{');
        _writer.Write($"\"c\":{_state.Cycles},");
        _writer.Write($"\"addr\":\"0x{linear:X5}\",");
        _writer.Write($"\"cs\":\"{_state.CS:X4}\",");
        _writer.Write($"\"ip\":\"{_state.IP:X4}\",");
        _writer.Write($"\"ds\":\"{_state.DS:X4}\",");
        _writer.Write($"\"es\":\"{_state.ES:X4}\",");
        _writer.Write($"\"value8\":\"{Convert.ToHexString(ctx)}\",");
        _writer.Write($"\"ax\":\"{_state.AX:X4}\",");
        _writer.Write($"\"bx\":\"{_state.BX:X4}\",");
        _writer.Write($"\"cx\":\"{_state.CX:X4}\",");
        _writer.Write($"\"dx\":\"{_state.DX:X4}\",");
        _writer.Write($"\"si\":\"{_state.SI:X4}\",");
        _writer.Write($"\"di\":\"{_state.DI:X4}\"");
        _writer.WriteLine('}');
        _events++;
    }

    public void Dispose() {
        Stop();
        _writer.Dispose();
    }
}
