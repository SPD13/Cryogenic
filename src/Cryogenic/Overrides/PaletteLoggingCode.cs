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
        DoOnTopOfInstruction(cs2, 0xB68, LogPaletteLoadEntry);
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
            sb.Append(",\"hnmId\":").Append(hnmId);
            sb.Append(",\"hnmIdHex\":\"0x").Append(hnmId.ToString("X2")).Append('"');
            sb.Append('}');
            AppendLine(sb.ToString());
        } catch (Exception e) {
            _loggerService.Warning("PaletteLogging/hnm: {@Error}", e.Message);
        }
    }
}
