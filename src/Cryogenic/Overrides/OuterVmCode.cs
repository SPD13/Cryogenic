namespace Cryogenic.Overrides;

/// <summary>
/// Partial class containing the outer dialogue VM at <c>cs1:0x9F9E</c>
/// (Tech/16 §3, Tech/28).
/// </summary>
/// <remarks>
/// <para>
/// The outer VM is a "find-and-fire-the-next-matching-record" iterator, NOT a
/// "play all records" loop. Each invocation either:
/// </para>
/// <list type="bullet">
/// <item><description><b>Hits the 0xFFFF terminator</b> → returns with CF=1 (matches asm <c>stc; ret</c> at <c>cs1:0x9F9C</c>).</description></item>
/// <item><description><b>Skips records</b> whose flag-byte AND context-byte gating says "not now" → loops to the next record.</description></item>
/// <item><description><b>Finds a matching record</b> whose inner-VM predicate returns non-zero (DX != 0) → delegates to the side-effects path at <c>cs1:0x9FD8</c> (still asm), which dispatches the verb, emits phrases, and eventually <c>clc; ret</c>s to the outer VM's caller with CF=0.</description></item>
/// </list>
/// <para>
/// The dispatch FRAME is fully ported here. The side-effects path at <c>cs1:0x9FD8</c>
/// stays as asm because it calls into ~10 unported engine helpers
/// (<c>cs1:0xA0F1, 0x1803, 0x3AF9, 0x91A0, 0xCF70, 0x88F1, 0x8944, 0x8B11, ...</c>).
/// When those are ported, the side-effects path can be migrated incrementally without
/// touching this file.
/// </para>
/// </remarks>
public partial class Overrides {
    /// <summary>
    /// Registers the outer dialogue VM dispatch-frame override with Spice86.
    /// </summary>
    public void DefineOuterVmCodeOverrides() {
        DefineFunction(cs1, 0x9F9E, OuterVmDispatchFrame_1000_9F9E_019F9E);
    }

