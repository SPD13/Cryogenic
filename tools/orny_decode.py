#!/usr/bin/env python3
"""
ORNY.HSQ bytecode decoder — extracts the engine's per-scene scene-script
records (Tech/19 §1) into a structured JSON + markdown report.

Pipeline (engine source: `cs1:0x9F9E` scene-script VM):
  1. ORNY.HSQ is loaded into RAM with its base byte at `ds:0xAA76`.
  2. Header @ +0x0000:  u16 = 0x0002  (first valid scene id)
                +0x0002: u16 = 0x0012  (sentinel marking end of "low" table)
  3. Offset table @ +0x0004 onward: u16[] indexed by scene_id, mapping
     scene_id → in-file offset where that scene's bytecode starts.
     Per the doc, the explicit table only goes up to scene_id 9; entries
     beyond that point to addresses outside the file (probably an
     extension table loaded separately, or unused slots that never get
     dispatched because the engine's scene-id ticker doesn't reach them).
  4. Per-scene bytecode: 4-byte instructions terminated by `0xFFFF` (u16).
     Instruction layout:
       byte 0:
         bit 7      consumed flag (set after first fire — gate via 0x47C2)
         bit 6      mode bit (skips the consumed-mask gate)
         bits 4-5   gate hi-bits (passed via AL & 0x47C2)
         bits 0-3   sub-verb id (1..15; 0 = "no sub-verb, predicate-only")
       byte 1   predicate template id  (indexes GENERIC.HSQ format table)
       byte 2   predicate operand low + selector bits
       byte 3   predicate operand high (xchg'd then `and 0x3FF / or 0x800`
                produces a 10-bit phrase id with bit 11 set)
  5. When the predicate evaluator (`cs1:0xA396`) returns NZ, the sub-verb
     at `cs1:0xA107 + (subverb-1)*2` fires (or none if subverb==0).
     Tech/19 §sub-verb-table-decoded enumerates the 15 active slots.

Run:
  python3 tools/orny_decode.py
Outputs:
  Rebuild/DOCUMENTATION/Tech/49-orny-decoded.md  (human-readable report)
  Rebuild/extracted-assets/state/orny-scenes.json (machine-readable graph)
"""

from __future__ import annotations
import json
import struct
from dataclasses import dataclass, asdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ORNY_PATH = ROOT / "Rebuild" / "extracted-assets" / "raw" / "ORNY.HSQ.bin"
REPORT_PATH = ROOT / "Rebuild" / "DOCUMENTATION" / "Tech" / "49-orny-decoded.md"
JSON_PATH = ROOT / "Rebuild" / "extracted-assets" / "state" / "orny-scenes.json"

# Sub-verb names from Tech/19 §sub-verb-table-decoded.
# Index = sub-verb id as stored in byte0 bits 0-3.
SUBVERB_NAMES = {
     0: "(none / predicate-only fire)",
     1: "set_47A5_FF",
     2: "dispatch_127C (scene-id range topic)",
     3: "set_476D_00",
     4: "set_476D_01",
     5: "inc_47A8 (counter)",
     6: "set_47A5_80",
     7: "group_branch_A125 (per [ds:0x47C4])",
     8: "group_branch_A157 (per [ds:0x47C4])",
     9: "larger_A25B",
    10: "advance_scene (inc [ds:0x2A], call B17A)",
    11: "advance_group_of_4 (round_down_4 + 4)",
    12: "larger_A28E",
    13: "inc_C2 (location-visited flag)",
    14: "group_branch_A172 (per [ds:0x47C4])",
    15: "screen_redraw (gfx_copy_framebuffer)",
}

# Sub-verbs that influence scene-id state — the candidates for player nav.
NAV_RELATED_SUBVERBS = {2, 7, 8, 10, 11, 14}

# Indices that explicitly advance scene_id.
SCENE_ADVANCE_SUBVERBS = {10, 11}


