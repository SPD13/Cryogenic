namespace Cryogenic.Harness;

/// <summary>
/// Process-wide context shared between the CLI front end and the override supplier
/// that Spice86 instantiates for us. Spice86 only knows how to construct the supplier
/// reflectively, so we have to smuggle harness configuration in through a static
/// holder. Single-process, single-run — no concurrency concerns.
/// </summary>
public static class HarnessContext {
    public static HarnessOptions Options { get; set; } = new();
}
