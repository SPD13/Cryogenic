using Cryogenic;
using Cryogenic.Harness;

string[] forwardedArgs = HarnessCli.Parse(args);

// Same trick the main Cryogenic project uses: reserve segment 0x800 for BIOS/IRQ
// handlers so the Cryo drivers can be remapped to clean 0x?000 segments.
List<string> finalArgs = forwardedArgs.ToList();
finalArgs.Add($"--ProvidedAsmHandlersSegment={DriverLoadToolbox.INTERRUPT_HANDLER_SEGMENT}");

Console.Error.WriteLine($"[harness] launching Spice86 (mode={HarnessContext.Options.Mode}, " +
                        $"checkpoint={HarnessContext.Options.Checkpoint.Segment:X4}:{HarnessContext.Options.Checkpoint.Offset:X4})");

global::Spice86.Program.RunWithOverrides<Cryogenic.Harness.HarnessOverrideSupplier>(
    finalArgs.ToArray(),
    "5F30AEB84D67CF2E053A83C09C2890F010F2E25EE877EBEC58EA15C5B30CFFF9");

bool ok = HarnessContext.Options.Mode switch {
    HarnessMode.SnapshotOnCheckpoint => HarnessContext.Options.SnapshotTaken,
    HarnessMode.Invoke => HarnessContext.Options.InvokeReturnCaptured,
    _ => false,
};

Console.Error.WriteLine($"[harness] Spice86 returned. ok={ok}");

// Spice86 spawns audio device threads (SoundBlaster, OPL3FM) as foreground threads, so
// returning from Main isn't enough — the .NET runtime won't exit until they do, and
// they're typically only released on hard shutdown. Force the process down ourselves.
Environment.Exit(ok ? 0 : 2);