@dataclass
class Instr:
    """One decoded 4-byte ORNY instruction."""
    file_offset: int
    raw: list[int]                # 4 raw bytes
    consumed: bool                # byte0 bit 7
    mode: bool                    # byte0 bit 6
    gate_hi: int                  # byte0 bits 4-5
    subverb: int                  # byte0 bits 0-3
    subverb_name: str
    predicate_id: int             # byte 1
    sel: int                      # byte 2
    extra: int                    # byte 3
    phrase_id: int                # ((byte3 << 8) | byte2) & 0x3FF, then | 0x800

    def to_json(self) -> dict:
        return {
            "off":  self.file_offset,
            "raw":  self.raw,
            "consumed": self.consumed,
            "mode": self.mode,
            "gate_hi": self.gate_hi,
            "subverb":      self.subverb,
            "subverb_name": self.subverb_name,
            "predicate_id": self.predicate_id,
            "sel":   self.sel,
            "extra": self.extra,
            "phrase_id": self.phrase_id,
        }


@dataclass
class Scene:
    scene_id: int
    file_offset: int               # where instructions start
    valid: bool                    # offset is in-file and decodes cleanly
    instr_count: int
    terminated: bool               # hit a 0xFFFF terminator
    subverb_histogram: dict[int, int]
    nav_records: list[dict]        # subset of instructions whose subverb
                                   # is in NAV_RELATED_SUBVERBS
    instrs: list[Instr]


def decode_instruction(buf: bytes, off: int) -> Instr:
    b0, b1, b2, b3 = buf[off], buf[off+1], buf[off+2], buf[off+3]
    subverb = b0 & 0x0F
    consumed = (b0 & 0x80) != 0
    mode = (b0 & 0x40) != 0
    gate_hi = (b0 >> 4) & 0x03
    # Phrase id reconstruction mirrors cs1:0x9FF8..0xA002:
    #   lodsw (low=b2, high=b3) → ax
    #   xchg ah,al
    #   and ax,0x3FF
    #   or  ax,0x800
    # That swap-then-mask yields phrase = ((b3<<8) | b2) byte-swapped then
    # masked. Equivalent: ((b2 << 8) | b3) & 0x3FF, then | 0x800.
    raw_ax = (b3 << 8) | b2
    swapped = ((raw_ax & 0xFF) << 8) | ((raw_ax >> 8) & 0xFF)
    phrase_id = (swapped & 0x3FF) | 0x800
    return Instr(
        file_offset=off,
        raw=[b0, b1, b2, b3],
        consumed=consumed,
        mode=mode,
        gate_hi=gate_hi,
        subverb=subverb,
        subverb_name=SUBVERB_NAMES.get(subverb, f"?subverb_{subverb}"),
        predicate_id=b1,
        sel=b2,
        extra=b3,
        phrase_id=phrase_id,
    )


def find_scene_boundaries(buf: bytes, table_start: int = 0x0004,
                          max_scene_id: int = 64) -> list[tuple[int, int, bool]]:
    """Walk the scene-id → offset table, returning `(scene_id, offset, in_file)`.

    Tech/19 documents the table starting at file offset 0x0004 for scene_id
    starting at 2 (scenes 0/1 are header words; the engine handles them
    specially or never dispatches them in the ORNY VM). The table extends
    until either an offset becomes implausibly large (outside file) OR we
    hit the `0xFFFF` sentinel.
    """
    out = []
    scene_id = 2  # per Tech/19 — scenes 0/1 are header u16s
    p = table_start
    while p + 1 < len(buf) and scene_id <= max_scene_id:
        off = struct.unpack_from('<H', buf, p)[0]
        if off == 0xFFFF:
            break
        in_file = 0 <= off < len(buf)
        out.append((scene_id, off, in_file))
        scene_id += 1
        p += 2
    return out


def decode_scene(buf: bytes, scene_id: int, file_offset: int,
                 stop_at: int | None = None,
                 max_steps: int = 1024) -> Scene:
    """Walk a single scene's 4-byte instructions.

    Stops at the first of: a `0xFFFF` u16 (the documented terminator —
    in practice never present in ORNY; see Tech/49 finding), `stop_at`
    byte offset (an upper bound to avoid overshooting into adjacent
    scenes), `max_steps` instructions, or end of file.
    """
    instrs: list[Instr] = []
    histogram: dict[int, int] = {}
    nav_records: list[dict] = []
    terminated = False
    p = file_offset
    n = 0
    if not (0 <= file_offset < len(buf)):
        return Scene(scene_id=scene_id, file_offset=file_offset, valid=False,
                     instr_count=0, terminated=False, subverb_histogram={},
                     nav_records=[], instrs=[])
    while p + 1 < len(buf) and n < max_steps:
        if stop_at is not None and p >= stop_at:
            break
        w = struct.unpack_from('<H', buf, p)[0]
        if w == 0xFFFF:
            terminated = True
            break
        if p + 4 > len(buf):
            break
        ins = decode_instruction(buf, p)
        instrs.append(ins)
        histogram[ins.subverb] = histogram.get(ins.subverb, 0) + 1
        if ins.subverb in NAV_RELATED_SUBVERBS:
            nav_records.append(ins.to_json())
        p += 4
        n += 1
    return Scene(
        scene_id=scene_id,
        file_offset=file_offset,
        valid=True,
        instr_count=len(instrs),
        terminated=terminated,
        subverb_histogram=histogram,
        nav_records=nav_records,
        instrs=instrs,
    )


