namespace Cryogenic.Harness;

using Spice86.Core.CLI;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.ReverseEngineer;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Emulator.VM.Breakpoint;
using Spice86.Shared.Interfaces;

/// <summary>
/// Installs harness-specific hooks on top of whatever the main project's
/// <c>Cryogenic.Overrides.Overrides</c> registered. Lives alongside the regular
/// override registration in the same <see cref="FunctionInformation"/> dictionary
/// so Spice86 dispatches to both.
/// </summary>
/// <remarks>
/// Phase 1: snapshot at checkpoint.
/// Phase 2: redirect into a target function (Invoke mode), capture return state.
/// Phase 3: trace pipe — RAM memdiff + per-known-function entry trace +
///         (best-effort) ExecutionFlowRecorder via reflection.
/// </remarks>
public sealed class HarnessHelper : CSharpOverrideHelper {
    private readonly HarnessOptions _options;
    private readonly DirectInvoker _invoker;
    private readonly CallTrace _calls = new();
    private readonly IDictionary<SegmentedAddress, FunctionInformation> _knownFunctions;
    private InvokeSnapshot? _preInvoke;
    private byte[]? _preInvokeRam;
    private InstructionTrace? _instrTrace;
    private MemWriteWatch? _watch;
    private FunctionEntryTrace? _fnTrace;
    private long _maxCyclesCap;

    // Boot-flow probe (Tech/57): checkpoint-independent first-hit timeline.
    private readonly List<(ushort Off, string Label)> _bootProbeAddrs = new();
    private readonly Dictionary<ushort, long> _bootProbeFirst = new();
    private readonly Dictionary<ushort, long> _bootProbeHits = new();
    private bool _bootProbeSummarized;

    public HarnessHelper(
        IDictionary<SegmentedAddress, FunctionInformation> functionInformations,
        Machine machine,
        ILoggerService loggerService,
        Configuration configuration,
        HarnessOptions options)
        : base(functionInformations, machine, loggerService, configuration) {
        _options = options;
        _invoker = new DirectInvoker(_options);
        _knownFunctions = functionInformations;
        InstallHooks();
    }

    // Trace counters for harness-fwd diagnostics (cs1:0x580 play_intro,
    // cs1:0x93F record reader). These are passive hooks that fire on
    // top of the native instruction without replacing it; used only
    // when CRYO_HARNESS_FAST_HNM is set to confirm the dispatcher is
    // progressing through records.
    private int _playIntroHits;
    private int _recordReaderHits;

