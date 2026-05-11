namespace Cryogenic.Overrides;

using Spice86.Shared.Utils;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Runtime hooks that log every palette load and HNM cinematic load to a
/// JSONL file. Used to extract HNM ↔ palette pairings empirically — a single
/// play-through that triggers each cinematic produces a complete pairing
/// table that the asset extractor's post-processor consumes.
/// </summary>
/// <remarks>
/// <para>
/// Background: Cryo Dune's HNM(1) format allows cinematics to ship without
/// their own palette, instead reusing the engine's currently-loaded palette
/// state. Static reverse engineering of the script VM (DOCUMENTATION/Tech/16
/// through Tech/23) showed that the palette is set entirely from cs2 (the
/// VGA driver code) as a side effect of resource loads — there is NO direct
/// (HNM, palette) pairing table in the engine binary. The pairing only
/// exists in the temporal order of resource loads at runtime.
/// </para>
/// <para>
/// This hook captures that runtime order. It writes one JSON line per
/// palette/HNM event to <c>cryogenic-palette-trace.jsonl</c> in the current
/// working directory. The asset extractor reconciles: for each
/// <c>hnm_load</c> event, the most-recent prior palette load is the paired
/// palette. The output is stable across runs that follow the same play
/// path.
/// </para>
/// </remarks>
public partial class Overrides {
    private bool _palLogInitialized;
    private string? _palLogPath;
    private readonly Dictionary<string, byte[]> _seenPalettes = new();
    private long _palLogEventCounter;

    /// <summary>
    /// Registers <c>DoOnTopOfInstruction</c> hooks at the entry of
    /// <c>LoadPaletteInVgaDac</c> (cs2:0xB68) and <c>hnm_load_ida</c>
    /// (cs1:0xCA1B). Each hook appends a JSON line to the trace file
    /// describing the call's arguments.
    /// </summary>
    public void DefinePaletteLoggingOverrides() {
        // Only cs1:0xCA1B (hnm_load) is logged via DoOnTopOfInstruction because
        // its DefineFunction override is gated off in non-harness mode. The
        // other targets (palette, blit, script, scene_boundary, dialogue) all
        // have C# DefineFunction overrides registered, which causes Spice86
        // to dispatch to the C# method and skip any DoOnTopOfInstruction at
        // the same address. For those, the log call is made from inside the
        // override implementation (see VgaDriverCode, ScriptedSceneCode,
        // UnknownCode, DialoguesCode).
        DoOnTopOfInstruction(cs1, 0xCA1B, LogHnmLoadEntry);
    }

    private void EnsurePaletteLogInitialized() {
        if (_palLogInitialized) return;
        _palLogInitialized = true;
        _palLogPath = Path.Combine(Environment.CurrentDirectory, "cryogenic-palette-trace.jsonl");
        try {
            File.WriteAllText(_palLogPath, "");
            File.AppendAllText(_palLogPath,
                "{\"t\":\"meta\",\"format\":\"cryogenic-palette-trace/v1\",\"started\":\""
                + DateTime.UtcNow.ToString("o") + "\"}\n");
            _loggerService.Information("PaletteLogging: writing trace to {@Path}", _palLogPath);
        } catch (Exception e) {
            _loggerService.Warning("PaletteLogging: failed to initialize {@Path}: {@Error}",
                _palLogPath, e.Message);
        }
    }

    private void AppendLine(string line) {
        if (_palLogPath is null) return;
        try {
            File.AppendAllText(_palLogPath, line + "\n");
        } catch {
            // Trace writes are best-effort; never crash gameplay because of an I/O error.
        }
    }

