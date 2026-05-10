namespace Cryogenic.Harness;

using Spice86.Core.CLI;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.ReverseEngineer;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// Override supplier handed to Spice86. Composes the project's own
/// <see cref="Cryogenic.Overrides.Overrides"/> (so all the regular game overrides
/// keep working) with our harness-specific hooks (<see cref="HarnessHelper"/>).
/// </summary>
/// <remarks>
/// Spice86 reflectively constructs this class via the
/// <c>--OverrideSupplierClassName</c> CLI option, so it has to have a parameterless
/// constructor. Harness-side configuration is read out of <see cref="HarnessContext.Options"/>,
/// which the CLI populated before invoking <c>Spice86.Program.RunWithOverrides</c>.
/// </remarks>
public sealed class HarnessOverrideSupplier : IOverrideSupplier {
    public IDictionary<SegmentedAddress, FunctionInformation> GenerateFunctionInformations(
        ILoggerService loggerService,
        Configuration configuration,
        ushort programStartSegment,
        Machine machine) {
        Dictionary<SegmentedAddress, FunctionInformation> res = new();

        // 1) Bring in every game-level override the regular Cryogenic project registers.
        new Cryogenic.Overrides.Overrides(res, programStartSegment, machine, loggerService, configuration);

        // 2) Layer the harness checkpoint hook on top.
        new HarnessHelper(res, machine, loggerService, configuration, HarnessContext.Options);

        return res;
    }
}