    /// <summary>
    /// Override for cs1:0x9F9E — the outer dialogue VM dispatch frame.
    /// </summary>
    /// <remarks>
    /// Asm (entry through pre-side-effects, 9F9E..9FD7):
    /// <code>
    /// 9F9E: 89 36 7C 47      mov [0x477C], si       ; save script base
    /// 9FA2: E8 4E F5         call 0x94F3            ; per-script init (now C#)
    /// 9FA5: C7 06 BC 47 B0 A6  mov word [0x47BC], 0xA6B0
    /// 9FAB: 8B 04            mov ax, [si]           ; read record-word (flags + script_idx_lo)
    /// 9FAD: 3D FF FF         cmp ax, 0xFFFF
    /// 9FB0: 74 EA            jz 0x9F9C              ; FFFF terminator → stc; ret
    /// 9FB2: A8 80            test al, 0x80          ; bit 7 of flags
    /// 9FB4: 74 0A            jz 0x9FC0              ; bit 7 = 0 → unconditionally run
    /// 9FB6: A8 40            test al, 0x40
    /// 9FB8: 75 06            jnz 0x9FC0             ; bit 6 = 1 → unconditionally run
    /// 9FBA: 22 06 C2 47      and al, [0x47C2]
    /// 9FBE: 75 13            jnz 0x9FD3             ; (flags &amp; ctx_mask) != 0 → skip
    /// 9FC0: 56               push si
    /// 9FC1: 8A C4            mov al, ah             ; AL = byte[1] (script_idx_lo)
    /// 9FC3: 8A 64 02         mov ah, [si+0x2]       ; AH = byte[2] (verb_byte)
    /// 9FC6: D0 C4            rol ah, 1
    /// 9FC8: D0 C4            rol ah, 1
    /// 9FCA: 80 E4 03         and ah, 0x03           ; AH = top 2 bits of byte[2]
    /// 9FCD: E8 C6 03         call 0xA396            ; inner VM (now C#)
    /// 9FD0: 5E               pop si
    /// 9FD1: 75 05            jnz 0x9FD8             ; predicate true → side-effects (still asm)
    /// 9FD3: 83 C6 04         add si, 0x4            ; advance to next record
    /// 9FD6: EB D3             jmp 0x9FAB             ; loop
    /// </code>
    /// Pre-loop work calls the now-ported <c>cs1:0x94F3</c> directly as a C# method.
    /// The inner-VM call at <c>9FCD</c> likewise invokes the C# <c>cs1:0xA396</c>
    /// override directly; we mirror the asm's 2-byte stack push of <c>0x9FD0</c>
    /// (the return-address the asm's <c>call</c> would have written) for memdiff parity.
    /// </remarks>
    public System.Action OuterVmDispatchFrame_1000_9F9E_019F9E(int gotoAddress) {
        // 9F9E: mov [0x477C], si — save script base. Generated setter is byte-truncated;
        //       use indexer.
        UInt16[DS, 0x477C] = SI;

        // 9FA2: call 0x94F3 — per-script init. Both 9F9E and 94F3 are C# overrides;
        //       call the C# method directly. Mirror the 2-byte stack push of 9FA5
        //       (the asm's call return address) for memdiff parity.
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x9FA5;
        OuterVmHelperPerScriptInit_1000_94F3_0194F3(0);
        SP = (ushort)(SP + 2);

        // 9FA5: mov word [0x47BC], 0xA6B0
        UInt16[DS, 0x47BC] = 0xA6B0;

        // 9FAB: loop body
        while (true) {
            // mov ax, [si]
            ushort ax = UInt16[DS, SI];
            AX = ax;

            // cmp ax, 0xFFFF; jz 0x9F9C
            if (ax == 0xFFFF) {
                // 9F9C: stc; ret — set CF, ret. Inline the effect directly.
                CarryFlag = true;
                return NearRet();
            }

            byte al = (byte)(ax & 0xFF);
            byte ah = (byte)(ax >> 8);

            // test al, 0x80
            bool runUnconditionally = (al & 0x80) == 0;
            if (!runUnconditionally) {
                // test al, 0x40
                if ((al & 0x40) != 0) {
                    runUnconditionally = true;
                } else {
                    // and al, [0x47C2]; jnz 0x9FD3 (skip)
                    byte ctx = (byte)globalsOnDs.Get1138_47C2_Byte8();
                    if ((al & ctx) != 0) {
                        // skip path
                        SI = (ushort)(SI + 4);
                        continue;
                    }
                    // else fall through to run
                }
            }

            // 9FC0: RUN path — push si; compute action_index; call A396
            SP = (ushort)(SP - 2);
            UInt16[SS, SP] = SI;

            // 9FC1: mov al, ah — AL becomes byte[1] (script_idx_lo)
            AL = ah;
            // 9FC3: mov ah, [si+0x2] — AH becomes byte[2] (verb_byte)
            byte b2 = UInt8[DS, (ushort)(SI + 2)];
            // 9FC6..9FCA: rol ah,1; rol ah,1; and ah,0x03 — extract top 2 bits of b2 into AH
            AH = (byte)((b2 >> 6) & 0x03);

            // 9FCD: call 0xA396 — inner VM. Both 9F9E and A396 are C# overrides; call
            //       directly. Mirror the asm's 2-byte stack push of 9FD0 (return addr)
            //       for memdiff parity.
            SP = (ushort)(SP - 2);
            UInt16[SS, SP] = 0x9FD0;
            ScriptVmDispatcher_1000_A396_01A396(0);
            SP = (ushort)(SP + 2);

            // 9FD0: pop si
            SI = UInt16[SS, SP];
            SP = (ushort)(SP + 2);

            // 9FD1: jnz 0x9FD8 — predicate non-zero (DX != 0 from A396) → side-effects.
            // A396's tail `or dx, dx` sets ZF; ScriptVmDispatcher mirrors that via Alu16.Or.
            if (!ZeroFlag) {
                // Delegate to the asm side-effects path at 0x9FD8. The asm there does
                // its work and eventually does `clc; ret` at cs1:0xA0F0..A0F1, returning
                // to our outer VM caller with CF=0.
                return NearJump(0x9FD8);
            }

            // 9FD3..9FD6: add si, 0x4; jmp 0x9FAB — skip-and-loop
            SI = (ushort)(SI + 4);
            // continue loop
        }
    }
}