    private void InstallHooks() {
        SegmentedAddress cp = _options.Checkpoint;
        DoOnTopOfInstruction(cp.Segment, cp.Offset, OnCheckpointHit);
        Console.Error.WriteLine($"[harness] checkpoint armed @ {cp.Segment:X4}:{cp.Offset:X4}");

        if (_options.SkipIntroViaEsc) {
            InstallIntroSkipHook();
        }

        if (_options.BootProbe) {
            InstallBootProbe();
        }

        if (_options.StallSample) {
            InstallStallSampler();
        }

        // Harness-fwd diagnostic hooks. Passive — these run on top of
        // the native instruction and don't replace it. Used to localise
        // where the boot intro stalls under headless emulation.
        if (Environment.GetEnvironmentVariable("CRYO_HARNESS_FAST_HNM") is not null) {
            var traceAddrs = new (ushort Off, string Label)[] {
                (0x0580, "play_intro_entry"),
                (0x0585, "after_de54_call"),
                (0x0589, "after_far_3959"),
                (0x058C, "after_aeb7_call"),
                (0x0592, "after_call_945"),
                (0x0599, "before_call_93F_record_read"),
                (0x059C, "after_call_93F"),
                (0x05A8, "after_jnz_to_5A3"),
                (0x05AB, "after_call_911"),
                (0x05DC, "after_call_C07C"),
                (0x05E4, "before_call_DD63"),
                (0x05E9, "after_call_DD63"),
                (0x05F6, "before_jz_to_592"),
                (0x05FD, "exit_via_pushf"),
                (0x093F, "record_reader_93F"),
                (0x0945, "store_si_to_4854"),
                (0x0925, "init_46d7_write"),  // mov byte [0x46d7], 0
                (0x0798, "boot_helper_timer_set"),  // boot_helper_timer_set writes [0x4780]
                (0x061C, "load_VIRGIN_HNM_entry"),  // record 0's helperA
                (0xCA1B, "hnm_load_entry"),         // confirm jmp lands here
                (0xCC85, "CheckIfHnmComplete_entry"),
                (0xC9F4, "do_frame_entry"),
                (0xCA60, "hnm_do_frame_entry"),
                (0x0798, "BOOT_TIMER_SET"),     // boot_helper_timer_set — should fire on records 15,16,22,23,25,26
                (0x05ED, "DISPATCHER_CALL_AX"), // the actual record callTarget dispatch
            };
            var hitCounts = new System.Collections.Generic.Dictionary<ushort, int>();
            foreach (var (off, label) in traceAddrs) {
                var capturedOff = off;
                var capturedLabel = label;
                hitCounts[off] = 0;
                DoOnTopOfInstruction(0x1000, off, () => {
                    hitCounts[capturedOff]++;
                    int n = hitCounts[capturedOff];
                    if (n <= 3 || n % 200 == 0) {
                        Console.Error.WriteLine(
                            $"[harness-fwd] {capturedOff:X4} {capturedLabel} #{n} (ax={State.AX:X4} bx={State.BX:X4} si={State.SI:X4} ds={State.DS:X4} es={State.ES:X4})");
                    }
                });
            }
            Console.Error.WriteLine($"[harness-fwd] diagnostic hooks armed on {traceAddrs.Length} addresses");
        }

        // Watch memory writes BEFORE the invoke trampoline so we catch loader writes
        // that happen during boot (e.g. decoder-pointer patches at DS:0x38FD).
        if (_options.WatchAddresses.Count > 0) {
            string watchPath = Path.Combine(
                _options.InvokeReturnDumpDir ?? _options.OutputPath,
                "memwrites.ndjson");
            _watch = new MemWriteWatch(EmulatorBreakpointsManager, State, Memory, watchPath);
            _watch.Arm(_options.WatchAddresses);
            Console.Error.WriteLine(
                $"[harness] mem-write watch armed on {_options.WatchAddresses.Count} address(es) → {watchPath}");
        }

        if (_options.Mode == HarnessMode.Invoke) {
            SegmentedAddress sn = _options.EffectiveSentinel;
            DoOnTopOfInstruction(sn.Segment, sn.Offset, OnSentinelHit);
            Console.Error.WriteLine($"[harness] sentinel armed   @ {sn.Segment:X4}:{sn.Offset:X4} ({_options.InvokeConvention})");
            Console.Error.WriteLine($"[harness] invoke target    @ {_options.InvokeTarget.Segment:X4}:{_options.InvokeTarget.Offset:X4}");

            InstallKnownFunctionTraces();
        }
    }