    /// <summary>
    /// Hook that fires at the start of <c>LoadPaletteInVgaDac</c> (cs2:0xB68).
    /// Reads the palette bytes from <c>(ES:DX)</c>, computes a content hash,
    /// and emits a JSON event with the start index, color count, and a
    /// SHA-256 of the palette bytes (de-duplicating identical palette loads).
    /// </summary>
    private void LogPaletteLoadEntry() {
        EnsurePaletteLogInitialized();
        try {
            uint baseAddr = MemoryUtils.ToPhysicalAddress(ES, DX);
            byte writeIndex = BL;
            ushort numberOfColors = CX;
            byte loadMode = globalsOnCsSegment0X2538.Get2538_01BD_Byte8_PaletteLoadMode();

            // Read the palette bytes (numberOfColors * 3 bytes of 6-bit RGB).
            int byteCount = numberOfColors * 3;
            byte[] palette = new byte[byteCount];
            for (int i = 0; i < byteCount; i++) {
                palette[i] = UInt8[(uint)(baseAddr + i)];
            }

            // SHA-256 dedupe.
            string sha;
            using (SHA256 sha256 = SHA256.Create()) {
                byte[] hash = sha256.ComputeHash(palette);
                sha = ConvertUtils.ByteArrayToHexString(hash);
            }
            bool firstSeen = !_seenPalettes.ContainsKey(sha);
            if (firstSeen) {
                _seenPalettes[sha] = palette;
            }

            // Emit the event. Include base64 palette bytes only on first sighting
            // to keep the trace compact (re-loads of the same palette dedupe).
            StringBuilder sb = new();
            sb.Append("{\"t\":\"palette\",\"seq\":").Append(_palLogEventCounter++);
            sb.Append(",\"baseAddr\":\"0x").Append(baseAddr.ToString("X")).Append('"');
            sb.Append(",\"writeIndex\":").Append(writeIndex);
            sb.Append(",\"numberOfColors\":").Append(numberOfColors);
            sb.Append(",\"loadMode\":").Append(loadMode);
            sb.Append(",\"sha\":\"").Append(sha).Append('"');
            if (firstSeen) {
                sb.Append(",\"bytes\":\"").Append(Convert.ToBase64String(palette)).Append('"');
            }
            sb.Append('}');
            AppendLine(sb.ToString());
        } catch (Exception e) {
            _loggerService.Warning("PaletteLogging/palette: {@Error}", e.Message);
        }
    }

    /// <summary>
    /// Hook that fires at the start of <c>hnm_load_ida</c> (cs1:0xCA1B).
    /// Captures the AX register (= the engine's internal HNM video ID, NOT
    /// a TOC slot) and emits a JSON event. The reconciler must resolve
    /// hnmId → TOC slot via the engine's name table (cs1:0x33A3).
    /// </summary>
    private void LogHnmLoadEntry() {
        EnsurePaletteLogInitialized();
        try {
            ushort hnmId = AX;
            StringBuilder sb = new();
            sb.Append("{\"t\":\"hnm\",\"seq\":").Append(_palLogEventCounter++);
            sb.Append(",\"cycles\":").Append(State.Cycles);
            sb.Append(",\"hnmId\":").Append(hnmId);
            sb.Append(",\"hnmIdHex\":\"0x").Append(hnmId.ToString("X2")).Append('"');
            sb.Append('}');
            AppendLine(sb.ToString());
        } catch (Exception e) {
            _loggerService.Warning("PaletteLogging/hnm: {@Error}", e.Message);
        }
    }

    /// <summary>
    /// Hook at <c>CopySquareOfPixels</c> (cs2:0x1B8E) — the main sprite
    /// blit dispatched by VgaFunc14/16 and direct callers. Captures src
    /// segment, dest segment, X/Y, columns, rows. This is fine-grained
    /// (every sprite paint), so the trace can get large.
    /// </summary>
    private void LogBlitEntry() {
        EnsurePaletteLogInitialized();
        try {
            ushort srcSeg = DS;
            ushort dstSeg = ES;
            ushort x = DX;
            ushort y = BX;
            ushort cols = AX;
            ushort rows = BP;
            StringBuilder sb = new();
            sb.Append("{\"t\":\"blit\",\"seq\":").Append(_palLogEventCounter++);
            sb.Append(",\"cycles\":").Append(State.Cycles);
            sb.Append(",\"srcSeg\":\"0x").Append(srcSeg.ToString("X4")).Append('"');
            sb.Append(",\"dstSeg\":\"0x").Append(dstSeg.ToString("X4")).Append('"');
            sb.Append(",\"x\":").Append(x);
            sb.Append(",\"y\":").Append(y);
            sb.Append(",\"cols\":").Append(cols);
            sb.Append(",\"rows\":").Append(rows);
            sb.Append('}');
            AppendLine(sb.ToString());
        } catch (Exception e) {
            _loggerService.Warning("PaletteLogging/blit: {@Error}", e.Message);
        }
    }

