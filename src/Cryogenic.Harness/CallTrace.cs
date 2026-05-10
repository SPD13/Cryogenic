namespace Cryogenic.Harness;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Records function-entry events during the invoke window. Two sources, layered:
/// <list type="number">
///   <item>An address-set built from every FunctionInformation registered by the
///   override supplier — gives us a deterministic trace of which named/known
///   functions were entered. Cheap (one breakpoint per known address).</item>
///   <item>(Best-effort) Spice86's <c>ExecutionFlowRecorder</c>, accessed
///   through reflection. When available, gives us calls, jumps, and returns to
///   addresses that aren't in the registered set.</item>
/// </list>
/// </summary>
public sealed class CallTrace {
    private readonly List<CallEvent> _events = new();
    private readonly object _eventsLock = new();
    private bool _recording;
    private bool _flowRecorderEnabledByUs;
    private object? _flowRecorder; // Spice86.Core.Emulator.Function.ExecutionFlowRecorder

    public IReadOnlyList<CallEvent> Events {
        get { lock (_eventsLock) { return _events.ToArray(); } }
    }

    public void Start(Machine machine) {
        _recording = true;
        TryEnableFlowRecorder(machine);
    }

    public void Stop() {
        _recording = false;
        TryDisableFlowRecorder();
    }

    public void OnFunctionEntry(SegmentedAddress addr, State state, string? label) {
        if (!_recording) {
            return;
        }
        lock (_eventsLock) {
            _events.Add(new CallEvent {
                Cycles = state.Cycles,
                CalleeSegment = addr.Segment,
                CalleeOffset = addr.Offset,
                CalleeLabel = label,
                CallerCS = state.CS,
                CallerIP = state.IP,
                AX = state.AX, BX = state.BX, CX = state.CX, DX = state.DX,
                SI = state.SI, DI = state.DI, BP = state.BP, SP = state.SP,
                DS = state.DS, ES = state.ES,
            });
        }
    }

    public void Save(string outDir) {
        Directory.CreateDirectory(outDir);
        CallTraceDump dump = new() {
            EventCount = _events.Count,
            Events = _events,
            FlowRecorderUsed = _flowRecorder != null,
            FlowRecorderCalls = ReadFlowRecorderCalls(),
        };
        File.WriteAllText(
            Path.Combine(outDir, "calls.json"),
            JsonSerializer.Serialize(dump, JsonOpts));
    }

    private void TryEnableFlowRecorder(Machine machine) {
        try {
            // Walk Cpu → its FunctionHandler field → its ExecutionFlowRecorder field. Spice86
            // doesn't expose these publicly, so we use reflection. If the layout changes in a
            // future Spice86 version, this just degrades to "no flow recorder" — the address-set
            // hook layer still works.
            Cpu cpu = machine.Cpu;
            object? handler = GetFieldOrProperty(cpu, "FunctionHandler") ?? GetFieldOrProperty(cpu, "_functionHandler");
            if (handler is null) {
                Console.Error.WriteLine("[harness] flow recorder: cannot find FunctionHandler on Cpu");
                return;
            }
            object? rec = GetFieldOrProperty(handler, "ExecutionFlowRecorder")
                       ?? GetFieldOrProperty(handler, "_executionFlowRecorder");
            if (rec is null) {
                Console.Error.WriteLine("[harness] flow recorder: cannot find ExecutionFlowRecorder on FunctionHandler");
                return;
            }

            PropertyInfo? recordData = rec.GetType().GetProperty("RecordData");
            if (recordData?.GetValue(rec) is bool wasOn && !wasOn) {
                recordData.SetValue(rec, true);
                _flowRecorderEnabledByUs = true;
            }
            _flowRecorder = rec;
            Console.Error.WriteLine("[harness] flow recorder armed");
        } catch (Exception e) {
            Console.Error.WriteLine($"[harness] flow recorder unavailable: {e.Message}");
            _flowRecorder = null;
        }
    }

    private void TryDisableFlowRecorder() {
        if (_flowRecorder is null || !_flowRecorderEnabledByUs) {
            return;
        }
        try {
            _flowRecorder.GetType().GetProperty("RecordData")?.SetValue(_flowRecorder, false);
        } catch {
            // best-effort
        }
    }

    private List<FlowRecorderCall>? ReadFlowRecorderCalls() {
        if (_flowRecorder is null) {
            return null;
        }
        // ExecutionFlowRecorder stores calls as Dictionary<uint, HashSet<SegmentedAddress>>
        // keyed by the caller's linear address. The actual field name has shifted across
        // Spice86 versions, so try a few known candidates.
        foreach (string fieldName in new[] { "CallsFromTo", "_callsFromTo", "callsFromTo" }) {
            object? raw = GetFieldOrProperty(_flowRecorder, fieldName);
            if (raw is null) {
                continue;
            }
            try {
                List<FlowRecorderCall> result = new();
                System.Collections.IDictionary dict = (System.Collections.IDictionary)raw;
                foreach (System.Collections.DictionaryEntry entry in dict) {
                    uint fromLinear = Convert.ToUInt32(entry.Key);
                    if (entry.Value is not System.Collections.IEnumerable targets) {
                        continue;
                    }
                    foreach (object t in targets) {
                        SegmentedAddress to = (SegmentedAddress)t;
                        result.Add(new FlowRecorderCall {
                            FromLinear = fromLinear,
                            ToSegment = to.Segment,
                            ToOffset = to.Offset,
                        });
                    }
                }
                return result;
            } catch (Exception e) {
                Console.Error.WriteLine($"[harness] flow recorder: dump failed ({e.Message})");
                return null;
            }
        }
        return null;
    }

    private static object? GetFieldOrProperty(object target, string name) {
        Type t = target.GetType();
        const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo? field = t.GetField(name, F);
        if (field != null) {
            return field.GetValue(target);
        }
        PropertyInfo? prop = t.GetProperty(name, F);
        return prop?.GetValue(target);
    }

    private static readonly JsonSerializerOptions JsonOpts = new() {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}

public sealed class CallEvent {
    public long Cycles { get; set; }
    public ushort CalleeSegment { get; set; }
    public ushort CalleeOffset { get; set; }
    public string? CalleeLabel { get; set; }
    public ushort CallerCS { get; set; }
    public ushort CallerIP { get; set; }
    public ushort AX { get; set; }
    public ushort BX { get; set; }
    public ushort CX { get; set; }
    public ushort DX { get; set; }
    public ushort SI { get; set; }
    public ushort DI { get; set; }
    public ushort BP { get; set; }
    public ushort SP { get; set; }
    public ushort DS { get; set; }
    public ushort ES { get; set; }
}

public sealed class FlowRecorderCall {
    public uint FromLinear { get; set; }
    public ushort ToSegment { get; set; }
    public ushort ToOffset { get; set; }
}

public sealed class CallTraceDump {
    public int EventCount { get; set; }
    public List<CallEvent> Events { get; set; } = new();
    public bool FlowRecorderUsed { get; set; }
    public List<FlowRecorderCall>? FlowRecorderCalls { get; set; }
}
