namespace Cryogenic.Harness;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Run-time configuration for one harness invocation. Populated from CLI args by
/// <see cref="HarnessCli"/> and read by <see cref="HarnessOverrideSupplier"/>.
/// </summary>
/// <remarks>
/// Modes:
///   * <see cref="HarnessMode.SnapshotOnCheckpoint"/> — boot, run until the checkpoint
///     address is hit, dump RAM + CPU state to <see cref="OutputPath"/>, exit.
///   * <see cref="HarnessMode.Invoke"/> — boot to <see cref="Checkpoint"/> (the
///     "trampoline" address), then redirect execution to <see cref="InvokeTarget"/>
///     after pushing args + a sentinel return. When the sentinel is hit, dump the
///     resulting state and exit.
/// All addresses are real-mode segmented (segment, offset). For the post-driver-load
/// checkpoint that the project already dumps from `DefineMemoryDumpsMapping`, the
/// default is CS1:000C = 0x1000:0x000C.
/// </remarks>
public sealed class HarnessOptions {
    public HarnessMode Mode { get; set; } = HarnessMode.SnapshotOnCheckpoint;

    /// <summary>Address at which to capture the snapshot or arm the trampoline.</summary>
    public SegmentedAddress Checkpoint { get; set; } = new(0x1000, 0x000C);

    /// <summary>Directory to write the snapshot files into. Created if missing.</summary>
    public string OutputPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "snapshots", "default");

    /// <summary>Stop the emulation as soon as the snapshot has been written.</summary>
    public bool ExitOnSnapshot { get; set; } = true;

    /// <summary>Single-snapshot guard — once the snapshot is taken, ignore further hits.</summary>
    public bool SnapshotTaken { get; set; } = false;

    // ---- Invoke mode ---------------------------------------------------------

    /// <summary>Target function entry point.</summary>
    public SegmentedAddress InvokeTarget { get; set; } = new(0x0000, 0x0000);

    /// <summary>How the call returns to the sentinel.</summary>
    public CallConvention InvokeConvention { get; set; } = CallConvention.NearCdecl;

    /// <summary>Word-sized stack args (in push order, last entry is pushed last and is on top of the stack at function entry, i.e. is the first argument in C-cdecl).</summary>
    public List<ushort> InvokeStackArgs { get; set; } = new();

    /// <summary>Register pre-sets, e.g. {"AX", 0x1234}.</summary>
    public Dictionary<string, uint> InvokeRegisters { get; set; } = new();

    /// <summary>Sentinel address used as the return target. Hit ⇒ invoke complete.</summary>
    /// <remarks>
    /// For <see cref="CallConvention.NearCdecl"/> the segment is irrelevant on the
    /// stack (NEAR RET only pops IP), so the address actually hooked is
    /// <see cref="EffectiveSentinel"/> — the target's segment + this offset.
    /// </remarks>
    public SegmentedAddress InvokeSentinel { get; set; } = new(0x0000, 0xFFE0);

    /// <summary>The address actually hooked / actually hit by the RET.</summary>
    public SegmentedAddress EffectiveSentinel => InvokeConvention == CallConvention.NearCdecl
        ? new SegmentedAddress(InvokeTarget.Segment, InvokeSentinel.Offset)
        : InvokeSentinel;

    /// <summary>Hard cap on instruction count after the redirect (defence against runaway calls).</summary>
    public ulong InvokeBudgetInstructions { get; set; } = 5_000_000;

    /// <summary>Set true once the sentinel has been hit.</summary>
    public bool InvokeReturnCaptured { get; set; } = false;

    /// <summary>If non-null, dump RAM/state to this dir when the sentinel fires.</summary>
    public string? InvokeReturnDumpDir { get; set; } = null;

    /// <summary>Optional pre-sentinel snapshot — captured at trampoline time, before the redirect, so we have a 'before' state for memdiff.</summary>
    public bool DumpBeforeInvoke { get; set; } = false;

    /// <summary>Bytes to splat into memory at trampoline time, before the redirect.
    /// Keyed by linear address. Multiple --write-mem entries accumulate.</summary>
    public Dictionary<uint, byte[]> MemoryWrites { get; set; } = new();

    /// <summary>Enable per-instruction trace (ndjson, one record per instr).</summary>
    public bool TraceInstructions { get; set; } = false;

    /// <summary>Linear-address range for the instruction trace (inclusive).</summary>
    public uint InstructionTraceStart { get; set; } = 0x10000;

    public uint InstructionTraceEnd { get; set; } = 0x1FFFF;

    /// <summary>Linear addresses to watch for memory writes; each fires an ndjson event with CS:IP + new value.</summary>
    public List<uint> WatchAddresses { get; set; } = new();

    // ---- Trace mode ----------------------------------------------------------

    /// <summary>Function entries to log on hit; each fires an ndjson event with cycles + regs + DS:SI excerpt.</summary>
    public List<FunctionEntryTrace.Entry> TraceFunctions { get; set; } = new();

    /// <summary>Number of cycles after the checkpoint at which to exit. 0 = no limit (run until next breakpoint exits).</summary>
    public ulong MaxCyclesAfterCheckpoint { get; set; } = 0;

    /// <summary>Set true once the trace's max-cycles cap fires, so we don't double-exit.</summary>
    public bool TraceCapTripped { get; set; } = false;
}

public enum HarnessMode {
    SnapshotOnCheckpoint,
    Invoke,
    Trace,
}

public enum CallConvention {
    /// <summary>Target is reached via a near return (RET, single-segment). Sentinel return = 16-bit IP only.</summary>
    NearCdecl,
    /// <summary>Target is reached via a far return (RETF, segment+offset). Sentinel return = far ptr.</summary>
    FarCdecl,
}