    /// <summary>
    /// Hook at <c>LoadSceneSequenceDataIntoAXAndAdvanceSI</c> (cs1:0x93F) —
    /// the scene-script VM's per-step instruction read. The script bytecode
    /// lives in the code segment (CS); the byte-offset of the next opcode
    /// is held in <c>ds[0x4854]</c>. Logs offset + the 8 bytes ahead so the
    /// reconciler can decode which scene-script verb is executing.
    /// </summary>
    private void LogSceneScriptStepEntry() {
        EnsurePaletteLogInitialized();
        try {
            ushort offset = globalsOnDs.Get1138_4854_Word16_SceneSequenceOffset();
            ushort sceneId = (ushort)UInt8[DS, 0x2A];
            uint streamLinear = MemoryUtils.ToPhysicalAddress(CS, offset);
            byte[] head = new byte[8];
            for (int i = 0; i < 8; i++) head[i] = UInt8[(uint)(streamLinear + i)];
            StringBuilder sb = new();
            sb.Append("{\"t\":\"script\",\"seq\":").Append(_palLogEventCounter++);
            sb.Append(",\"cycles\":").Append(State.Cycles);
            sb.Append(",\"sceneId\":").Append(sceneId);
            sb.Append(",\"cs\":\"0x").Append(CS.ToString("X4")).Append('"');
            sb.Append(",\"offset\":\"0x").Append(offset.ToString("X4")).Append('"');
            sb.Append(",\"head\":\"").Append(ConvertUtils.ByteArrayToHexString(head)).Append('"');
            sb.Append('}');
            AppendLine(sb.ToString());
        } catch (Exception e) {
            _loggerService.Warning("PaletteLogging/script: {@Error}", e.Message);
        }
    }

    /// <summary>
    /// Framebuffer-copy event for <c>MemcpyDSToESFor64000</c> (cs2:0x1B7C) —
    /// 64000-byte full-screen copies are how HNM frames and scene
    /// backgrounds reach VRAM, so each event is a "new screen" marker.
    /// Also auto-captures the sprite cache + compositing buffer at the
    /// onset of each post-MTG1 palace-interlude phase (see Tech/32).
    /// </summary>
    private void LogFramebufferCopyEntry() {
        EnsurePaletteLogInitialized();
        try {
            StringBuilder sb = new();
            sb.Append("{\"t\":\"fbcopy\",\"seq\":").Append(_palLogEventCounter++);
            sb.Append(",\"cycles\":").Append(State.Cycles);
            sb.Append(",\"srcSeg\":\"0x").Append(DS.ToString("X4")).Append('"');
            sb.Append(",\"dstSeg\":\"0x").Append(ES.ToString("X4")).Append('"');
            sb.Append('}');
            AppendLine(sb.ToString());

            MaybeCapturePalaceScene();
        } catch (Exception e) {
            _loggerService.Warning("PaletteLogging/fbcopy: {@Error}", e.Message);
        }
    }

    // Capture every FBCOPY-to-VGA whose previous one was >5M cycles ago
    // (= a new "scene" boundary). Empirically the post-MTG1 palace scenes
    // come in three quick room-establishment shots ~10Mc apart, then longer
    // dialogue scenes, then a quick "palace view" right before MTG2.
    // A 30M-cycle threshold (earlier attempt) lumped the three quick shots
    // into one cluster; 5M cycles correctly separates them while still
    // collapsing the multiple repaints inside a single dialogue scene.
    private const long SceneCaptureCycleGap = 5_000_000L;
    private const long SceneCaptureWindowStart = 250_000_000L;
    private const long SceneCaptureWindowEnd = 1_500_000_000L;
    private const int SceneCaptureMax = 16;
    private long _lastPresentCycles = -1;
    private int _sceneCaptureCount = 0;

