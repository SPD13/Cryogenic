namespace Cryogenic.Harness;

using System.Text.Json;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Sets up a synthetic call: pushes a sentinel return, optional word args, and any
/// register pre-sets, then redirects CS:IP to the target so the emulator runs *into*
/// the function. When the sentinel is hit, captures and writes the return state.
/// </summary>
/// <remarks>
/// Phase 2: minimal viable invoker. The trampoline assumes:
///   * The CPU is in real mode.
///   * SS:SP points at the same stack the game uses (we do NOT install a fresh stack).
///   * The target uses one of <see cref="CallConvention"/> for return.
///
/// Not yet handled: register-saving conventions (most of Cryo's compiled code clobbers
/// caller-save without ceremony, so this is fine), variable-length arg lists with
/// register-passed first arg, FAR pointers as args (callers can pass seg/off as two
/// pushed words for now).
/// </remarks>
public sealed class DirectInvoker {
    private readonly HarnessOptions _options;

    public DirectInvoker(HarnessOptions options) {
        _options = options;
    }

    /// <summary>Step 1: at trampoline time — push sentinel + args, set registers, redirect CS:IP.</summary>
    public InvokeSnapshot Setup(IMemory memory, State state, Stack stack) {
        InvokeSnapshot pre = InvokeSnapshot.Capture(state, "pre-invoke");

        // Push word args. Cdecl pushes RtoL, but to keep config order intuitive
        // we push the list as-is: list[0] is pushed first ⇒ ends up at higher SP
        // ⇒ becomes the LAST positional arg in C-style notation. Callers that
        // need C order should reverse the list before handing it in.
        foreach (ushort word in _options.InvokeStackArgs) {
            stack.Push16(word);
        }

        // Push the sentinel return. NearRet only restores IP (CS stays the same),
        // so the segment we put on the stack is irrelevant — only the offset is.
        if (_options.InvokeConvention == CallConvention.FarCdecl) {
            stack.PushSegmentedAddress(_options.InvokeSentinel);
        } else {
            stack.Push16(_options.InvokeSentinel.Offset);
        }

        // Apply register pre-sets.
        foreach (KeyValuePair<string, uint> kv in _options.InvokeRegisters) {
            ApplyRegister(state, kv.Key, kv.Value);
        }

        // Redirect to the target.
        SegmentedAddress target = _options.InvokeTarget;
        state.CS = target.Segment;
        state.IP = target.Offset;

        return pre;
    }

    /// <summary>Step 2: at sentinel time — capture and persist the return state.</summary>
    public InvokeReturn CaptureReturn(IMemory memory, State state, InvokeSnapshot pre) {
        InvokeSnapshot post = InvokeSnapshot.Capture(state, "post-invoke");

        InvokeReturn ret = new() {
            Pre = pre,
            Post = post,
            BudgetCyclesUsed = post.Cycles - pre.Cycles,
            Target = _options.InvokeTarget,
            Sentinel = _options.InvokeSentinel,
            Convention = _options.InvokeConvention.ToString(),
            RegisterPresets = new Dictionary<string, uint>(_options.InvokeRegisters),
            StackArgs = new List<ushort>(_options.InvokeStackArgs),
        };

        string outDir = _options.InvokeReturnDumpDir ?? _options.OutputPath;
        Directory.CreateDirectory(outDir);
        File.WriteAllText(
            Path.Combine(outDir, "invoke-return.json"),
            JsonSerializer.Serialize(ret, JsonOpts));
        return ret;
    }

    private static void ApplyRegister(State s, string name, uint value) {
        switch (name.ToUpperInvariant()) {
            case "AX": s.AX = (ushort)value; break;
            case "AL": s.AL = (byte)value; break;
            case "AH": s.AH = (byte)value; break;
            case "EAX": s.EAX = value; break;
            case "BX": s.BX = (ushort)value; break;
            case "BL": s.BL = (byte)value; break;
            case "BH": s.BH = (byte)value; break;
            case "EBX": s.EBX = value; break;
            case "CX": s.CX = (ushort)value; break;
            case "CL": s.CL = (byte)value; break;
            case "CH": s.CH = (byte)value; break;
            case "ECX": s.ECX = value; break;
            case "DX": s.DX = (ushort)value; break;
            case "DL": s.DL = (byte)value; break;
            case "DH": s.DH = (byte)value; break;
            case "EDX": s.EDX = value; break;
            case "SI": s.SI = (ushort)value; break;
            case "DI": s.DI = (ushort)value; break;
            case "BP": s.BP = (ushort)value; break;
            case "DS": s.DS = (ushort)value; break;
            case "ES": s.ES = (ushort)value; break;
            default:
                throw new ArgumentException($"unknown register name '{name}'");
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
}

public sealed class InvokeSnapshot {
    public string Phase { get; set; } = string.Empty;
    public ushort CS { get; set; }
    public ushort IP { get; set; }
    public uint EAX { get; set; }
    public uint EBX { get; set; }
    public uint ECX { get; set; }
    public uint EDX { get; set; }
    public uint ESI { get; set; }
    public uint EDI { get; set; }
    public uint EBP { get; set; }
    public uint ESP { get; set; }
    public ushort DS { get; set; }
    public ushort ES { get; set; }
    public ushort SS { get; set; }
    public ushort Flags { get; set; }
    public long Cycles { get; set; }

    public static InvokeSnapshot Capture(State s, string phase) => new() {
        Phase = phase,
        CS = s.CS, IP = s.IP,
        EAX = s.EAX, EBX = s.EBX, ECX = s.ECX, EDX = s.EDX,
        ESI = s.ESI, EDI = s.EDI, EBP = s.EBP, ESP = s.ESP,
        DS = s.DS, ES = s.ES, SS = s.SS,
        Flags = (ushort)s.Flags.FlagRegister,
        Cycles = s.Cycles,
    };
}

public sealed class InvokeReturn {
    public InvokeSnapshot Pre { get; set; } = new();
    public InvokeSnapshot Post { get; set; } = new();
    public long BudgetCyclesUsed { get; set; }
    public SegmentedAddress Target { get; set; }
    public SegmentedAddress Sentinel { get; set; }
    public string Convention { get; set; } = string.Empty;
    public Dictionary<string, uint> RegisterPresets { get; set; } = new();
    public List<ushort> StackArgs { get; set; } = new();
}
