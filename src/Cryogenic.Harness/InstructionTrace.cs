namespace Cryogenic.Harness;

using System.Text;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Emulator.VM.Breakpoint;

/// <summary>
/// Per-instruction execution trace. AddressBreakPoint matches a *single* linear
/// address, so to trace a range we install one breakpoint per byte in the range.
/// This is dense (a 64 KB segment ⇒ 65 536 breakpoints) but the manager dispatches
/// in O(1) per cycle, and breakpoints that never fire (mid-instruction byte
/// addresses) cost nothing.
///
/// Output is ndjson — one record per fired instruction. Designed for streaming so
/// long sweeps don't blow up memory.
/// </summary>
public sealed class InstructionTrace : IDisposable {
    private readonly EmulatorBreakpointsManager _bpm;
    private readonly State _state;
    private readonly IMemory _memory;
    private readonly uint _rangeStart;
    private readonly uint _rangeEnd;
    private readonly StreamWriter _writer;
    private readonly List<AddressBreakPoint> _breakpoints = new();
    private long _eventsWritten;

    public long EventsWritten => _eventsWritten;

    public InstructionTrace(
        EmulatorBreakpointsManager bpm,
        State state,
        IMemory memory,
        uint rangeStart,
        uint rangeEnd,
        string outputPath) {
        _bpm = bpm;
        _state = state;
        _memory = memory;
        _rangeStart = rangeStart;
        _rangeEnd = rangeEnd;
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        _writer = new StreamWriter(outputPath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    public void Start() {
        // Pre-allocate to avoid the list growing while we install. Capacity matters here —
        // a 64 KB range is 65 536 entries.
        long count = (long)_rangeEnd - _rangeStart + 1;
        _breakpoints.Capacity = (int)Math.Min(count, int.MaxValue);

        for (long addr = _rangeStart; addr <= _rangeEnd; addr++) {
            AddressBreakPoint bp = new(
                BreakPointType.CPU_EXECUTION_ADDRESS,
                addr,
                _ => OnInstruction(),
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

    private void OnInstruction() {
        uint linear = (uint)((_state.CS << 4) + _state.IP);
        Span<byte> opcode = stackalloc byte[8];
        for (int i = 0; i < opcode.Length; i++) {
            opcode[i] = _memory.UInt8[linear + (uint)i];
        }
        _writer.Write('{');
        _writer.Write($"\"c\":{_state.Cycles},");
        _writer.Write($"\"cs\":\"{_state.CS:X4}\",");
        _writer.Write($"\"ip\":\"{_state.IP:X4}\",");
        _writer.Write($"\"op\":\"{Convert.ToHexString(opcode)}\",");
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
        _writer.Write($"\"fl\":\"{(ushort)_state.Flags.FlagRegister:X4}\"");
        _writer.WriteLine('}');
        _eventsWritten++;
    }

    public void Dispose() {
        Stop();
        _writer.Dispose();
    }
}
