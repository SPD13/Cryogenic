namespace Cryogenic.Harness;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Argument parser for harness-specific options. Anything we recognise we strip
/// out of the argv and translate into <see cref="HarnessOptions"/> + extra Spice86
/// CLI flags. Anything we don't recognise is passed straight through to Spice86
/// (so flags like <c>-e DNCDPRG.EXE</c> work like in the main project).
/// </summary>
public static class HarnessCli {
    /// <summary>
    /// Parse argv, mutating <see cref="HarnessContext.Options"/> and returning the
    /// argv to forward to Spice86.Program.RunWithOverrides.
    /// </summary>
    public static string[] Parse(string[] args) {
        List<string> forwarded = new();
        HarnessOptions opt = HarnessContext.Options;

        for (int i = 0; i < args.Length; i++) {
            string a = args[i];
            string? next = i + 1 < args.Length ? args[i + 1] : null;

            switch (a) {
                case "--checkpoint":
                    if (next is null) {
                        throw new ArgumentException("--checkpoint requires SEGMENT:OFFSET (e.g. 1000:000C)");
                    }
                    opt.Checkpoint = ParseSegmentedAddress(next);
                    i++;
                    break;
                case "--snapshot-out":
                    if (next is null) {
                        throw new ArgumentException("--snapshot-out requires a directory path");
                    }
                    opt.OutputPath = next;
                    i++;
                    break;
                case "--no-exit":
                    opt.ExitOnSnapshot = false;
                    break;
                case "--mode":
                    if (next is null) {
                        throw new ArgumentException("--mode requires a mode name");
                    }
                    opt.Mode = ParseMode(next);
                    i++;
                    break;
                case "--target":
                    if (next is null) {
                        throw new ArgumentException("--target requires SEGMENT:OFFSET");
                    }
                    opt.InvokeTarget = ParseSegmentedAddress(next);
                    i++;
                    break;
                case "--sentinel":
                    if (next is null) {
                        throw new ArgumentException("--sentinel requires SEGMENT:OFFSET");
                    }
                    opt.InvokeSentinel = ParseSegmentedAddress(next);
                    i++;
                    break;
                case "--call-convention":
                    if (next is null) {
                        throw new ArgumentException("--call-convention requires near|far");
                    }
                    opt.InvokeConvention = next.ToLowerInvariant() switch {
                        "near" or "near-cdecl" => CallConvention.NearCdecl,
                        "far" or "far-cdecl" => CallConvention.FarCdecl,
                        _ => throw new ArgumentException($"unknown call convention '{next}'"),
                    };
                    i++;
                    break;
                case "--arg-stack":
                    if (next is null) {
                        throw new ArgumentException("--arg-stack requires a 16-bit hex value");
                    }
                    opt.InvokeStackArgs.Add(ParseUShort(next));
                    i++;
                    break;
                case "--arg-reg":
                    if (next is null) {
                        throw new ArgumentException("--arg-reg requires REG=VALUE (e.g. AX=0x1234)");
                    }
                    (string regName, uint regVal) = ParseRegisterAssignment(next);
                    opt.InvokeRegisters[regName] = regVal;
                    i++;
                    break;
                case "--invoke-budget":
                    if (next is null) {
                        throw new ArgumentException("--invoke-budget requires a number of instructions");
                    }
                    opt.InvokeBudgetInstructions = ulong.Parse(next);
                    i++;
                    break;
                case "--return-dump":
                    if (next is null) {
                        throw new ArgumentException("--return-dump requires a directory path");
                    }
                    opt.InvokeReturnDumpDir = next;
                    i++;
                    break;
                case "--dump-before-invoke":
                    opt.DumpBeforeInvoke = true;
                    break;
                case "--trace-instr":
                    opt.TraceInstructions = true;
                    break;
                case "--write-mem":
                    if (next is null) {
                        throw new ArgumentException("--write-mem requires LINEAR=HEXBYTES (e.g. 0x23000=DEADBEEF)");
                    }
                    (uint wmAddr, byte[] wmBytes) = ParseMemoryWrite(next);
                    opt.MemoryWrites[wmAddr] = wmBytes;
                    i++;
                    break;
                case "--instr-range":
                    if (next is null) {
                        throw new ArgumentException("--instr-range requires START-END (e.g. 0x10000-0x1FFFF)");
                    }
                    (opt.InstructionTraceStart, opt.InstructionTraceEnd) = ParseLinearRange(next);
                    i++;
                    break;
                case "--watch-mem":
                    if (next is null) {
                        throw new ArgumentException("--watch-mem requires LINEAR or LINEAR..LINEAR (inclusive)");
                    }
                    foreach (uint addr in ParseLinearRangeOrSingle(next)) {
                        opt.WatchAddresses.Add(addr);
                    }
                    i++;
                    break;
                case "--trace-fn":
                    if (next is null) {
                        throw new ArgumentException("--trace-fn requires SEG:OFF[:LABEL[:DS_SI_BYTES]]");
                    }
                    opt.TraceFunctions.Add(ParseTraceEntry(next));
                    i++;
                    break;
                case "--max-cycles":
                    if (next is null) {
                        throw new ArgumentException("--max-cycles requires a non-negative integer");
                    }
                    opt.MaxCyclesAfterCheckpoint = ulong.Parse(next, System.Globalization.NumberStyles.Integer);
                    i++;
                    break;
                case "--skip-intro":
                    opt.SkipIntroViaEsc = true;
                    break;
                case "--skip-intro-until":
                    if (next is null) {
                        throw new ArgumentException("--skip-intro-until requires a scene_id (decimal or 0xHEX)");
                    }
                    opt.SkipIntroViaEsc = true;
                    opt.SkipIntroStopAtSceneId = (int)ParseUInt(next);
                    i++;
                    break;
                case "-h":
                case "--help":
                case "--harness-help":
                    PrintHelp();
                    Environment.Exit(0);
                    break;
                default:
                    forwarded.Add(a);
                    break;
            }
        }

        // Inject the Spice86 flags the harness needs. Note:
        //   * RunWithOverrides<T> already supplies --OverrideSupplierClassName, so we don't repeat it.
        //   * `--HeadlessMode=Minimal` runs without any UI.
        //   * `--GdbPort=0` disables the GDB server (otherwise port-10000 collisions happen
        //     when one harness run is still spinning down while the next starts).
        //   * `--RecordedDataDirectory` keeps Spice86's own dumps out of the source tree;
        //     the user can override by passing it explicitly later in the argv.
        forwarded.Add("--UseCodeOverride=true");
        forwarded.Add("--HeadlessMode=Minimal");
        forwarded.Add("--GdbPort=0");
        string spice86Dir = Path.Combine(opt.OutputPath, "_spice86");
        Directory.CreateDirectory(spice86Dir); // Cryogenic project's own dumps need this to exist.
        forwarded.Add($"--RecordedDataDirectory={spice86Dir}");

        return forwarded.ToArray();
    }