    /// <summary>
    /// Auto-skip the boot intro by hooking the engine's Esc consumer at
    /// <c>cs1:0xDE54</c> and stuffing scancode 1 into the latch at
    /// <c>ds:0xCEE8</c> before each read. The consumer (decoded in
    /// Tech/45) checks the latch byte; if it equals 1 it reports
    /// "Esc was pressed" via ZF=1, which the boot intro's HNM playback
    /// + IRULAN voice prompts + palace interlude treat as a skip
    /// command.
    ///
    /// Boot sequence:
    ///   cs1:0x000C   FB (sti) — harness checkpoint
    ///   cs1:0x000D   call play_intro (cs1:0x580)        ← Esc skips HNMs
    ///   cs1:0x0010   call 0x0309
    ///   cs1:0x0013   call play_intro2 (cs1:0x21C)       ← Esc skips IRULAN
    ///                play_intro2 NEVER returns — it `jmp 0x8F0`
    ///   cs1:0x08F0   ★ main game loop entry (gameplay starts here)
    ///
    /// So the "intro done" signal is "cs1:0x8F0 has been reached". We
    /// hook 0x8F0 to flip a flag; until then, every cs1:0xDE54 call gets
    /// Esc-injected. Once 0x8F0 fires, the hook becomes a no-op so the
    /// main game loop's natural input flow (if any) is preserved.
    /// </summary>
    private void InstallIntroSkipHook() {
        // Linear addresses derived from the engine's data segment layout
        // (DS = 0x2000 paragraph at boot, per spice86dumpMemoryDump.bin).
        const uint LinearEscLatch = 0x20000 + 0xCEE8;
        long injections = 0;
        bool introDone = false;

        // cs1:0x08F0 is the main game loop entry that play_intro2 jmps
        // to after IRULAN finishes (or after Esc-skip).
        DoOnTopOfInstruction(0x1000, 0x08F0, () => {
            if (introDone) return;
            introDone = true;
            Console.Error.WriteLine(
                $"[harness] intro-skip: reached cs1:0x08F0 (main game loop) after {injections} Esc injections, cycles={State.Cycles}");
        });

        // cs1:0xDE54 is the Esc consumer per Tech/45.
        DoOnTopOfInstruction(0x1000, 0xDE54, () => {
            if (introDone) return;
            Memory.UInt8[LinearEscLatch] = 0x01;
            injections++;
            if (injections == 1 || injections % 1000 == 0) {
                Console.Error.WriteLine(
                    $"[harness] esc-injection #{injections} cycles={State.Cycles}");
            }
        });
        Console.Error.WriteLine(
            "[harness] intro-skip armed — injecting Esc at cs1:0xDE54 until cs1:0x8F0 (main loop entry)");
    }

    /// <summary>
    /// For every address that has a registered FunctionInformation, install a passive
    /// "function entered" trace hook. Triggers only when <see cref="CallTrace.Start"/>
    /// has been called (i.e. we're inside the invoke window).
    /// </summary>
    private void InstallKnownFunctionTraces() {
        // Snapshot the dict — installing hooks below mutates the underlying registration
        // map, and we don't want our own trace addresses to recurse.
        SegmentedAddress[] addrs = _knownFunctions.Keys.ToArray();
        int installed = 0;
        foreach (SegmentedAddress addr in addrs) {
            string? label = _knownFunctions[addr].Name;
            // Don't trace our own checkpoint or sentinel.
            if (addr.Equals(_options.Checkpoint) || addr.Equals(_options.EffectiveSentinel)) {
                continue;
            }
            try {
                DoOnTopOfInstruction(addr.Segment, addr.Offset, () => _calls.OnFunctionEntry(addr, State, label));
                installed++;
            } catch {
                // Some addresses can't be hooked (non-instruction boundaries) — skip.
            }
        }
        Console.Error.WriteLine($"[harness] function-entry traces armed: {installed}/{addrs.Length}");
    }

