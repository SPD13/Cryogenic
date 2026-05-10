namespace Cryogenic.Overrides;

using Spice86.Core.Emulator.ReverseEngineer;

/// <summary>
/// Partial class containing video playback related overrides for HNM video format.
/// </summary>
/// <remarks>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly.
/// </remarks>
public partial class Overrides {
    /// <summary>
    /// Registers video playback related function overrides with Spice86.
    /// </summary>
    public void DefineVideoCodeOverrides() {
        DefineFunction(cs1, 0xC921, GetHnmResourceFlagNamePtrByIndexAXToBx_1000_C921_01C921);
        DefineFunction(cs1, 0xCA59, VideoPlayRelated_1000_CA59_01CA59);
        DefineFunction(cs1, 0xCC85, CheckIfHnmComplete_1000_CC85_01CC85);
        DefineFunction(cs1, 0xC9F4, DoFrameAndCheckIfFrameAdvanced_1000_C9F4_01C9F4);
        DefineFunction(cs1, 0xCA1B, HnmLoad_1000_CA1B_01CA1B);
    }

    /// <summary>
    /// Override for cs1:CA1B — `hnm_load_ida`. Loads an HNM resource
    /// (opens file, reads header). Under harness fast-forward, we
    /// pretend the resource loaded successfully without actually
    /// touching disk — so the subsequent play_*_HNM loop exits
    /// after one CheckIfHnmComplete tick.
    /// </summary>
    public Action HnmLoad_1000_CA1B_01CA1B(int gotoAddress) {
        if (HarnessFastForwardHnm) {
            // Mark "loaded successfully": set the finished flag to
            // anything-not-0-or-1 so CheckIfHnmComplete's normal path
            // ALSO returns ZF=clear if our HNM override fast-fwd is
            // somehow bypassed.
            globalsOnDs.Set1138_DBE7_Byte8_hnmFinishedFlag(2);
            ClearCarry();
            return NearRet();
        }
        // No override outside harness — Spice86 falls back to native.
        // (We still get registered so the function info exists.)
        ClearCarry();
        return NearRet();
    }

    private void ClearCarry() {
        State.CarryFlag = false;
    }

    /// <summary>
    /// When the CRYO_HARNESS_FAST_HNM env var is set, the harness wants
    /// every HNM playback loop to exit after one iteration so the boot
    /// intro fast-forwards through the cinematics to reach post-MTG1
    /// scene logic. Used by Cryogenic.Harness for memory-write tracing
    /// of the boot intro flow. Default: false (normal game playback).
    /// </summary>
    private static bool HarnessFastForwardHnm =>
        Environment.GetEnvironmentVariable("CRYO_HARNESS_FAST_HNM") is not null;

    /// <summary>
    /// Override for CS1:C9F4 — `do_frame_and_check_if_frame_advanced`.
    /// When the harness fast-forward env var is set, force ZF=0
    /// (frame advanced) so the play_*_HNM outer loop exits in one
    /// iteration after CheckIfHnmComplete reports "complete".
    /// Otherwise: falls back to native code (caller should not have
    /// installed this override) — but for safety we keep ZF unset.
    /// </summary>
    public Action DoFrameAndCheckIfFrameAdvanced_1000_C9F4_01C9F4(int gotoAddress) {
        if (HarnessFastForwardHnm) {
            ZeroFlag = false;
            return NearRet();
        }
        // No fast-forward — return without changing the engine's
        // state; the native loop will keep running.
        ZeroFlag = false;
        return NearRet();
    }

    /// <summary>
    /// Override for CS1:CC85 - Checks if the current HNM video has finished playing.
    /// </summary>
    /// <param name="gotoAddress">Target address for potential jumps (unused in this override).</param>
    /// <returns>A near return action to exit the function.</returns>
    /// <remarks>
    /// Sets the Zero flag based on the HNM finished flag value (0 or 1 sets ZF, other values clear it).
    /// </remarks>
    private static int _hnmFastForwardCount = 0;
    public Action CheckIfHnmComplete_1000_CC85_01CC85(int gotoAddress) {
        if (HarnessFastForwardHnm) {
            _hnmFastForwardCount++;
            if (_hnmFastForwardCount % 50 == 1) {
                Console.Error.WriteLine($"[harness-fwd] HNM fast-forward hit #{_hnmFastForwardCount}");
            }
            // Force "complete" so the play_*_HNM loop exits.
            ZeroFlag = false;
            return NearRet();
        }
        int value = globalsOnDs.Get1138_DBE7_Byte8_hnmFinishedFlag();
        _loggerService.Debug("DBE7={@DBE7}", value);
        ZeroFlag = value is 0 or 1;
        return NearRet();
    }

    /// <summary>
    /// Override for CS1:C921 - Reads an HNM resource flag name pointer by index and returns it in BX.
    /// </summary>
    /// <param name="gotoAddress">Target address for potential jumps (unused in this override).</param>
    /// <returns>A near return action to exit the function.</returns>
    /// <remarks>
    /// Reads value at DS:(AX*2)+0x33A3 and returns it in BX register.
    /// Only executed when a video starts. The offset 0x33A3 points to a lookup table
    /// for HNM resource names or flags.
    /// </remarks>
    public Action GetHnmResourceFlagNamePtrByIndexAXToBx_1000_C921_01C921(int gotoAddress) {
        // Only executed when a video starts
        ushort offset = (ushort)(AX * 2 + 0x33A3);
        ushort value = UInt16[DS, offset];
        _loggerService.Debug("read33A3WithAxOffset {@Ax} {@Value}", AX, value);
        BX = value;
        return NearRet();
    }

    /// <summary>
    /// Override for CS1:CA59 - Copies a video playback related index value between global variables.
    /// </summary>
    /// <param name="gotoAddress">Target address for potential jumps (unused in this override).</param>
    /// <returns>A near return action to exit the function.</returns>
    /// <remarks>
    /// Only executed during video playback. The function appears to have minimal impact
    /// on actual video playback, likely serving as state synchronization.
    /// </remarks>
    public Action VideoPlayRelated_1000_CA59_01CA59(int gotoAddress) {
        // seems to have no impact what so ever is done here. Only executed during videos
        ushort value = globalsOnDs.Get1138_CE7A_Word16_VideoPlayRelatedIndex();
        globalsOnDs.Set1138_DC22_Word16_VideoPlayRelatedIndex(value);
        _loggerService.Debug("videoPlayRelated value:{@Value}", value);
        return NearRet();
    }
}