    private static SegmentedAddress ParseSegmentedAddress(string s) {
        string[] parts = s.Split(':');
        if (parts.Length != 2) {
            throw new ArgumentException($"expected SEGMENT:OFFSET, got '{s}'");
        }
        ushort seg = ushort.Parse(parts[0], System.Globalization.NumberStyles.HexNumber);
        ushort off = ushort.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
        return new SegmentedAddress(seg, off);
    }

    private static HarnessMode ParseMode(string s) => s.ToLowerInvariant() switch {
        "snapshot" or "snapshot-on-checkpoint" => HarnessMode.SnapshotOnCheckpoint,
        "invoke" => HarnessMode.Invoke,
        "trace" => HarnessMode.Trace,
        "driver-dump" or "driverdump" => HarnessMode.DriverDump,
        _ => throw new ArgumentException($"unknown harness mode '{s}'"),
    };

    private static FunctionEntryTrace.Entry ParseTraceEntry(string s) {
        // Accept: "1000:CC96", "1000:CC96:hnm_decode", "1000:CC96:hnm_decode:32",
        // "1000:CC96:hnm_decode:32@dc00=0x2D0B0;2"
        // The outer SEG:OFF[:LABEL[:BYTES]] format splits on ':' so the BYTES
        // segment-selector uses ';' as its internal separator:
        //   "32"            → read 32 bytes at DS:SI (back-compat)
        //   "ds;32"         → same as bare 32
        //   "cs;32"         → read 32 bytes at CS:SI instead
        //   "ds;32+cs;48"   → read both (DS:SI 32 bytes + CS:SI 48 bytes)
        // Trailing "@..." part supports two read-spec forms:
        //   "name=LINEAR;BYTES"        (existing — fixed linear address)
        //   "name=SEG;REG;BYTES"       (NEW — register-relative, e.g. CS;BP;4)
        string[] readParts = s.Split('@');
        string head = readParts[0];
        string[] parts = head.Split(':');
        if (parts.Length < 2) {
            throw new ArgumentException($"--trace-fn: expected SEG:OFF[:LABEL[:BYTES][@reads]], got '{s}'");
        }
        ushort seg = ushort.Parse(parts[0], System.Globalization.NumberStyles.HexNumber);
        ushort off = ushort.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
        uint linear = (uint)((seg << 4) + off);
        string label = parts.Length >= 3 ? parts[2] : $"{seg:X4}_{off:X4}";
        int dsSi = 0;
        int csSi = 0;
        if (parts.Length >= 4) {
            // Tokens like "32", "cs;48", "ds;32+cs;48"
            foreach (string tok in parts[3].Split('+')) {
                string t = tok.Trim();
                if (t.Length == 0) continue;
                int semi = t.IndexOf(';');
                if (semi < 0) { dsSi = int.Parse(t); continue; }
                string segName = t[..semi].ToLowerInvariant();
                int n = int.Parse(t[(semi + 1)..]);
                if (segName == "ds") dsSi = n;
                else if (segName == "cs") csSi = n;
                else throw new ArgumentException($"--trace-fn: unknown segment '{segName}' in BYTES (use ds; or cs;)");
            }
        }
        List<(string, uint, int)>? reads = null;
        List<FunctionEntryTrace.SegRegRead>? segReads = null;
        if (readParts.Length > 1) {
            foreach (string entry in readParts[1].Split(',')) {
                int eq = entry.IndexOf('=');
                if (eq < 0) throw new ArgumentException($"--trace-fn: bad read spec '{entry}'");
                string name = entry[..eq];
                string rest = entry[(eq + 1)..];
                // Register-relative form has TWO semicolons (e.g. "CS;BP;4");
                // fixed-linear form has ONE (e.g. "0x247BE;2").
                string[] restParts = rest.Split(';');
                if (restParts.Length == 3 && IsRegName(restParts[0]) && IsRegName(restParts[1])) {
                    int readBytes = int.Parse(restParts[2]);
                    segReads ??= new();
                    segReads.Add(new FunctionEntryTrace.SegRegRead(name, restParts[0], restParts[1], readBytes));
                } else if (restParts.Length == 2) {
                    uint readLinear = ParseUIntHex(restParts[0]);
                    int readBytes = int.Parse(restParts[1]);
                    reads ??= new();
                    reads.Add((name, readLinear, readBytes));
                } else {
                    throw new ArgumentException($"--trace-fn: read spec '{entry}' must be 'name=LINEAR;BYTES' or 'name=SEG;REG;BYTES'");
                }
            }
        }
        return new FunctionEntryTrace.Entry(linear, label, dsSi, reads, csSi, segReads);
    }