def write_json(scenes: list[Scene]) -> None:
    JSON_PATH.parent.mkdir(parents=True, exist_ok=True)
    payload = {
        "source_file": str(ORNY_PATH.relative_to(ROOT)),
        "decoder_doc": "Rebuild/DOCUMENTATION/Tech/19-scene-script-vm.md",
        "subverb_names": SUBVERB_NAMES,
        "scene_advance_subverbs": sorted(SCENE_ADVANCE_SUBVERBS),
        "nav_related_subverbs": sorted(NAV_RELATED_SUBVERBS),
        "scenes": [
            {
                "scene_id": s.scene_id,
                "file_offset": s.file_offset,
                "valid": s.valid,
                "terminated": s.terminated,
                "instr_count": s.instr_count,
                "subverb_histogram": s.subverb_histogram,
                "nav_records": s.nav_records,
                "instrs": [i.to_json() for i in s.instrs],
            }
            for s in scenes
        ],
    }
    JSON_PATH.write_text(json.dumps(payload, indent=2))


def write_markdown(scenes: list[Scene]) -> None:
    lines: list[str] = []
    lines.append("# 49 — ORNY Bytecode (decoded)")
    lines.append("")
    lines.append("> Auto-generated by `tools/orny_decode.py`. Re-run to refresh.")
    lines.append("> Source: `Rebuild/extracted-assets/raw/ORNY.HSQ.bin`")
    lines.append(">")
    lines.append("> Format: Tech/19 §1 — 4-byte instructions, per-scene")
    lines.append("> offsets at file +0x0004 (scene_id 2..N).")
    lines.append("")
    lines.append("## ★ Headline finding — ORNY is cinematic, NOT player-room nav")
    lines.append("")
    lines.append("ORNY's scene-id offset table starts at **scene_id 2**.")
    lines.append("Scene_ids 0 and 1 are header words (0x0002, 0x0012), not")
    lines.append("script offsets. Tech/19's caller analysis shows the engine")
    lines.append("steps `[ds:0x2A]` (the script-VM scene index) through ids")
    lines.append("2..9 during the boot intro / cinematic interludes. Sub-verb")
    lines.append("10 (`advance_scene`) only increments `[ds:0x2A]` linearly —")
    lines.append("no per-direction branching.")
    lines.append("")
    lines.append("The porch (logical scene_id `[ds:0x47BE]=0`) has NO ORNY")
    lines.append("entry. Player-room navigation is **not** driven by this VM.")
    lines.append("That confirms the static-analysis result in")
    lines.append("`Rebuild/game/navigationGraph.js`: the click-on-arrow path")
    lines.append("inside the palace is handled by another (still-unmapped)")
    lines.append("dispatcher, not the ORNY scene-script VM.")
    lines.append("")
    lines.append("## ⚠ Finding — no FFFF terminators in ORNY")
    lines.append("")
    lines.append("Tech/19 §1 describes the format as `4-byte fixed records,")
    lines.append("terminated by 0xFFFF (u16)`. A byte-scan of ORNY.HSQ.bin shows")
    lines.append("**zero `0xFF` bytes anywhere in the file**. So either:")
    lines.append("")
    lines.append("- the engine's runtime terminator is set at load time (e.g. by")
    lines.append("  the loader writing FFFF at the end of the buffer), or")
    lines.append("- scenes terminate via the SI=0xFFFF self-poison path at")
    lines.append("  `cs1:0xA0A7` (`mov si, 0xFFFF`) which fires when `inc_47A8`")
    lines.append("  sub-verb 5's counter underflows, or")
    lines.append("- scenes simply run until end of file / next scene's offset.")
    lines.append("")
    lines.append("The decoder bounds each scene by either the next strictly-")
    lines.append("greater offset in the table or end of file. Scenes 4..9")
    lines.append("legitimately *share* a common tail block — their offsets are")
    lines.append("only 6 bytes apart (0x0C33, 0x0C39, …, 0x0C51), so they're")
    lines.append("essentially different entry points into one cinematic script.")
    lines.append("")
    # Global summary
    total_instrs = sum(s.instr_count for s in scenes)
    valid_scenes = [s for s in scenes if s.valid]
    terminated = [s for s in valid_scenes if s.terminated]
    lines.append("## Summary")
    lines.append("")
    lines.append(f"- Scenes in table: **{len(scenes)}** (ids {scenes[0].scene_id if scenes else '-'}..{scenes[-1].scene_id if scenes else '-'})")
    lines.append(f"- Valid (offset in file): **{len(valid_scenes)}**")
    lines.append(f"- FFFF-terminated cleanly: **{len(terminated)}**")
    lines.append(f"- Total decoded instructions: **{total_instrs}**")
    lines.append("")
    # Histogram across all scenes
    global_hist: dict[int, int] = {}
    for s in valid_scenes:
        for k, v in s.subverb_histogram.items():
            global_hist[k] = global_hist.get(k, 0) + v
    lines.append("### Global sub-verb histogram")
    lines.append("")
    lines.append("| sv | count | name |")
    lines.append("|---:|------:|------|")
    for sv in sorted(global_hist.keys()):
        lines.append(f"| {sv} | {global_hist[sv]} | {SUBVERB_NAMES.get(sv, '?')} |")
    lines.append("")
    # Scene-advance map
    lines.append("## Scene-advance candidates (sub-verbs 10 / 11)")
    lines.append("")
    lines.append("Sub-verb 10 = `inc [ds:0x2A]` (scene_id += 1). Sub-verb 11 =")
    lines.append("`scene_id = round_down_4(scene_id) + 4`. These are the ONLY")
    lines.append("paths that mutate the engine's scene-id register from a")
    lines.append("scene script. The targets are *deterministic linear* — not")
    lines.append("an arbitrary nav graph.")
    lines.append("")
    lines.append("| scene_id | sv10 count | sv11 count | implied targets |")
    lines.append("|---------:|-----------:|-----------:|-----------------|")
    for s in valid_scenes:
        sv10 = s.subverb_histogram.get(10, 0)
        sv11 = s.subverb_histogram.get(11, 0)
        if sv10 == 0 and sv11 == 0:
            continue
        targets = []
        if sv10 > 0:
            targets.append(f"scene_id+1 = {s.scene_id + 1}")
        if sv11 > 0:
            base = (s.scene_id & ~0x3) + 4
            targets.append(f"round_down_4+4 = {base}")
        lines.append(f"| {s.scene_id} | {sv10} | {sv11} | {' · '.join(targets)} |")
    lines.append("")
    # Group-branch map — sub-verbs 7/8/14 read ds:0x47C4 (scene group)
    lines.append("## Group-branch sub-verbs (7 / 8 / 14)")
    lines.append("")
    lines.append("These dispatch through bigger handlers (`unknown_2170` etc.)")
    lines.append("keyed on the *scene group* `[ds:0x47C4]` rather than scene_id.")
    lines.append("Scene group is a separate state variable — these sub-verbs")
    lines.append("are how cinematic interludes branch on PoV character / chapter.")
    lines.append("")
    lines.append("| scene_id | sv7 | sv8 | sv14 |")
    lines.append("|---------:|----:|----:|-----:|")
    for s in valid_scenes:
        sv7  = s.subverb_histogram.get(7,  0)
        sv8  = s.subverb_histogram.get(8,  0)
        sv14 = s.subverb_histogram.get(14, 0)
        if sv7 + sv8 + sv14 == 0:
            continue
        lines.append(f"| {s.scene_id} | {sv7} | {sv8} | {sv14} |")
    lines.append("")
    # Per-scene full listing
    lines.append("## Per-scene listings")
    lines.append("")
    for s in scenes:
        lines.append(f"### scene_id {s.scene_id}")
        lines.append("")
        if not s.valid:
            lines.append(f"- file_offset = `0x{s.file_offset:04X}` → **out of file**, skipped.")
            lines.append("")
            continue
        terminator = "FFFF" if s.terminated else "(no FFFF — hit walk cap)"
        lines.append(f"- file_offset = `0x{s.file_offset:04X}`")
        lines.append(f"- instructions = **{s.instr_count}**  ·  terminator = {terminator}")
        hist_str = ", ".join(
            f"sv{k}={v}" for k, v in sorted(s.subverb_histogram.items())
        )
        lines.append(f"- sub-verb histogram: {hist_str or '(empty)'}")
        if s.instrs:
            lines.append("")
            lines.append("```")
            lines.append("offs   raw           cons mode g4 sv  pred sel  ex  phrase  name")
            for ins in s.instrs:
                raw = " ".join(f"{b:02X}" for b in ins.raw)
                lines.append(
                    f"{ins.file_offset:04X}   {raw}   "
                    f"{int(ins.consumed)}    {int(ins.mode)}    {ins.gate_hi}  "
                    f"{ins.subverb:>2}  {ins.predicate_id:02X}   "
                    f"{ins.sel:02X}   {ins.extra:02X}  "
                    f"0x{ins.phrase_id:03X}   {ins.subverb_name}"
                )
            lines.append("```")
        lines.append("")
    REPORT_PATH.parent.mkdir(parents=True, exist_ok=True)
    REPORT_PATH.write_text("\n".join(lines))


