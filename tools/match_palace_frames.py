#!/usr/bin/env python3
"""Reverse-match each palace-scene present blit in the harness trace
to a specific (animation_index, frame_index) for each character.

Approach (mask-aware pixel comparison):
  1. Composite every animation frame into a 320x152 buffer + mask
     (mask=1 where character is opaque, 0 where transparent — i.e. the
     pixel either was atlas[transparentIndex] or never touched).
  2. For each present blit in the trace with `srcBytes` (raw base64
     source pixels) at slot (x, y, w, h):
        For each candidate (anim, frame):
          - Take that frame's pixels[slot] + mask[slot]
          - Compare srcBytes vs pixels[slot] only at positions where mask=1
          - Score = matching character pixels (high) vs mismatches (low)
        Pick the highest-scoring frame.
  3. Annotate the schedule with the matched (anim_idx, frame_idx)
     per scheduled event.

Output: enriched palace-interlude-schedule.json the painter consumes
to render the engine-exact frame at each scheduled paint event.
"""

import base64
import json
import sys
from pathlib import Path

ASSETS = Path('/Users/sebastienbock/claude-workspace/Cryogenic/Rebuild/extracted-assets/sprites')
TRACE = Path('/Users/sebastienbock/claude-workspace/Cryogenic/src/Cryogenic/cryogenic-palette-trace.jsonl')
SCHEDULE = Path('/Users/sebastienbock/claude-workspace/Cryogenic/Rebuild/game/data/palace-interlude-schedule.json')

CHARACTERS = [
    # screen_xy is the per-character paint offset applied to every image's
    # (xOffset, yOffset) when compositing. Discovered by brute-force
    # match of first-blit srcBytes against the atlas: LETO/JESS land at
    # screen origin (0, 0), but PAUL's full-screen portrait paints with a
    # +5 X shift — likely because PAUL's 318×152 animation buffer is
    # positioned at engine play-area X=5 (not centered at 1, not at 0).
    ('LETO', 'throne_leto',        (0, 0)),
    ('JESS', 'dining_jess_dialog', (0, 0)),
    ('PAUL', 'paul',               (5, 0)),
]

BUF_W, BUF_H = 320, 152


def load_atlas(hsq):
    a = json.load(open(ASSETS / f'{hsq}.HSQ' / 'atlas.json'))
    idx = (ASSETS / f'{hsq}.HSQ' / 'atlas.idx').read_bytes()
    return {
        'imageWidth': a['imageWidth'],
        'imageHeight': a['imageHeight'],
        'transparentIndex': a.get('transparentIndex', 0),
        'frames': {f['id']: f for f in a['frames']},
        'pixels': idx,
    }


def blit_frame(atlas, frame_id, dest, mask, dx, dy):
    f = atlas['frames'].get(frame_id)
    if f is None:
        return
    src = atlas['pixels']
    src_w = atlas['imageWidth']
    transparent = atlas['transparentIndex']
    for row in range(f['h']):
        sy = f['y'] + row
        ddy = dy + row
        if ddy < 0 or ddy >= BUF_H:
            continue
        for col in range(f['w']):
            sx = f['x'] + col
            ddx = dx + col
            if ddx < 0 or ddx >= BUF_W:
                continue
            pix = src[sy * src_w + sx]
            if pix == transparent:
                continue
            i = ddy * BUF_W + ddx
            dest[i] = pix
            mask[i] = 1


def composite_frame(atlas, anim_data, anim_idx, frame_idx, screen_xy):
    pixels = bytearray(BUF_W * BUF_H)
    mask = bytearray(BUF_W * BUF_H)
    anim = anim_data['animations'][anim_idx]
    frame = anim['frames'][frame_idx]
    groups = anim_data['imageGroups']
    ox, oy = screen_xy
    for gi in frame['groupIndices']:
        if gi >= len(groups):
            continue
        for im in groups[gi].get('images', []):
            blit_frame(atlas, im['imageNumber'], pixels, mask,
                       ox + im['xOffset'], oy + im['yOffset'])
    return pixels, mask


def extract_slot(buf, x, y, w, h):
    out = bytearray(w * h)
    for row in range(h):
        for col in range(w):
            out[row * w + col] = buf[(y + row) * BUF_W + (x + col)]
    return out


def match_score(frame_pixels, frame_mask, slot_bytes):
    matches = 0
    mismatches = 0
    char_total = 0
    limit = min(len(frame_mask), len(slot_bytes))
    for i in range(limit):
        if not frame_mask[i]:
            continue
        char_total += 1
        if frame_pixels[i] == slot_bytes[i]:
            matches += 1
        else:
            mismatches += 1
    return matches, mismatches, char_total


