#!/usr/bin/env python3
"""Identify which atlas-frame (sprite) the engine painted at each
position for every compose blit during the palace interlude.

Approach:
  1. Read the harness trace; for each compose blit (src=0x335B,
     dst=0x42FB) decode `bufBytes` — the full character-area buffer
     state at that moment (260x152 anchored at (30, 0) on screen).
  2. For each animation's `imageGroups[].images[]`, walk the known
     paint positions (xOffset, yOffset). At each position, the engine
     painted SOME atlas frame. Extract the corresponding sub-rect
     from the buffer.
  3. For each atlas frame, render its bytes (W x H) and compare to
     the extracted sub-rect. The matching atlas frame is the engine's
     sprite choice at that position.
  4. Output: per compose event, a list of (group_position_xy,
     atlas_frame_id) tuples — the engine's actual sprite selections.

This avoids guessing animation/frame indices. Instead we ground-truth
each sprite slot against the engine's runtime composite.
"""

import base64
import json
import sys
from pathlib import Path

ASSETS = Path('/Users/sebastienbock/claude-workspace/Cryogenic/Rebuild/extracted-assets/sprites')
TRACE = Path('/Users/sebastienbock/claude-workspace/Cryogenic/src/Cryogenic/cryogenic-palette-trace.jsonl')

# Character → (HSQ, scene cycle range, primary slot for filtering).
# Scene cycle ranges come from the harness capture log (palace_scene_NN
# auto-snapshots). Filtering excludes other characters' composes.
CHARACTERS = [
    ('LETO', 'throne_leto',        (57, 22, 46, 20)),
    ('JESS', 'dining_jess_dialog', (44, 47, 70, 24)),
    ('PAUL', 'paul',               (125, 44, 70, 21)),
]

# Buffer rect that the C# logger dumps in `bufBytes` (must match).
BUF_X, BUF_Y = 30, 0
BUF_W, BUF_H = 260, 152


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


def atlas_image_pixels(atlas, frame_id):
    """Return a flat W*H bytearray for the atlas frame, indexed
    row-major. Pixels at transparentIndex stay as-is in the buffer;
    callers should use the mask logic to compare correctly."""
    f = atlas['frames'].get(frame_id)
    if f is None:
        return None, 0, 0
    w, h = f['w'], f['h']
    out = bytearray(w * h)
    src = atlas['pixels']
    sw = atlas['imageWidth']
    for row in range(h):
        for col in range(w):
            out[row * w + col] = src[(f['y'] + row) * sw + (f['x'] + col)]
    return out, w, h


def extract_buf_rect(buf, x, y, w, h):
    """Extract a w*h sub-rect from `buf` (a BUF_W * BUF_H bytearray).
    Coordinates are SCREEN coordinates; adjusted by (BUF_X, BUF_Y)."""
    bx, by = x - BUF_X, y - BUF_Y
    out = bytearray(w * h)
    for row in range(h):
        sy = by + row
        if sy < 0 or sy >= BUF_H:
            continue
        for col in range(w):
            sx = bx + col
            if sx < 0 or sx >= BUF_W:
                continue
            out[row * w + col] = buf[sy * BUF_W + sx]
    return out


def match_sprite_at(atlas, buf, paint_xy, screen_xy_offset=(0, 0)):
    """Find the atlas frame_id whose pixels best match the buffer
    sub-rect at paint position `paint_xy`. Returns (frame_id, score)
    where score is matching-opaque-pixel count. Skips transparent
    pixels in the candidate during comparison.

    `paint_xy` is the engine's xOffset/yOffset for an image group
    PRE adding `screen_xy_offset`. For PAUL screen_xy_offset is (5,0).
    """
    candidates = []
    pxs, pys = screen_xy_offset
    paint_x = paint_xy[0] + pxs
    paint_y = paint_xy[1] + pys
    transparent = atlas['transparentIndex']
    for fid, f in atlas['frames'].items():
        w, h = f['w'], f['h']
        if w <= 0 or h <= 0:
            continue
        # Extract from buffer at paint position
        sub = extract_buf_rect(buf, paint_x, paint_y, w, h)
        # Read atlas image
        atlas_pix = bytearray(w * h)
        src = atlas['pixels']
        sw = atlas['imageWidth']
        for row in range(h):
            for col in range(w):
                atlas_pix[row * w + col] = src[(f['y'] + row) * sw + (f['x'] + col)]
        # Score: at each position where atlas pixel != transparent,
        # buffer pixel should equal atlas pixel.
        matches = 0
        char_total = 0
        for i, ap in enumerate(atlas_pix):
            if ap == transparent:
                continue
            char_total += 1
            if sub[i] == ap:
                matches += 1
        if char_total == 0:
            continue
        score = matches / char_total
        candidates.append((score, matches, char_total, fid))
    candidates.sort(reverse=True)
    return candidates[:5]  # top 5


def main():
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

    events_sorted = sorted([e for e in events if 'cycles' in e], key=lambda e: e['cycles'])

    # Find first MTG1 and MTG2
    hnms = [(e['cycles'], int(e['hnmIdHex'], 16)) for e in events_sorted if e['t'] == 'hnm']
    mtg1 = next(c for c, h in hnms if h == 0x10)
    mtg2 = next(c for c, h in hnms if h == 0x11)
    print(f'MTG1 at {mtg1:,}; MTG2 at {mtg2:,}')

    # Per character: pick first compose blit with bufBytes, analyse sprite
    # choices at every group's expected paint position.
    for hsq, scene_id, slot in CHARACTERS:
        atlas = load_atlas(hsq)
        anim_data = json.load(open(ASSETS / f'{hsq}.HSQ' / 'animations.json'))
        # Find composes in scene
        composes = [e for e in events_sorted
                    if e.get('t') == 'blit'
                    and e.get('dstSeg') == '0x42FB'
                    and e.get('srcSeg') == '0x335B'
                    and 'bufBytes' in e
                    and (e['x'], e['y'], e['rows'], e['cols']) == slot
                    and mtg1 < e['cycles'] < mtg2]
        print(f'\n=== {hsq} ({scene_id}): {len(composes)} composes with bufBytes ===')
        if not composes:
            continue
        # Collect all (xOffset, yOffset) positions referenced by any
        # animation's imageGroups (deduplicated).
        positions = set()
        for grp in anim_data['imageGroups']:
            for im in grp.get('images', []):
                positions.add((im['xOffset'], im['yOffset']))
        positions = sorted(positions)
        print(f'  unique paint positions in animations.json: {len(positions)}')

        # PAUL needs (5, 0) screen offset
        screen_xy_off = (5, 0) if hsq == 'PAUL' else (0, 0)

        # For the first compose, identify which atlas frame matches at each
        # known position. Print top match.
        e = composes[0]
        buf = base64.b64decode(e['bufBytes'])
        print(f'  first compose: cycles={e["cycles"]:,}')
        for paint_xy in positions[:25]:
            tops = match_sprite_at(atlas, buf, paint_xy, screen_xy_off)
            if not tops:
                continue
            score, matches, total, fid = tops[0]
            if score < 0.7:
                continue
            print(f'    at {paint_xy}: frame_id={fid} ({matches}/{total} = {100*score:.0f}%)')


if __name__ == '__main__':
    main()