    /// <summary>
    /// Tech/57 boot-flow trace. First-hit, cycle-stamped probes on the
    /// boot/driver-load path, armed at emulation start (NOT gated on the
    /// checkpoint, which the prior run never reached). A cycle cap on the
    /// constantly-hit intro poll sites (cs1:0xDE54 Esc consumer, cs1:0x0580
    /// play_intro) forces an ordered-timeline summary + exit even when the
    /// engine spins in the boot intro. Read-only — no behaviour change.
    /// </summary>
    private void InstallBootProbe() {
        _bootProbeAddrs.AddRange(new (ushort, string)[] {
            (0x000C, "checkpoint / post-driver-load entry"),
            (0x000D, "call play_intro (instr after 0x000C)"),
            (0xE675, "open_dune_dat (DUNE.DAT/driver open orchestrator)"),
            (0xF1FB, "subLoadSavegame (resolve+open+size)"),
            (0xF20D, "dnmaj-open INT21 return site (driver-file probe)"),
            (0xE57B, "driver-load: RemapDrivers hook point"),
            (0xE589, "driver-load: ReadDriverFunctionTable"),
            (0xE593, "driver-load: ResetAllocator / ret (dump hook)"),
            (0x0580, "play_intro (boot intro VM)"),
            (0x08F0, "main game loop entry"),
        });
        foreach ((ushort off, string label) in _bootProbeAddrs) {
            _bootProbeFirst[off] = -1;
            _bootProbeHits[off] = 0;
            ushort capturedOff = off;
            string capturedLabel = label;
            DoOnTopOfInstruction(0x1000, capturedOff, () => {
                _bootProbeHits[capturedOff]++;
                if (_bootProbeFirst[capturedOff] < 0) {
                    _bootProbeFirst[capturedOff] = State.Cycles;
                    Console.Error.WriteLine(
                        $"[bootprobe] FIRST cs1:0x{capturedOff:X4} {capturedLabel} " +
                        $"cycles={State.Cycles} (ax={State.AX:X4} bx={State.BX:X4} " +
                        $"si={State.SI:X4} ds={State.DS:X4} es={State.ES:X4})");
                }
                MaybeBootProbeCap();
            });
        }
        Console.Error.WriteLine(
            $"[bootprobe] armed {_bootProbeAddrs.Count} boot-flow probes (cycle cap={_options.BootProbeCycleCap})");
    }

    private void MaybeBootProbeCap() {
        if (_bootProbeSummarized) {
            return;
        }
        if ((ulong)State.Cycles >= _options.BootProbeCycleCap) {
            DumpBootProbeSummary($"CYCLE-CAP at {State.Cycles}");
            Exit();
        }
    }

    private void DumpBootProbeSummary(string reason) {
        if (_bootProbeSummarized) {
            return;
        }
        _bootProbeSummarized = true;
        Console.Error.WriteLine($"[bootprobe] ===== TIMELINE SUMMARY ({reason}) =====");
        foreach ((ushort off, string label) in _bootProbeAddrs) {
            long first = _bootProbeFirst[off];
            long hits = _bootProbeHits[off];
            Console.Error.WriteLine(first < 0
                ? $"[bootprobe]   cs1:0x{off:X4}  NOT HIT            — {label}"
                : $"[bootprobe]   cs1:0x{off:X4}  first@{first,-12} hits={hits,-10} — {label}");
        }
        Console.Error.WriteLine("[bootprobe] ===== END SUMMARY =====");
    }

    // Stall-localiser (Tech/57): periodic CYCLES-breakpoint PC sampler.
    private int _stallSamples;
    private readonly Dictionary<uint, int> _stallPcHist = new();

    private void InstallStallSampler() {
        Console.Error.WriteLine(
            $"[stall] sampler armed — every {_options.StallSampleInterval} cycles, " +
            $"max {_options.StallSampleMax} samples (also bound the run with --StopAfterCycles)");
        ScheduleNextStallSample((long)_options.StallSampleInterval);
    }

    private void ScheduleNextStallSample(long targetCycle) {
        AddressBreakPoint bp = new(
            BreakPointType.CPU_CYCLES,
            targetCycle,
            _ => OnStallSample(),
            isRemovedOnTrigger: true);
        EmulatorBreakpointsManager.ToggleBreakPoint(bp, on: true);
    }