    /// <summary>
    /// Dumps the sprite-cache segment (0x335B) and compositing back-buffer
    /// segment (0x42FB) to disk on the first FBCOPY-to-VGA of every new
    /// "scene" — defined as a present that comes more than 30M cycles
    /// after the previous one. Captures within a wide cycles window
    /// covering Irulan → MTG2 onset. Capped at 16 captures total.
    /// </summary>
    private void MaybeCapturePalaceScene() {
        if (ES != 0xA000) return;
        long cycles = (long)State.Cycles;
        if (cycles < SceneCaptureWindowStart) return;
        if (cycles > SceneCaptureWindowEnd) return;

        bool isNewScene = _lastPresentCycles < 0
            || (cycles - _lastPresentCycles) > SceneCaptureCycleGap;
        _lastPresentCycles = cycles;
        if (!isNewScene) return;
        if (_sceneCaptureCount >= SceneCaptureMax) return;

        _sceneCaptureCount++;
        int idx = _sceneCaptureCount;
        CaptureSegmentToFile(0x335B, $"palace_scene_{idx:D2}_335B_cycles{cycles}.bin");
        CaptureSegmentToFile(0x42FB, $"palace_scene_{idx:D2}_42FB_cycles{cycles}.bin");
        _loggerService.Information(
            "PaletteLogging: captured palace scene #{@Idx} at cycles {@Cycles}",
            idx, cycles);
    }

    private void CaptureSegmentToFile(ushort segment, string filename) {
        try {
            uint baseLinear = (uint)segment << 4;
            byte[] data = new byte[65536];
            for (uint i = 0; i < 65536; i++) {
                data[i] = UInt8[baseLinear + i];
            }
            string path = Path.Combine(Environment.CurrentDirectory, filename);
            File.WriteAllBytes(path, data);
        } catch (Exception e) {
            _loggerService.Warning("PaletteLogging/capture: {@Error}", e.Message);
        }
    }

    /// <summary>
    /// Hook at <c>Fill47F8WithFF</c> (cs1:0x3AE9) — fires on every scene
    /// enter/leave. Cheap "section marker" to anchor blit/script events
    /// to a scene transition timeline.
    /// </summary>
    private void LogSceneBoundaryEntry() {
        EnsurePaletteLogInitialized();
        try {
            ushort sceneId = (ushort)UInt8[DS, 0x2A];
            StringBuilder sb = new();
            sb.Append("{\"t\":\"scene_boundary\",\"seq\":").Append(_palLogEventCounter++);
            sb.Append(",\"cycles\":").Append(State.Cycles);
            sb.Append(",\"sceneId\":").Append(sceneId);
            sb.Append('}');
            AppendLine(sb.ToString());
        } catch (Exception e) {
            _loggerService.Warning("PaletteLogging/scene_boundary: {@Error}", e.Message);
        }
    }

    /// <summary>
    /// Hook at <c>InitDialogue</c> (cs1:0xC85B) — dialogue tree entry.
    /// Captures the AX register (dialogue entry id) so the reconciler
    /// can match which DIALOGUE.HSQ entry is active.
    /// </summary>
    private void LogDialogueInitEntry() {
        EnsurePaletteLogInitialized();
        try {
            ushort dialogueId = AX;
            StringBuilder sb = new();
            sb.Append("{\"t\":\"dialogue\",\"seq\":").Append(_palLogEventCounter++);
            sb.Append(",\"cycles\":").Append(State.Cycles);
            sb.Append(",\"dialogueId\":").Append(dialogueId);
            sb.Append('}');
            AppendLine(sb.ToString());
        } catch (Exception e) {
            _loggerService.Warning("PaletteLogging/dialogue: {@Error}", e.Message);
        }
    }
}