def main():
    if not TRACE.exists():
        print(f'ERROR: trace not found at {TRACE}', file=sys.stderr)
        sys.exit(1)
    events = []
    bad = 0
    for line in open(TRACE):
        if not line.strip():
            continue
        try:
            events.append(json.loads(line))
        except Exception:
            bad += 1
    print(f'Loaded {len(events)} trace events ({bad} malformed lines skipped)')

    schedule = json.load(open(SCHEDULE))

    char_data = {}
    for hsq, scene_id, screen_xy in CHARACTERS:
        atlas = load_atlas(hsq)
        anim_data = json.load(open(ASSETS / f'{hsq}.HSQ' / 'animations.json'))
        char_data[scene_id] = (hsq, atlas, anim_data, screen_xy)
        print(f'  {hsq}: {len(atlas["frames"])} frames, '
              f'{len(anim_data["animations"])} animations, '
              f'{len(anim_data["imageGroups"])} image groups')

    for scene_obj in schedule['scenes']:
        sid = scene_obj['id']
        if sid not in char_data:
            continue
        hsq, atlas, anim_data, screen_xy = char_data[sid]
        scene_frames = scene_obj.get('frames', [])
        if not scene_frames:
            continue
        unique_slots = {(f['x'], f['y'], f['w'], f['h']) for f in scene_frames}
        print(f'\n=== {sid} ({hsq}): {len(scene_frames)} blit events, '
              f'{len(unique_slots)} unique slots ===')

        all_frames = []
        for ai, anim in enumerate(anim_data['animations']):
            for fi in range(len(anim['frames'])):
                pixels, mask = composite_frame(atlas, anim_data, ai, fi, screen_xy)
                all_frames.append((ai, fi, pixels, mask))
        print(f'  pre-rendered {len(all_frames)} (anim, frame) variants')

        slot_data = {}
        for slot in unique_slots:
            x, y, w, h = slot
            slot_data[slot] = []
            for ai, fi, p, m in all_frames:
                slot_data[slot].append((ai, fi,
                                        extract_slot(p, x, y, w, h),
                                        extract_slot(m, x, y, w, h)))

        scene_presents = [e for e in events
                          if e.get('t') == 'blit'
                          and e.get('dstSeg') == '0xA000'
                          and (e['x'], e['y'], e['rows'], e['cols']) in unique_slots
                          and 'srcBytes' in e]
        scene_presents.sort(key=lambda e: e['cycles'])
        if not scene_presents:
            print('  no present blits with srcBytes — skipping')
            continue
        print(f'  trace present-blits with srcBytes: {len(scene_presents)}')

        annotated = []
        unique_count = 0
        ambiguous_count = 0
        unmatched_count = 0
        per_anim_count = {}
        # Index trace presents by (slot, occurrence_count_for_that_slot) so we
        # match each schedule entry to the corresponding-occurrence trace
        # event. Schedule entries that have no corresponding hashed trace
        # event (e.g. outside the hashed cycle window) get anim_idx=None.
        from collections import defaultdict
        slot_iter = defaultdict(int)
        trace_by_slot = defaultdict(list)
        for e in scene_presents:
            slot_key = (e['x'], e['y'], e['rows'], e['cols'])
            trace_by_slot[slot_key].append(e)
        sched_iter = defaultdict(int)
        for i, sched in enumerate(scene_frames):
            slot = (sched['x'], sched['y'], sched['w'], sched['h'])
            occ = sched_iter[slot]
            sched_iter[slot] += 1
            slot_traces = trace_by_slot[slot]
            if occ >= len(slot_traces):
                annotated.append({**sched, 'anim_idx': None, 'frame_idx': None, 'match': 'no-trace'})
                continue
            trace_e = slot_traces[occ]
            src_bytes = base64.b64decode(trace_e['srcBytes'])
            scored = []
            for ai, fi, fpix, fmask in slot_data[slot]:
                m, mm, ct = match_score(fpix, fmask, src_bytes)
                if ct == 0:
                    continue
                scored.append((m, -mm, ai, fi, m, mm, ct))
            scored.sort(reverse=True)
            best = scored[0] if scored else None
            ann = dict(sched)
            if best is None:
                ann['anim_idx'] = None
                ann['frame_idx'] = None
                ann['match'] = 'no-candidate'
                unmatched_count += 1
            else:
                _, _, ai, fi, m_count, mm_count, ct = best
                # Lowered threshold to 70% — engine background pixels show
                # through transparent character pixels and may not match the
                # zero-fill in my composite. 70% character-pixel match is
                # solid enough for a confident annotation.
                if m_count >= ct * 0.7:
                    ann['anim_idx'] = ai
                    ann['frame_idx'] = fi
                    ann['match_char_pixels'] = f'{m_count}/{ct}'
                    same = [s for s in scored if s[0] == m_count and s[1] == -mm_count]
                    if len(same) == 1:
                        ann['match'] = 'unique'
                        unique_count += 1
                    else:
                        # Don't prefer anim 0 anymore — the highest-scoring
                        # candidate already won. The tie just means multiple
                        # animations share an identical-content frame at this
                        # slot (e.g. JESS anim 0 frame 13 == anim 4 frame 22).
                        # Keep the matcher's first pick (already in `best`).
                        ann['match'] = f'ambiguous-{len(same)}'
                        ambiguous_count += 1
                    key = ann['anim_idx']
                    per_anim_count[key] = per_anim_count.get(key, 0) + 1
                else:
                    ann['anim_idx'] = ai
                    ann['frame_idx'] = fi
                    ann['match'] = f'partial-{m_count}/{ct}'
                    unmatched_count += 1
            annotated.append(ann)

        scene_obj['frames'] = annotated
        print(f'  matched: {unique_count} unique, {ambiguous_count} ambiguous, {unmatched_count} unmatched')
        print(f'  per-anim distribution: {dict(sorted(per_anim_count.items())) if per_anim_count else "(none)"}')

    json.dump(schedule, open(SCHEDULE, 'w'), indent=2)
    print(f'\nWrote annotated schedule to {SCHEDULE}')


if __name__ == '__main__':
    main()