    private void OnStallSample() {
        _stallSamples++;
        ushort cs = State.CS;
        ushort ip = State.IP;
        uint lin = (uint)((cs << 4) + ip);
        _stallPcHist[lin] = _stallPcHist.TryGetValue(lin, out int c) ? c + 1 : 1;
        Console.Error.WriteLine(
            $"[stall] #{_stallSamples} cyc={State.Cycles} {cs:X4}:{ip:X4} (lin={lin:X5}) " +
            $"ax={State.AX:X4} bx={State.BX:X4} cx={State.CX:X4} dx={State.DX:X4} " +
            $"si={State.SI:X4} di={State.DI:X4} bp={State.BP:X4} " +
            $"ds={State.DS:X4} es={State.ES:X4} ss={State.SS:X4} sp={State.SP:X4}");

        if (_stallSamples >= _options.StallSampleMax) {
            Console.Error.WriteLine("[stall] ===== TOP SAMPLED PCs (the spin loop) =====");
            foreach (KeyValuePair<uint, int> kv in _stallPcHist
                         .OrderByDescending(k => k.Value).Take(15)) {
                Console.Error.WriteLine(
                    $"[stall]   lin 0x{kv.Key:X5}  ×{kv.Value}");
            }
            Console.Error.WriteLine("[stall] ===== END (max samples reached) =====");
            Exit();
            return;
        }
        ScheduleNextStallSample(State.Cycles + (long)_options.StallSampleInterval);
    }

    private void OnCheckpointHit() {
        if (_options.BootProbe) {
            DumpBootProbeSummary("CHECKPOINT cs1:000C HIT");
        }
        switch (_options.Mode) {
            case HarnessMode.SnapshotOnCheckpoint:
                HandleSnapshotMode();
                break;
            case HarnessMode.Invoke:
                HandleInvokeTrampoline();
                break;
            case HarnessMode.Trace:
                HandleTraceArm();
                break;
            case HarnessMode.DriverDump:
                HandleDriverDumpMode();
                break;
        }
    }

    /// <summary>
    /// Phase-40 enablement (Tech/57). The checkpoint is reached only after the
    /// engine's init has run the driver-load sequence, so the project's
    /// <c>cs1:0xE593</c> per-driver-load-pass hook has already emitted the
    /// <c>..._E593_After_driver_load_pass_N.bin</c> dumps. Here we snapshot,
    /// then self-verify that the driver runtime segments actually hold code
    /// (real binaries loaded, not shadowed by C# overrides) and report a
    /// definitive verdict + the list of per-pass dumps, so a single run tells
    /// the operator whether Phase-40 RE can proceed from real bytes.
    /// </summary>
    private void HandleDriverDumpMode() {
        if (_options.SnapshotTaken) {
            return;
        }
        _options.SnapshotTaken = true;

        SnapshotMeta meta = Snapshot.Save(_options.OutputPath, Memory, State, _options.Checkpoint);
        Console.Error.WriteLine(
            $"[harness] driver-dump snapshot → {_options.OutputPath} (cycles={State.Cycles})");

        (string name, uint lin)[] segs = {
            ("DNVGA  @0xD000", 0xD0000u),
            ("DNPCS2 @0xE000", 0xE0000u),
            ("DNMID  @0xF000", 0xF0000u),
        };
        bool allLoaded = true;
        foreach ((string name, uint lin) in segs) {
            byte[] sample = Memory.ReadRam(64u, lin);
            int nz = 0;
            foreach (byte b in sample) {
                if (b != 0) {
                    nz++;
                }
            }
            bool loaded = nz > 0;
            allLoaded &= loaded;
            Console.Error.WriteLine(
                $"[harness] {name}: {nz}/64 nonzero — {(loaded ? "LOADED" : "ALL-ZERO (driver not resident)")}");
        }

        string dumpDir = Configuration.RecordedDataDirectory ?? Environment.CurrentDirectory;
        string[] passDumps;
        try {
            passDumps = Directory.GetFiles(dumpDir, "spice86dumpMemoryDump_1000_E593_After_driver_load_pass_*.bin");
        } catch {
            passDumps = Array.Empty<string>();
        }
        Array.Sort(passDumps);
        Console.Error.WriteLine(
            $"[harness] per-driver-load-pass dumps ({passDumps.Length}) in {dumpDir}:");
        foreach (string p in passDumps) {
            Console.Error.WriteLine($"[harness]   {Path.GetFileName(p)}");
        }

        Console.Error.WriteLine(allLoaded && passDumps.Length > 0
            ? "[harness] DRIVER-DUMP PASS — real driver binaries resident; Phase-40 static RE can proceed."
            : "[harness] DRIVER-DUMP FAIL — driver segments empty and/or no pass dumps. " +
              "The run replaced the drivers with C# overrides or didn't reach the driver-load path; " +
              "re-run a full boot with the real driver binaries (see Tech/57).");

        Exit();
    }

