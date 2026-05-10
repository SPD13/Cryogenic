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
        DefineFunction(cs1, 0xCA60, HnmDoFrame_1000_CA60_01CA60);
        DefineFunction(cs1, 0xC9E8, HnmDoFrameSkippable_1000_C9E8_01C9E8);
        DefineFunction(cs1, 0xCA01, HnmCloseResource_1000_CA01_01CA01);
        DefineFunction(cs1, 0xDDF0, BootIntroWait_1000_DDF0_01DDF0);
        // Boot-intro per-HNM load helpers — each ends with `jmp 0xCA1B`,
        // which Spice86 doesn't intercept via DefineFunction. We
        // override the load helpers themselves (entered via `call`)
        // so our stub fires for the load AND its tail-chained
        // hnm_load_ida. NearRet here pops the dispatcher's saved
        // return and aborts the entire load chain.
        DefineFunction(cs1, 0x061C, LoadVirginHnm_1000_061C_01061C);
        DefineFunction(cs1, 0x064D, LoadCryoHnm_1000_064D_01064D);
        DefineFunction(cs1, 0x0658, LoadCryo2Hnm_1000_0658_010658);
        DefineFunction(cs1, 0x0678, LoadPresentHnm_1000_0678_010678);
        DefineFunction(cs1, 0x069E, LoadIntroHnm_1000_069E_01069E);
    }

    private Action FastForwardLoadHelper(string label, byte hnmIdHint) {
        if (HarnessFastForwardHnm) {
            _hnmLoadCount++;
            if (_hnmLoadCount <= 30 || _hnmLoadCount % 50 == 0) {
                Console.Error.WriteLine($"[harness-fwd] load-helper {label} #{_hnmLoadCount} (ax pre={AX:X4})");
            }
            globalsOnDs.Set1138_DBE7_Byte8_hnmFinishedFlag(2);
            ClearCarry();
            return NearRet();
        }
        return NearRet();
    }

    public Action LoadVirginHnm_1000_061C_01061C(int gotoAddress)   => FastForwardLoadHelper("VIRGIN", 0x15);
    public Action LoadCryoHnm_1000_064D_01064D(int gotoAddress)     => FastForwardLoadHelper("CRYO", 0x16);
    public Action LoadCryo2Hnm_1000_0658_010658(int gotoAddress)    => FastForwardLoadHelper("CRYO2", 0x17);
    public Action LoadPresentHnm_1000_0678_010678(int gotoAddress)  => FastForwardLoadHelper("PRESENT", 0x18);
    public Action LoadIntroHnm_1000_069E_01069E(int gotoAddress)    => FastForwardLoadHelper("INTRO", 0x0F);

    /// <summary>
    /// Override for cs1:DDF0 — the boot-intro dispatcher's per-record
    /// WAIT LOOP. Native code at this address loops `call 0xABA3 →
    /// call 0xDD63 (stc_on_user_input) → jnc back` until either a
    /// scene-timer condition fires (jz path) or the user provides
    /// input (jc path). This is what makes each record take ~10 s
    /// wall time under headless emulation — the loop spins on the
    /// real-time PIT.
    ///
    /// Under harness fast-fwd: return immediately with CF=0 so the
    /// dispatcher's `jnc 0x592` jumps back to dispatch the next
    /// record without waiting.
    /// </summary>
    public Action BootIntroWait_1000_DDF0_01DDF0(int gotoAddress) {
        if (HarnessFastForwardHnm) {
            ClearCarry();
            return NearRet();
        }
        ClearCarry();
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:CA60 — `hnm_do_frame_ida`. The engine's
    /// per-tick gfx handler calls this every PIT interrupt; without
    /// a fast stub the native frame decoder grinds through bytes
    /// even with no loaded HNM. Returns immediately under harness.
    /// </summary>
    public Action HnmDoFrame_1000_CA60_01CA60(int gotoAddress) {
        if (HarnessFastForwardHnm) {
            ZeroFlag = false;
            return NearRet();
        }
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:C9E8 — `hnm_do_frame_skippable_ida`. Wrapper
    /// around frame decode; stubbed for the same reason as above.
    /// </summary>
    public Action HnmDoFrameSkippable_1000_C9E8_01C9E8(int gotoAddress) {
        if (HarnessFastForwardHnm) {
            ZeroFlag = false;
            return NearRet();
        }
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:CA01 — `hnm_close_resource_ida`. Frees the
    /// HNM file handle / buffer. Under harness fast-fwd, no-op since
    /// HnmLoad didn't actually open anything.
    /// </summary>
    public Action HnmCloseResource_1000_CA01_01CA01(int gotoAddress) {
        if (HarnessFastForwardHnm) {
            globalsOnDs.Set1138_DBE7_Byte8_hnmFinishedFlag(0);
            ClearCarry();
            return NearRet();
        }
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:CA1B — `hnm_load_ida`. Loads an HNM resource
    /// (opens file, reads header). Under harness fast-forward, we
    /// pretend the resource loaded successfully without actually
    /// touching disk — so the subsequent play_*_HNM loop exits
    /// after one CheckIfHnmComplete tick.
    /// </summary>
    private static int _hnmLoadCount = 0;
    public Action HnmLoad_1000_CA1B_01CA1B(int gotoAddress) {
        if (HarnessFastForwardHnm) {
            _hnmLoadCount++;
            Console.Error.WriteLine($"[harness-fwd] hnm_load #{_hnmLoadCount} ax={AX:X4}");
            globalsOnDs.Set1138_DBE7_Byte8_hnmFinishedFlag(2);
            ClearCarry();
            return NearRet();
        }
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
            if (_hnmFastForwardCount <= 30 || _hnmFastForwardCount % 50 == 0) {
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