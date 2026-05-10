# Cryogenic.Harness

A headless driver for `DNCDPRG.EXE` running in Spice86. Use it to capture
checkpoint snapshots, drive specific subroutines directly, and record
traces for offline reverse-engineering — without sitting through a full
playthrough.

## Status

- **Phase 1 — bootstrap + snapshot**: boots the game in Spice86, hooks a
  configurable checkpoint address, dumps RAM + CPU state, and exits.
- **Phase 2 — direct invoker**: redirects CS:IP into a target function
  with controlled args + a sentinel return, captures pre/post register
  state.
- **Phase 3 — trace pipe**: per-known-function entry trace + RAM memdiff
  layered on the invoke flow. The flow recorder inside Spice86 is hooked
  through reflection for best-effort full call/jump/ret capture.
- **Phase 4 — instruction trace**: per-instruction ndjson trace inside a
  configurable linear-address window (one execution breakpoint per byte
  in range). The memory R/W tape is deferred — Spice86's breakpoint
  callback doesn't carry the triggering address through the public API,
  so we'd need deeper integration to do it cleanly.
- **Phase 5 — opcode sweep**: a Python orchestrator
  (`tools/harness/sweep.py`) drives the harness across a parameter grid
  and aggregates the per-input traces into a single `sweep-report.json`.
  Each slot in the config supplies CLI fragments (write-mem, arg-reg,
  target) so the same machinery handles DIALOGUE opcodes, HNM blocks,
  and any other call-with-controlled-input task.
- **Phase 6 — HNM Block 172 infrastructure**: starter config at
  `tools/harness/configs/hnm-block172.json.template` plus a recipe in
  `DOCUMENTATION/Tech/29-trace-harness.md`. The decoder entry is at
  `1000:CC96` (`hnm_decode_video_frame_ida`); the actual block-172 RE
  is now down to running the harness against the right inputs.

## Quick start

```sh
cd src/Cryogenic.Harness
dotnet build

dotnet run -- \
  --checkpoint 1000:000C \
  --snapshot-out /tmp/dune-snap \
  --Exe ~/claude-workspace/Cryogenic/DNCDPRG.EXE \
  -p 4096 \
  -a "ADL220 SBP2227"
```

Outputs (in `/tmp/dune-snap`):

| File          | Contents                                                |
|---------------|---------------------------------------------------------|
| `ram.bin`     | 1 MiB conventional-memory image                         |
| `state.json`  | CPU general/segment registers + IP + flags + cycles     |
| `meta.json`   | checkpoint address, RAM SHA-256, capture timestamp      |
| `_spice86/`   | Spice86's own auxiliary dumps (kept out of the source tree) |

The snapshot at `1000:000C` matches the post-driver-load checkpoint that
the existing `DefineMemoryDumpsMapping` hook captures, so other tooling
that targets that address can consume our `ram.bin`.

## Flags

### Harness flags

| Flag                    | Default              | Meaning                                |
|-------------------------|----------------------|----------------------------------------|
| `--checkpoint SEG:OFF`  | `1000:000C`          | Address at which to capture state.     |
| `--snapshot-out DIR`    | `./snapshots/default`| Output directory.                      |
| `--no-exit`             | (off)                | Keep emulating after the snapshot.     |
| `--mode NAME`           | `snapshot`           | Reserved for future modes.             |
| `--watch-mem LINEAR`    | (off)                | Log every memory write to LINEAR (or `LINEAR..LINEAR` range) as ndjson. Repeatable. |
| `-h`, `--help`          | —                    | Show help.                             |

### Spice86 flags (forwarded)

Anything we don't recognise we forward to Spice86. The most useful are:

| Flag                  | Default        | Meaning                                |
|-----------------------|----------------|----------------------------------------|
| `-e PATH`             | required       | Path to `DNCDPRG.EXE`.                 |
| `-c PATH`             | exe parent dir | Mounted C: drive root.                 |
| `-p NUM`              | `368`          | Program-load segment (use `4096`).     |
| `-a "STR"`            | none           | EXE args (e.g. `"ADL220 SBP2227"`).    |
| `--VerboseLogs=true`  | `false`        | Verbose Spice86 logs.                  |
| `--SilencedLogs=true` | `false`        | Suppress all Spice86 logs.             |

The harness automatically injects `--HeadlessMode=Minimal`,
`--UseCodeOverride=true`, `--GdbPort=0`, and a
`--RecordedDataDirectory` under the snapshot output directory.

## How it works

1. `HarnessCli.Parse` extracts harness-specific flags and forwards the
   rest to Spice86.
2. `HarnessOverrideSupplier` (passed to `Spice86.Program.RunWithOverrides`)
   first installs every override that the regular `Cryogenic` project
   registers, then layers `HarnessHelper` on top.
3. `HarnessHelper.InstallCheckpointHook` arms a `DoOnTopOfInstruction`
   hook at the configured address. When the CPU hits it, we dump RAM
   via `IMemory.ReadRam(0x100000, 0)`, serialise registers from
   `State`, and write three files to the snapshot directory.
4. With `ExitOnSnapshot=true` (the default) we then call
   `CSharpOverrideHelper.Exit()`, which raises `HaltRequestedException`.
   The emulation loop catches it, returns from `ProgramExecutor.Run`,
   and the process exits cleanly.

## Determinism

Boot is *almost* deterministic but not bit-exact: across two consecutive
runs against the same `DNCDPRG.EXE`, the post-driver-load `ram.bin`
typically differs by a single byte (and the cycle counter by a few
hundred thousand). That byte sits in a DOS scratch region — most likely
a wall-clock-driven counter — and doesn't affect game state at the
checkpoint.

What matters for downstream phases is **restore-from-snapshot**
determinism, which is a pure `memcpy` and is bit-exact by construction.
Phase 2 will lean on that, not on re-bootstrapping each invocation.

Verify:

```sh
shasum -a 256 /tmp/run-a/ram.bin /tmp/run-b/ram.bin
cmp -l /tmp/run-a/ram.bin /tmp/run-b/ram.bin | wc -l   # → 1
```

## Sweeps

`tools/harness/sweep.py` drives the harness across a parameter grid:

```sh
python3 tools/harness/sweep.py \
  --config tools/harness/configs/my-sweep.json \
  --out /tmp/sweep-out

jq '.rows[] | {label, cycles, memdiff_bytes}' \
  /tmp/sweep-out/sweep-report.json
```

The config is a JSON document with three fields:
- `harness_exe` — how to launch the harness (typically `dotnet run …`).
- `common_args` — flags every slot shares.
- `slots` — one entry per invocation; each `extra_args` block can splice
  slot-specific values via `{slot.id}` / `{slot.label}` placeholders.

Each slot writes its outputs under `<out>/slot_<id>/` and is summarised
in the aggregate `sweep-report.json`. Boot-per-invoke means each slot
takes ~10 s; an in-process snapshot-restore loop would speed sweeps but
isn't built yet.

## Known limitations

- Snapshot covers conventional 1 MiB only. XMS/UMB beyond `0xFFFFF`
  isn't captured (the game doesn't appear to use it during init).
- Restoring a snapshot in-process is **not** implemented yet —
  sweeps re-boot Spice86 between slots. ~10 s per slot.
- Memory R/W tape: deferred — needs deeper Spice86 integration than
  the public API exposes.
- Instruction trace at large ranges installs one breakpoint per byte;
  memory cost is ~64 bytes/breakpoint × range size. A 64 KB range
  installs in well under a second.