    private void HandleTraceArm() {
        if (_fnTrace is not null) {
            return; // already armed
        }
        if (_options.TraceFunctions.Count == 0) {
            Console.Error.WriteLine("[harness] trace mode but no --trace-fn entries; nothing to do");
            Exit();
            return;
        }

        Directory.CreateDirectory(_options.OutputPath);
        string tracePath = Path.Combine(_options.OutputPath, "fn-trace.ndjson");
        _fnTrace = new FunctionEntryTrace(EmulatorBreakpointsManager, State, Memory, tracePath);
        _fnTrace.Arm(_options.TraceFunctions);
        Console.Error.WriteLine(
            $"[harness] trace armed on {_options.TraceFunctions.Count} address(es) → {tracePath}");

        if (_options.MaxCyclesAfterCheckpoint > 0) {
            // Spice86 11.1.0 has no CYCLES breakpoint; we poll on each fn-trace
            // hit (cheap because it's already in a callback). Set a tripwire
            // that the FunctionEntryTrace breakpoints will check.
            _maxCyclesCap = State.Cycles + (long)_options.MaxCyclesAfterCheckpoint;
            _fnTrace.SetCycleCap(_maxCyclesCap, OnTraceCapHit);
            Console.Error.WriteLine($"[harness] max-cycles cap = {_maxCyclesCap} (now={State.Cycles}, +{_options.MaxCyclesAfterCheckpoint})");
        }
    }

    private void OnTraceCapHit() {
        if (_options.TraceCapTripped) {
            return;
        }
        _options.TraceCapTripped = true;
        long n = _fnTrace?.EventsWritten ?? 0;
        _fnTrace?.Stop();
        if (_watch is not null) {
            long w = _watch.EventsWritten;
            _watch.Stop();
            Console.Error.WriteLine($"[harness] mem-write watch: {w} events");
        }
        Console.Error.WriteLine(
            $"[harness] max-cycles reached at {State.Cycles} — fn-trace events: {n}; exiting");
        Exit();
    }

    private void HandleSnapshotMode() {
        if (_options.SnapshotTaken) {
            return;
        }
        _options.SnapshotTaken = true;

        SnapshotMeta meta = Snapshot.Save(_options.OutputPath, Memory, State, _options.Checkpoint);
        Console.Error.WriteLine(
            $"[harness] snapshot written → {_options.OutputPath} " +
            $"(ram_sha256={meta.RamSha256[..16]}…, cycles={State.Cycles})");

        if (_watch is not null) {
            long n = _watch.EventsWritten;
            _watch.Stop();
            Console.Error.WriteLine($"[harness] mem-write watch: {n} events");
        }

        if (_options.ExitOnSnapshot) {
            Console.Error.WriteLine("[harness] ExitOnSnapshot=true — exiting emulator");
            Exit();
        }
    }

