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

    /// <summary>
    /// Auto-skip the boot intro by simulating Esc key-down events. When set,
    /// the harness hooks <c>cs1:0xDE54</c> (the engine's Esc consumer per
    /// Tech/45) and writes <c>ds:0xCEE8 = 1</c> (scancode 1 latch) on every
    /// entry — provided the engine hasn't yet landed on the configured
    /// stop-scene. The consumer reads the latch, sees scancode 1, treats it
    /// as Esc, and the boot intro's HNM playback skips to the next record.
    /// Net effect: instead of waiting ~75-90 wall-seconds for VIRGIN/CRYO/
    /// CREDITS/PRESENT/INTRO/IRULAN/palace-interlude/MTG1/MTG2 to play in
    /// real time, the harness skips them in &lt;5 sec and lands on the
    /// porch (scene 0) within ~30 sec total wall time.
    /// </summary>
    public bool SkipIntroViaEsc { get; set; } = false;

    /// <summary>
    /// Stop injecting Esc once <c>ds:0x47BE</c> (scene_id) reaches this value.
    /// Default 0 = stop when the porch loads (since scene 0 is the porch).
    /// Set to a higher number to also skip the first N gameplay scene
    /// transitions; set to -1 to inject forever (debug only).
    /// </summary>
    public int SkipIntroStopAtSceneId { get; set; } = 0;

    /// <summary>
    /// Boot-flow trace investigation (Tech/57). Installs checkpoint-independent
    /// first-hit, cycle-stamped probes (armed at emulation start, not gated on
    /// the checkpoint) on the boot/driver-load addresses, and a cycle cap on
    /// the constantly-hit intro poll sites so the run prints an ordered
    /// timeline and exits even if the engine spins in the boot intro. Answers
    /// "where does driver-load (cs1:0xE57B..0xE593 / 0xE675) sit relative to
    /// the cs1:0x000C checkpoint and the cs1:0x580 intro, and is it reached at
    /// all?". Read-only diagnostics — no emulation-behaviour change.
    /// </summary>
    public bool BootProbe { get; set; } = false;

    /// <summary>Cycle at which the boot-probe forces a summary + exit (default 60M — past the ~26M where the intro spin begins).</summary>
    public ulong BootProbeCycleCap { get; set; } = 60_000_000;
}

public enum HarnessMode {
    SnapshotOnCheckpoint,
    Invoke,
    Trace,

    /// <summary>
    /// Phase-40 enablement (Tech/57). A full boot to the checkpoint that lets
    /// the engine's init run the driver-load sequence (cs1:0xE57B..0xE593) so
    /// the project's per-driver-load-pass dump hook emits
    /// <c>spice86dumpMemoryDump_1000_E593_After_driver_load_pass_N.bin</c>.
    /// On checkpoint it snapshots, then <b>self-verifies</b> that the driver
    /// runtime segments (DNVGA→0xD000, DNPCS2/DNSBP→0xE000, DNMID/DNPCS→
    /// 0xF000) are actually non-zero (real driver binaries loaded, not
    /// replaced by C# overrides), prints a PASS/FAIL verdict + the list of
    /// per-pass dump files, and exits.
    /// </summary>
    DriverDump,
}

public enum CallConvention {
    /// <summary>Target is reached via a near return (RET, single-segment). Sentinel return = 16-bit IP only.</summary>
    NearCdecl,
    /// <summary>Target is reached via a far return (RETF, segment+offset). Sentinel return = far ptr.</summary>
    FarCdecl,
}