    private static readonly System.Collections.Generic.HashSet<string> _regNames = new(StringComparer.OrdinalIgnoreCase) {
        "CS", "DS", "ES", "SS", "FS", "GS",
        "AX", "BX", "CX", "DX", "SI", "DI", "BP", "SP",
    };
    private static bool IsRegName(string s) => _regNames.Contains(s);

    private static uint ParseUIntHex(string s) {
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
            return uint.Parse(s[2..], System.Globalization.NumberStyles.HexNumber);
        }
        return uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
    }

    private static ushort ParseUShort(string s) {
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
            return ushort.Parse(s[2..], System.Globalization.NumberStyles.HexNumber);
        }
        return ushort.Parse(s);
    }

    private static uint ParseUInt(string s) {
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
            return uint.Parse(s[2..], System.Globalization.NumberStyles.HexNumber);
        }
        return uint.Parse(s);
    }

    private static (uint Addr, byte[] Bytes) ParseMemoryWrite(string s) {
        int eq = s.IndexOf('=');
        if (eq <= 0 || eq == s.Length - 1) {
            throw new ArgumentException($"expected LINEAR=HEXBYTES, got '{s}'");
        }
        uint addr = ParseUInt(s[..eq]);
        string hex = s[(eq + 1)..];
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
            hex = hex[2..];
        }
        if (hex.Length % 2 != 0) {
            throw new ArgumentException($"hex byte string must have even length: '{hex}'");
        }
        byte[] bytes = Convert.FromHexString(hex);
        return (addr, bytes);
    }

    /// <summary>Parses either a single LINEAR address or a "LINEAR..LINEAR" inclusive
    /// range and yields each address. Lets the caller use one breakpoint per byte.</summary>
    private static IEnumerable<uint> ParseLinearRangeOrSingle(string s) {
        int dot = s.IndexOf("..");
        if (dot < 0) {
            yield return ParseUInt(s);
            yield break;
        }
        uint a = ParseUInt(s[..dot]);
        uint b = ParseUInt(s[(dot + 2)..]);
        for (uint x = a; x <= b; x++) yield return x;
    }

    private static (uint Start, uint End) ParseLinearRange(string s) {
        string[] parts = s.Split('-');
        if (parts.Length != 2) {
            throw new ArgumentException($"expected START-END, got '{s}'");
        }
        return (ParseUInt(parts[0]), ParseUInt(parts[1]));
    }

    private static (string Name, uint Value) ParseRegisterAssignment(string s) {
        int eq = s.IndexOf('=');
        if (eq <= 0 || eq == s.Length - 1) {
            throw new ArgumentException($"expected REG=VALUE, got '{s}'");
        }
        return (s[..eq].Trim().ToUpperInvariant(), ParseUInt(s[(eq + 1)..].Trim()));
    }

    private static void PrintHelp() {
        Console.WriteLine(
            "Cryogenic.Harness — drive DNCDPRG.EXE in Spice86 from C# and capture traces.\n\n" +
            "Usage:\n" +
            "  Cryogenic.Harness [HARNESS-FLAGS] [SPICE86-FLAGS]\n\n" +
            "Harness flags:\n" +
            "  --mode {snapshot|invoke} Harness execution mode. Default: snapshot.\n" +
            "  --checkpoint SEG:OFF     Trampoline address. Default 1000:000C (post-driver-load).\n" +
            "  --snapshot-out DIR       Where to write ram.bin + state.json + meta.json.\n" +
            "  --no-exit                Don't halt after the snapshot — keep emulating.\n" +
            "Invoke-mode flags:\n" +
            "  --target SEG:OFF         Function entry point.\n" +
            "  --sentinel SEG:OFF       Sentinel return address. Default FFFE:0000.\n" +
            "  --call-convention near|far  Default: near.\n" +
            "  --arg-stack VALUE        Push a 16-bit word arg onto the stack (repeatable).\n" +
            "  --arg-reg REG=VALUE      Pre-set a register (e.g. --arg-reg AX=0x1F7E).\n" +
            "  --invoke-budget N        Hard instruction-count cap. Default 5,000,000.\n" +
            "  --return-dump DIR        Dump RAM/state when sentinel fires.\n" +
            "  --dump-before-invoke     Dump RAM/state before the redirect (for memdiff).\n" +
            "  --skip-intro             Skip the boot intro (HNMs + IRULAN + palace interlude +\n" +
            "                           MTG1/MTG2) by injecting Esc scancode (1) into ds:0xCEE8\n" +
            "                           each time the engine's Esc consumer at cs1:0xDE54 reads.\n" +
            "                           Stops when scene_id (ds:0x47BE) reaches the porch (0).\n" +
            "                           Cuts boot-to-porch wall time from ~90 s to ~30 s.\n" +
            "  --skip-intro-until N     Same as --skip-intro but stop at scene_id = N. Decimal\n" +
            "                           or 0xHEX. Default with bare --skip-intro is 0.\n" +
            "  --trace-fn SEG:OFF[:LABEL[:BYTES][@reads]]\n" +
            "                           Sparse function-entry trace. BYTES is bare (DS:SI),\n" +
            "                           or 'ds:N' / 'cs:N' / 'ds:N+cs:N' for both segments.\n" +
            "                           '@reads' adds named memory excerpts: comma-separated\n" +
            "                           'name=LINEAR;BYTES' (fixed) or 'name=SEG:REG;BYTES'\n" +
            "                           (register-relative, e.g. CS:BP). Repeatable.\n" +
            "  --trace-instr            Per-instruction ndjson trace inside --instr-range.\n" +
            "  --instr-range START-END  Linear-address window for the instruction trace.\n" +
            "  --write-mem LINEAR=HEX   Write hex bytes to memory before the redirect (repeatable).\n" +
            "                           e.g. --write-mem 0x23B00=4001000A (a 4-byte event record).\n" +
            "  --watch-mem LINEAR       Log every memory write to LINEAR (or LINEAR..LINEAR\n" +
            "                           inclusive range) as ndjson. Use to find which routine\n" +
            "                           patches a global at runtime. Repeatable.\n" +
            "  -h, --help               Show this help.\n\n" +
            "Common Spice86 flags you'll want:\n" +
            "  -e PATH/DNCDPRG.EXE      Executable to run.\n" +
            "  -c PATH                  Mounted C: drive.\n" +
            "  --VerboseLogs=true       Verbose Spice86 logging.\n");
    }
}