    private void HandleInvokeTrampoline() {
        if (_preInvoke is not null) {
            return; // single-shot guard.
        }

        if (_options.DumpBeforeInvoke) {
            Snapshot.Save(Path.Combine(_options.OutputPath, "pre-invoke"), Memory, State, _options.Checkpoint);
        }

        // Apply requested memory writes BEFORE we snapshot RAM, so they're considered
        // part of the "pre" image and don't show up as diffs.
        foreach (KeyValuePair<uint, byte[]> kv in _options.MemoryWrites) {
            Memory.WriteRam(kv.Value, kv.Key);
        }

        // Capture pre-invoke RAM for memdiff at sentinel time.
        _preInvokeRam = Memory.ReadRam(Snapshot.RamSize, 0);

        _preInvoke = _invoker.Setup(Memory, State, Stack);
        _calls.Start(Machine);

        if (_options.TraceInstructions) {
            string instrPath = Path.Combine(_options.InvokeReturnDumpDir ?? _options.OutputPath, "instr.ndjson");
            _instrTrace = new InstructionTrace(EmulatorBreakpointsManager, State, Memory, _options.InstructionTraceStart, _options.InstructionTraceEnd, instrPath);
            _instrTrace.Start();
            Console.Error.WriteLine($"[harness] instruction trace armed for [0x{_options.InstructionTraceStart:X5}..0x{_options.InstructionTraceEnd:X5}] → {instrPath}");
        }

        Console.Error.WriteLine(
            $"[harness] redirecting → {_options.InvokeTarget.Segment:X4}:{_options.InvokeTarget.Offset:X4} " +
            $"(SP={State.SP:X4}, sentinel={_options.InvokeSentinel.Segment:X4}:{_options.InvokeSentinel.Offset:X4}, " +
            $"args={_options.InvokeStackArgs.Count}, regs={_options.InvokeRegisters.Count})");
    }

    private void OnSentinelHit() {
        if (_preInvoke is null) {
            Console.Error.WriteLine("[harness] sentinel hit before redirect — exiting");
            Exit();
            return;
        }
        if (_options.InvokeReturnCaptured) {
            return;
        }
        _options.InvokeReturnCaptured = true;
        _calls.Stop();
        if (_instrTrace is not null) {
            long count = _instrTrace.EventsWritten;
            _instrTrace.Stop();
            Console.Error.WriteLine($"[harness] instruction trace: {count} events");
        }
        if (_watch is not null) {
            long n = _watch.EventsWritten;
            _watch.Stop();
            Console.Error.WriteLine($"[harness] mem-write watch: {n} events");
        }

        InvokeReturn ret = _invoker.CaptureReturn(Memory, State, _preInvoke);
        Console.Error.WriteLine(
            $"[harness] sentinel hit — invoke complete in {ret.BudgetCyclesUsed} cycles, " +
            $"AX={ret.Post.EAX & 0xFFFFu:X4} BX={ret.Post.EBX & 0xFFFFu:X4} CX={ret.Post.ECX & 0xFFFFu:X4} DX={ret.Post.EDX & 0xFFFFu:X4}");

        // Memory diff against pre-invoke RAM.
        if (_preInvokeRam is not null) {
            byte[] postRam = Memory.ReadRam(Snapshot.RamSize, 0);
            MemDiffResult diff = MemDiff.Compute(_preInvokeRam, postRam);
            string diffDir = _options.InvokeReturnDumpDir ?? _options.OutputPath;
            MemDiff.Save(diffDir, diff);
            Console.Error.WriteLine(
                $"[harness] memdiff: {diff.ChangedRegionCount} regions, {diff.ChangedByteCount} bytes changed");
        }

        // Call-graph dump.
        string callsDir = _options.InvokeReturnDumpDir ?? _options.OutputPath;
        _calls.Save(callsDir);
        Console.Error.WriteLine($"[harness] call trace: {_calls.Events.Count} known-function entries recorded");

        // Optional return-state RAM dump.
        if (_options.InvokeReturnDumpDir is not null) {
            SnapshotMeta meta = Snapshot.Save(_options.InvokeReturnDumpDir, Memory, State, _options.InvokeSentinel);
            Console.Error.WriteLine($"[harness] return-state snapshot → {_options.InvokeReturnDumpDir} (ram_sha256={meta.RamSha256[..16]}…)");
        }

        Exit();
    }
}