def main() -> None:
    buf = ORNY_PATH.read_bytes()
    print(f"ORNY.HSQ.bin = {len(buf)} bytes (= 0x{len(buf):X})")
    header_first_scene = struct.unpack_from('<H', buf, 0)[0]
    header_table_end   = struct.unpack_from('<H', buf, 2)[0]
    print(f"Header u16[0] = 0x{header_first_scene:04X} (first valid scene id per Tech/19)")
    print(f"Header u16[1] = 0x{header_table_end:04X}   (table end sentinel per Tech/19)")
    print()
    boundaries = find_scene_boundaries(buf)
    # Build per-scene "stop_at" bounds. Tech/19 claims FFFF terminators
    # but a byte-scan shows ORNY has ZERO 0xFF bytes anywhere — see the
    # Tech/49 doc generated alongside. We instead bound each scene by
    # the next strictly-greater offset in the table (otherwise: end of
    # file). This catches the natural "cinematic chapter" boundaries
    # for scene_id 2..3 without truncating scenes 4..9 which legitimately
    # share a tail block via overlapping entry points.
    sorted_offsets = sorted({off for _, off, in_file in boundaries if in_file})
    def next_higher(o: int) -> int | None:
        for v in sorted_offsets:
            if v > o:
                return v
        return None
    scenes: list[Scene] = []
    for sid, off, in_file in boundaries:
        if not in_file:
            scenes.append(Scene(scene_id=sid, file_offset=off, valid=False,
                                instr_count=0, terminated=False,
                                subverb_histogram={}, nav_records=[], instrs=[]))
            continue
        scenes.append(decode_scene(buf, sid, off, stop_at=next_higher(off)))
    # Stop the scene list at the first INVALID offset — past that point the
    # table is "wild" and not engine-active per Tech/19.
    pruned: list[Scene] = []
    for s in scenes:
        pruned.append(s)
        if not s.valid:
            break
    print(f"Decoded {len(pruned)} scene entries (scene_id "
          f"{pruned[0].scene_id}..{pruned[-1].scene_id})")
    print()
    print("Per-scene summary:")
    print(f"{'sid':>4}  {'offset':>6}  {'instrs':>6}  {'terminated':>10}  histogram")
    for s in pruned:
        hist_str = ", ".join(f"sv{k}={v}" for k, v in sorted(s.subverb_histogram.items()))
        term = "FFFF" if s.terminated else ("oob" if not s.valid else "...")
        print(f"  {s.scene_id:>2}  0x{s.file_offset:04X}  {s.instr_count:>6}  "
              f"{term:>10}  {hist_str}")
    write_json(pruned)
    write_markdown(pruned)
    print()
    print(f"Wrote JSON   → {JSON_PATH.relative_to(ROOT)}")
    print(f"Wrote report → {REPORT_PATH.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
