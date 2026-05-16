namespace Cryogenic.Overrides;

/// <summary>
/// Partial class containing scene-chain lifecycle helpers (Phase 28 — Tech/36, 38, 39, 40, 41).
/// </summary>
/// <remarks>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly. These are leaves of the
/// chain-wakeup / chain-decommission / chain-merge mechanism that drives the engine's
/// dialogue-record activation pool.
/// </remarks>
public partial class Overrides {
    /// <summary>
    /// Registers scene-chain function overrides with Spice86.
    /// </summary>
    public void DefineSceneChainCodeOverrides() {
        DefineFunction(cs1, 0x6B25, InitChainTimerSlot_1000_6B25_016B25);
        DefineFunction(cs1, 0x1AC5, GameTimeShiftRight4_1000_1AC5_011AC5);
        DefineFunction(cs1, 0x6F56, IncrementRecordCounters_1000_6F56_016F56);
        DefineFunction(cs1, 0x6F78, UpdateCounter0029AndTailKick_1000_6F78_016F78);
        DefineFunction(cs1, 0x6906, FindRecordByIndex08AA_1000_6906_016906);
        DefineFunction(cs1, 0x858C, ChainTraverseAndPatch_1000_858C_01858C);
        DefineFunction(cs1, 0x66B1, ChainDecommission_1000_66B1_0166B1);
        DefineFunction(cs1, 0x66BE, ChainDecommissionContinuation_1000_66BE_0166BE);
        DefineFunction(cs1, 0x1EA1, ChainCountActiveCallback_1000_1EA1_011EA1);
        DefineFunction(cs1, 0x66CE, ChainWakeup_1000_66CE_0166CE);
        DefineFunction(cs1, 0x66ED, ChainWakeupPostWarnContinuation_1000_66ED_0166ED);
        DefineFunction(cs1, 0x6D5F, ChainMergeBestCallback_1000_6D5F_016D5F);
        DefineFunction(cs1, 0x6D19, ChainMerge_1000_6D19_016D19);
        DefineFunction(cs1, 0x6D3D, ChainMergeContinuation_1000_6D3D_016D3D);
        DefineFunction(cs1, 0x40AE, ReverseLookupRecordIndex_1000_40AE_0140AE);
        DefineFunction(cs1, 0x11CB, ChainElectorInit_1000_11CB_0111CB);
        DefineFunction(cs1, 0x1E24, ChainScanForFlag0400_1000_1E24_011E24);
        DefineFunction(cs1, 0x1EF3, ActiveRecordElector_1000_1EF3_011EF3);
        DefineFunction(cs1, 0x121F, HouseStateAdvance_1000_121F_01121F);
        DefineFunction(cs1, 0x1230, HouseStateAdvanceContinuation_1000_1230_011230);
    }

    /// <summary>
    /// Override for cs1:0x6B25 — initializes a chain-record timer slot at <c>[si+0x0A..0x0F]</c>
    /// from the current game elapsed time.
    /// </summary>
    /// <remarks>
    /// Asm (15 bytes):
    /// <code>
    /// 6B25: A1 02 00       mov ax, [0x0002]      ; game elapsed time
    /// 6B28: 89 44 0A       mov [si+0x0A], ax
    /// 6B2B: 33 C0          xor ax, ax
    /// 6B2D: 89 44 0C       mov [si+0x0C], ax
    /// 6B30: 89 44 0E       mov [si+0x0E], ax
    /// 6B33: C3             ret
    /// </code>
    /// Pure leaf. Stamps the wakeup time into <c>[si+0x0A]</c> and clears the two adjacent
    /// counter slots at <c>[si+0x0C]</c> and <c>[si+0x0E]</c>.
    /// </remarks>
    public System.Action InitChainTimerSlot_1000_6B25_016B25(int gotoAddress) {
        ushort gameTime = globalsOnDs.Get1138_0002_Word16_GameElapsedTime();
        AX = gameTime;
        UInt16[DS, (ushort)(SI + 0x0A)] = gameTime;
        AX = 0;
        UInt16[DS, (ushort)(SI + 0x0C)] = 0;
        UInt16[DS, (ushort)(SI + 0x0E)] = 0;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x1AC5 — returns the game elapsed time at <c>ds[0x0002]</c>
    /// shifted right by 4 (divided by 16).
    /// </summary>
    /// <remarks>
    /// Asm (12 bytes):
    /// <code>
    /// 1AC5: A1 02 00      mov ax, [0x0002]
    /// 1AC8: D1 E8         shr ax, 1
    /// 1ACA: D1 E8         shr ax, 1
    /// 1ACC: D1 E8         shr ax, 1
    /// 1ACE: D1 E8         shr ax, 1
    /// 1AD0: C3            ret
    /// </code>
    /// Pure leaf. Coarse-grained game-time tick (game_time / 16). Caller compares against
    /// chain-record timestamps to decide wakeup eligibility.
    /// </remarks>
    public System.Action GameTimeShiftRight4_1000_1AC5_011AC5(int gotoAddress) {
        ushort gameTime = globalsOnDs.Get1138_0002_Word16_GameElapsedTime();
        AX = (ushort)(gameTime >> 4);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x6F56 — walks the record table at <c>ds:0x08AA</c> (stride 0x1B,
    /// up to address 0x0FBB), and for each unsuppressed record bumps the per-record
    /// counter at <c>[+0x15]</c> by AL (capped at 0x64).
    /// </summary>
    /// <remarks>
    /// Asm (35 bytes, 6F56..6F77):
    /// <code>
    /// 6F56: 56                push si
    /// 6F57: BE AA 08          mov si, 0x08AA
    /// 6F5A: F6 44 03 A0       test byte [si+0x03], 0xA0     ; suppress mask
    /// 6F5E: 75 0D              jnz 6F6D                      ; skip this record
    /// 6F60: 00 44 15           add [si+0x15], al             ; bump counter
    /// 6F63: 80 7C 15 64        cmp byte [si+0x15], 0x64
    /// 6F67: 76 04              jbe 6F6D
    /// 6F69: C6 44 15 64        mov byte [si+0x15], 0x64     ; cap at 100
    /// 6F6D: 83 C6 1B           add si, 0x1B                  ; advance by 27 bytes
    /// 6F70: 81 FE BB 0F        cmp si, 0x0FBB
    /// 6F74: 72 E4              jc  6F5A                      ; loop
    /// 6F76: 5E                 pop si
    /// 6F77: C3                 ret
    /// </code>
    /// Pure leaf. Used by the chain wakeup / counter-bumping mechanism.
    /// </remarks>
    public System.Action IncrementRecordCounters_1000_6F56_016F56(int gotoAddress) {
        // push si
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;

        ushort si = 0x08AA;
        SI = si;
        byte al = AL;
        while (si < 0x0FBB) {
            // test byte [si+3], 0xA0
            byte suppress = UInt8[DS, (ushort)(si + 0x03)];
            if ((suppress & 0xA0) == 0) {
                // not suppressed — bump counter at [si+0x15] (cap at 0x64)
                byte counter = UInt8[DS, (ushort)(si + 0x15)];
                byte updated = (byte)(counter + al);
                UInt8[DS, (ushort)(si + 0x15)] = updated;
                if (updated > 0x64) {
                    // cap
                    UInt8[DS, (ushort)(si + 0x15)] = 0x64;
                }
            }
            si = (ushort)(si + 0x1B);
            SI = si;
        }
        // pop si
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x6F78 — updates the rate-limited counter at <c>ds[0x0029]</c>
    /// and, when the residual rate fraction is non-zero, tail-jumps into
    /// <see cref="IncrementRecordCounters_1000_6F56_016F56"/>.
    /// </summary>
    /// <remarks>
    /// Asm (27 bytes, 6F78..6F92):
    /// <code>
    /// 6F78: 8A 26 29 00       mov ah, [0x0029]
    /// 6F7C: 02 C4              add al, ah
    /// 6F7E: 3C C8              cmp al, 0xC8
    /// 6F80: 76 02              jbe 6F84
    /// 6F82: B0 C8              mov al, 0xC8                ; cap at 200
    /// 6F84: A2 29 00            mov [0x0029], al
    /// 6F87: 25 FC FC            and ax, 0xFCFC
    /// 6F8A: 2A C4               sub al, ah
    /// 6F8C: D0 E8               shr al, 1
    /// 6F8E: D0 E8               shr al, 1
    /// 6F90: 75 C4               jnz 6F56                    ; tail-jump to counter-bumper
    /// 6F92: C3                  ret
    /// </code>
    /// Updates the per-tick accumulator <c>[0x0029] = min([0x0029] + AL, 0xC8)</c>.
    /// Then computes the residual whole-quartets to add to each record's counter, and
    /// if non-zero, transfers control to <c>cs1:0x6F56</c> (now C#) — that function's
    /// ret returns to <em>our</em> caller, completing the original call cleanly.
    /// </remarks>
    public System.Action UpdateCounter0029AndTailKick_1000_6F78_016F78(int gotoAddress) {
        // mov ah, [0x0029]
        byte ah = (byte)globalsOnDs.Get1138_0029_Byte8();
        AH = ah;
        // add al, ah
        byte sum = (byte)(AL + ah);
        AL = sum;
        // cmp al, 0xC8; jbe 6F84; mov al, 0xC8 (cap)
        if (sum > 0xC8) {
            sum = 0xC8;
            AL = sum;
        }
        // mov [0x0029], al
        UInt8[DS, 0x0029] = sum;
        // and ax, 0xFCFC
        AX = (ushort)(AX & 0xFCFC);
        // sub al, ah  (AH is the AND-masked ah; AL was already AND-masked sum)
        // After `and ax, 0xFCFC`, both AL and AH have their low 2 bits cleared.
        byte alMasked = (byte)(sum & 0xFC);
        byte ahMasked = (byte)(ah & 0xFC);
        byte residual = (byte)(alMasked - ahMasked);
        AL = residual;
        // shr al, 1; shr al, 1
        residual = (byte)(residual >> 2);
        AL = residual;
        // jnz 6F56 — asm tail-jumps to cs1:0x6F56 (no push). For C# direct-callers to
        // work correctly (they discard returned NearJump Actions), we inline the call:
        // 0x6F56 is a C# override that always returns NearRet, so calling it as a method
        // and discarding the Action is equivalent to the asm tail-jump + ret pattern
        // (the outer caller's return address gets popped by our own NearRet below).
        if (residual != 0) {
            IncrementRecordCounters_1000_6F56_016F56(0);
        }
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x6906 — computes the address of record <c>(AL - 1)</c> in the
    /// 27-byte-stride record table at <c>ds:0x08AA</c>; returns the address in <c>SI</c>
    /// with flags set against the record's flag-byte <c>[si+0x03]</c> vs <c>0x80</c>.
    /// </summary>
    /// <remarks>
    /// Asm (17 bytes):
    /// <code>
    /// 6906: 8B F0           mov si, ax            ; (clobber-protect AX in SI)
    /// 6908: FE C8           dec al
    /// 690A: B4 1B           mov ah, 0x1B
    /// 690C: F6 E4           mul ah                 ; AX = AL * 0x1B
    /// 690E: 05 AA 08        add ax, 0x08AA
    /// 6911: 96              xchg ax, si           ; SI = record addr; AX = original
    /// 6912: 80 7C 03 80     cmp byte [si+0x03], 0x80
    /// 6916: C3              ret
    /// </code>
    /// Pure leaf. Flags at exit reflect the cmp: CF=1 if record is "below 0x80" (active),
    /// ZF=1 if exactly 0x80. Used by chain decommission and other record-lookup callers.
    /// </remarks>
    public System.Action FindRecordByIndex08AA_1000_6906_016906(int gotoAddress) {
        // mov si, ax — preserve caller's AX in SI temporarily
        ushort origAx = AX;
        SI = origAx;
        // dec al
        byte al = (byte)(AL - 1);
        AL = al;
        // mov ah, 0x1B; mul ah → AX = AL * 0x1B
        AH = 0x1B;
        ushort product = (ushort)((ushort)al * 0x1B);
        AX = product;
        // add ax, 0x08AA
        AX = (ushort)(AX + 0x08AA);
        // xchg ax, si — SI = record addr; AX = original
        ushort tmp = AX;
        AX = SI;
        SI = tmp;
        // cmp byte [si+0x03], 0x80 — set flags
        byte flagByte = UInt8[DS, (ushort)(SI + 0x03)];
        // Set CF and ZF as the asm's cmp would
        // (cmp = subtract, flags from result; CF=1 if flagByte < 0x80, ZF=1 if equal)
        CarryFlag = flagByte < 0x80;
        ZeroFlag = flagByte == 0x80;
        SignFlag = ((byte)(flagByte - 0x80) & 0x80) != 0;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x66B1 — chain decommission entry (Tech/39).
    /// </summary>
    /// <remarks>
    /// Asm (29 bytes, 66B1..66CD):
    /// <code>
    /// 66B1: E8 D8 1E         call 0x858C            ; chain-traverse-and-patch (C#)
    /// 66B4: 56                push si
    /// 66B5: 57                push di
    /// 66B6: E8 5E 02          call 0x6917            ; find-record-by-key (C#)
    /// 66B9: 75 03              jnz 66BE               ; no match → skip 0xC58A
    /// 66BB: E8 CC 5E           call 0xC58A            ; (still asm; verb-13 graphics)
    /// 66BE: 5F                 pop di
    /// 66BF: 5E                 pop si
    /// 66C0: C7 44 04 BC 0F    mov word [si+0x04], 0x0FBC
    /// 66C5: C6 44 03 A0        mov byte [si+0x03], 0xA0
    /// 66C9: C6 44 1A 00        mov byte [si+0x1A], 0
    /// 66CD: C3                 ret
    /// </code>
    /// Decommissions a chain record: marks it dormant via the flag byte at <c>[si+0x03] =
    /// 0xA0</c>, sets the follow-link to <c>0x0FBC</c> (free-list sentinel), and clears the
    /// counter at <c>[si+0x1A]</c>. When the record was previously linked into the active
    /// pool (signaled by <c>0x6917</c>'s ZF=1), invokes <c>0xC58A</c> to unhook UI state.
    /// </remarks>
    public System.Action ChainDecommission_1000_66B1_0166B1(int gotoAddress) {
        // call 0x858C — direct C# invocation; mirror asm push of 0x66B4
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x66B4;
        ChainTraverseAndPatch_1000_858C_01858C(0);
        SP = (ushort)(SP + 2);

        // push si
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;
        // push di
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = DI;

        // call 0x6917 — direct C# invocation; mirror asm push of 0x66B9
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x66B9;
        FindRecordByKey3CAF_1000_6917_016917(0);
        SP = (ushort)(SP + 2);

        if (!ZeroFlag) {
            // jnz 66BE — no match path: inline the cleanup
            return ChainDecommissionContinuation_1000_66BE_0166BE(0);
        }
        // ZF=1 → call 0xC58A (still asm) then continue at 66BE.
        // Chain-port: push 66BE, NearJump.
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x66BE;
        return NearJump(0xC58A);
    }

    /// <summary>
    /// Override for cs1:0x66BE — continuation of <see cref="ChainDecommission_1000_66B1_0166B1"/>
    /// after the asm-side <c>0xC58A</c> call returns.
    /// </summary>
    /// <remarks>
    /// Asm: <c>5F</c> pop di; <c>5E</c> pop si; <c>C7 44 04 BC 0F</c> writes; <c>C6 44 03 A0</c>;
    /// <c>C6 44 1A 00</c>; <c>C3</c> ret. Also invoked directly when 0x6917 reports
    /// no-match (the asm's jnz over the 0xC58A call).
    /// </remarks>
    /// <summary>
    /// Override for cs1:0x66CE — chain wakeup (Tech/38).
    /// </summary>
    /// <remarks>
    /// Asm (71 bytes, 66CE..6714):
    /// <code>
    /// 66CE: F6 44 03 80     test byte [si+0x03], 0x80
    /// 66D2: 74 40            jz  6714                      ; not dormant → ret
    /// 66D4: F6 44 10 80     test byte [si+0x10], 0x80
    /// 66D8: 75 3A            jnz 6714                      ; locked → ret
    /// 66DA: FE 06 28 00      inc byte [0x0028]            ; wakeup counter
    /// 66DE: A0 28 00         mov al, [0x0028]
    /// 66E1: 3A 06 78 11     cmp al, [0x1178]
    /// 66E5: 72 07            jc  66EE                      ; below threshold → skip warn
    /// 66E7: 56               push si
    /// 66E8: B0 4C             mov al, 0x4C
    /// 66EA: E8 32 AB         call 0x121F                   ; (asm) — fire warn
    /// 66ED: 5E               pop si                        ; (continuation entry)
    /// 66EE: B0 01             mov al, 0x01
    /// 66F0: E8 85 08         call 0x6F78                   ; (C# now)
    /// 66F3: 80 64 03 20     and byte [si+0x03], 0x20      ; ★ KILL bit 0x80 + most others
    /// 66F7: 80 4C 03 02     or  byte [si+0x03], 0x02      ; ★ SET active-flag
    /// 66FB: E8 27 04         call 0x6B25                   ; (C# now)
    /// 66FE: E8 C4 B3         call 0x1AC5                   ; (C# now)
    /// 6701: 88 44 14        mov [si+0x14], al
    /// 6704: 8B 7C 04         mov di, [si+0x04]
    /// 6707: 80 7D 0B 00     cmp byte [di+0x0B], 0x00
    /// 670B: 75 07            jnz 6714                      ; follow-link already active
    /// 670D: C6 45 0B 02     mov byte [di+0x0B], 0x02
    /// 6711: E8 3A FD         call 0x644E                   ; (asm — has SMC)
    /// 6714: C3                ret
    /// </code>
    /// Warn path chain-ports through asm <c>cs1:0x121F</c> (via continuation
    /// <see cref="ChainWakeupPostWarnContinuation_1000_66ED_0166ED"/>). The trailing
    /// <c>call 0x644E</c> tail-chain-ports through asm; <c>0x644E</c>'s ret pops the
    /// synthetic <c>0x6714</c> we push, landing on the bare-ret byte at 0x6714 which
    /// returns to the outer caller.
    /// </remarks>
    public System.Action ChainWakeup_1000_66CE_0166CE(int gotoAddress) {
        byte v3 = UInt8[DS, (ushort)(SI + 0x03)];
        if ((v3 & 0x80) == 0) {
            return NearRet();
        }
        byte v10 = UInt8[DS, (ushort)(SI + 0x10)];
        if ((v10 & 0x80) != 0) {
            return NearRet();
        }
        // inc [0x0028]; mov al, [0x0028]
        byte counter = (byte)((byte)globalsOnDs.Get1138_0028_Byte8() + 1);
        globalsOnDs.Set1138_0028_Byte8(counter);
        AL = counter;
        // cmp al, [0x1178]
        byte threshold = UInt8[DS, 0x1178];
        if (counter < threshold) {
            // jc 66EE — skip warn path; jump straight into the 66EE body (no pop si
            // because the warn path's push si was never executed in this branch).
            return ChainWakeupAfter66EE();
        }
        // Warn path: push si; mov al, 0x4C; call 0x121F (chain-port)
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;
        AL = 0x4C;
        // Push synthetic continuation 0x66ED
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x66ED;
        return NearJump(0x121F);
    }

    /// <summary>
    /// Override for cs1:0x66ED — continuation of <see cref="ChainWakeup_1000_66CE_0166CE"/>
    /// after the asm-side <c>cs1:0x121F</c> warn helper returns.
    /// </summary>
    /// <remarks>
    /// Only fires when 0x121F's asm rets (popping our synthetic 0x66ED off the stack).
    /// The skip-warn fast path in <c>0x66CE</c> calls <c>ChainWakeupAfter66EE</c>
    /// directly, bypassing this override and the <c>pop si</c> (since the skip-warn
    /// branch never executed the matching <c>push si</c> at 0x66E7).
    /// </remarks>
    public System.Action ChainWakeupPostWarnContinuation_1000_66ED_0166ED(int gotoAddress) {
        // pop si — restore the SI saved at 66E7 before the call to 0x121F.
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return ChainWakeupAfter66EE();
    }

    /// <summary>
    /// Helper for the post-66EE body of <c>cs1:0x66CE</c> chain wakeup.
    /// Both the skip-warn fast path and the chain-port continuation feed into here.
    /// </summary>
    private System.Action ChainWakeupAfter66EE() {
        AL = 0x01;
        // call 0x6F78 (C#) — mirror push of 0x66F3
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x66F3;
        UpdateCounter0029AndTailKick_1000_6F78_016F78(0);
        SP = (ushort)(SP + 2);

        // and byte [si+0x03], 0x20
        byte v3 = UInt8[DS, (ushort)(SI + 0x03)];
        v3 = (byte)(v3 & 0x20);
        UInt8[DS, (ushort)(SI + 0x03)] = v3;
        // or byte [si+0x03], 0x02
        v3 = (byte)(v3 | 0x02);
        UInt8[DS, (ushort)(SI + 0x03)] = v3;

        // call 0x6B25 (C#) — mirror push of 0x66FE
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x66FE;
        InitChainTimerSlot_1000_6B25_016B25(0);
        SP = (ushort)(SP + 2);

        // call 0x1AC5 (C#) — mirror push of 0x6701
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x6701;
        GameTimeShiftRight4_1000_1AC5_011AC5(0);
        SP = (ushort)(SP + 2);

        // mov [si+0x14], al
        UInt8[DS, (ushort)(SI + 0x14)] = AL;
        // mov di, [si+0x04]
        DI = UInt16[DS, (ushort)(SI + 0x04)];
        // cmp byte [di+0x0B], 0
        byte vB = UInt8[DS, (ushort)(DI + 0x0B)];
        if (vB != 0) {
            return NearRet();  // jnz 6714 → just ret
        }
        // mov byte [di+0x0B], 0x02
        UInt8[DS, (ushort)(DI + 0x0B)] = 0x02;
        // call 0x644E (asm; chain-port — synthetic push of 0x6714, which is bare `C3`)
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x6714;
        return NearJump(0x644E);
    }

    /// <summary>
    /// Override for cs1:0x121F — house-state advance + dispatch. Verb 12 tail-jumps here
    /// (cs1:0xA241 in <c>DialogueVerb12NextHouse</c>); also reachable from <c>cs1:0x66CE</c>'s
    /// warn path.
    /// </summary>
    /// <remarks>
    /// Asm (entry through pre-call, 121F..122F):
    /// <code>
    /// 121F: 3A 06 2A 00         cmp al, [0x002A]
    /// 1223: 76 1D                jbe 1242                    ; ret unchanged
    /// 1225: A2 2A 00              mov [0x002A], al            ; bump house state
    /// 1228: C6 06 FF 00 00       mov byte [0x00FF], 0
    /// 122D: E8 4A 9F              call 0xB17A                  ; (C#; chain-ports onward)
    /// 1230: ...                   (continuation)
    /// </code>
    /// Since <c>cs1:0xB17A</c> may itself return <c>NearJump(0x96B5)</c> when invoked via
    /// Spice86 dispatch, we use the chain-port pattern (synth push + NearJump) rather
    /// than a direct C# call — this lets the asm-chain through 0x96B5 → 0x9F9E and back
    /// run correctly.
    /// </remarks>
    public Action HouseStateAdvance_1000_121F_01121F(int gotoAddress) {
        byte v2A = (byte)globalsOnDs.Get1138_002A_Byte8();
        if (AL <= v2A) {
            return NearRet();
        }
        globalsOnDs.Set1138_002A_Byte8(AL);
        UInt8[DS, 0x00FF] = 0;
        // call 0xB17A — chain-port: synth push of 0x1230, NearJump
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x1230;
        return NearJump(0xB17A);
    }

    /// <summary>
    /// Override for cs1:0x1230 — continuation of <see cref="HouseStateAdvance_1000_121F_01121F"/>
    /// after the asm-chain through <c>0xB17A → 0x96B5 → 0x9F9E</c> returns.
    /// </summary>
    /// <remarks>
    /// Asm (1230..1242):
    /// <code>
    /// 1230: 8A 1E 2A 00         mov bl, [0x002A]
    /// 1234: 80 FB 6C             cmp bl, 0x6C
    /// 1237: 77 09                ja  1242                    ; ret
    /// 1239: 32 FF                xor bh, bh
    /// 123B: D1 EB                shr bx, 1
    /// 123D: 2E FF 97 E7 11       call far [cs:bx + 0x11E7]
    /// 1242: C3                   ret
    /// </code>
    /// The <c>call far [cs:bx + 0x11E7]</c> is an indirect far call through a function-pointer
    /// table at <c>cs1:0x11E7</c> (54 4-byte entries indexed by BL/2). We simulate by reading
    /// the target seg:off, pushing <c>cs1:0x1242</c> (the asm bare-near-ret byte) as the
    /// far-return address, and issuing <c>FarJump</c>. When the target retfs, control lands
    /// on the asm <c>C3</c> at <c>0x1242</c> which pops the outer caller's near return.
    /// </remarks>
    public Action HouseStateAdvanceContinuation_1000_1230_011230(int gotoAddress) {
        byte bl = (byte)globalsOnDs.Get1138_002A_Byte8();
        BL = bl;
        if (bl > 0x6C) {
            return NearRet();
        }
        BH = 0;
        ushort bx = (ushort)(BX >> 1);
        BX = bx;
        // call far [cs:bx + 0x11E7]
        uint tblAddr = (uint)((cs1 << 4) + bx + 0x11E7);
        ushort targetOff = Memory.UInt16[tblAddr];
        ushort targetSeg = Memory.UInt16[tblAddr + 2];
        // Far call: push CS, push IP, FarJump.
        // The push CS then push IP order matches `call far [m]` semantics; retf pops IP then CS.
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x1242;
        return FarJump(targetSeg, targetOff);
    }

    /// <summary>
    /// Override for cs1:0x40AE — reverse-lookup: given a record address in <c>DI</c>,
    /// compute its 1-based record index in <c>BH</c>.
    /// </summary>
    /// <remarks>
    /// Asm (21 bytes):
    /// <code>
    /// 40AE: 8B C7              mov ax, di
    /// 40B0: 2D 00 01           sub ax, 0x0100
    /// 40B3: B3 1C              mov bl, 0x1C
    /// 40B5: F6 F3              div bl                  ; AL = AX/0x1C, AH = AX%0x1C
    /// 40B7: 8A F8              mov bh, al
    /// 40B9: FE C7              inc bh                  ; 1-based
    /// 40BB: B3 80              mov bl, 0x80
    /// 40BD: 8A 75 08           mov dh, [di+0x08]
    /// 40C0: B2 01              mov dl, 0x01
    /// 40C2: C3                  ret
    /// </code>
    /// Pure leaf. The record table starts at <c>ds:0x0100</c> with 28-byte stride.
    /// </remarks>
    public Action ReverseLookupRecordIndex_1000_40AE_0140AE(int gotoAddress) {
        ushort axIn = (ushort)(DI - 0x0100);
        AX = axIn;
        BL = 0x1C;
        // div bl: AL = AX/BL, AH = AX%BL
        byte q = (byte)(axIn / 0x1C);
        byte r = (byte)(axIn % 0x1C);
        AL = q;
        AH = r;
        BH = q;
        BH = (byte)(BH + 1);
        BL = 0x80;
        DH = UInt8[DS, (ushort)(DI + 0x08)];
        DL = 0x01;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x11CB — chain elector init. Resets scratch state, sets DI to the
    /// first record (<c>0x011C</c>), invokes <see cref="ReverseLookupRecordIndex_1000_40AE_0140AE"/>,
    /// then publishes the result at <c>ds[0x1048]</c> / <c>ds[0x104A]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (27 bytes):
    /// <code>
    /// 11CB: C6 06 FF 00 00    mov byte [0x00FF], 0
    /// 11D0: C6 06 2A 00 60    mov byte [0x002A], 0x60
    /// 11D5: BF 1C 01           mov di, 0x011C
    /// 11D8: E8 D3 2E           call 0x40AE
    /// 11DB: B2 02              mov dl, 0x02
    /// 11DD: 89 16 48 10        mov [0x1048], dx
    /// 11E1: 89 1E 4A 10        mov [0x104A], bx
    /// 11E5: C3                  ret
    /// </code>
    /// </remarks>
    public Action ChainElectorInit_1000_11CB_0111CB(int gotoAddress) {
        UInt8[DS, 0x00FF] = 0;
        UInt8[DS, 0x002A] = 0x60;
        DI = 0x011C;
        // call 0x40AE — direct C# invocation; mirror asm push of 0x11DB
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x11DB;
        ReverseLookupRecordIndex_1000_40AE_0140AE(0);
        SP = (ushort)(SP + 2);
        DL = 0x02;
        UInt16[DS, 0x1048] = DX;
        UInt16[DS, 0x104A] = BX;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x1E24 — scans the chain at <c>[di+0x09]</c> looking for any link
    /// with bit 10 (<c>0x0400</c>) set in <c>[si+0x12]</c>. Returns CF=1 if found, CF=0 if
    /// the chain ended.
    /// </summary>
    /// <remarks>
    /// Asm (32 bytes, 1E24..1E42):
    /// <code>
    /// 1E24: 8A 45 09           mov al, [di+0x09]
    /// 1E27: 0A C0              or  al, al
    /// 1E29: 74 13              jz  1E3E                 ; chain empty → CF=0
    /// 1E2B: 56                  push si
    /// 1E2C: E8 D7 4A            call 0x6906             ; (C#) AL → SI = record addr
    /// 1E2F: F7 44 12 00 04      test word [si+0x12], 0x0400
    /// 1E34: 75 0A               jnz 1E40                ; found → CF=1
    /// 1E36: 8A 44 01            mov al, [si+0x01]
    /// 1E39: 0A C0               or  al, al
    /// 1E3B: 75 EF               jnz 1E2C                ; next link
    /// 1E3D: 5E                  pop si
    /// 1E3E: F8                  clc
    /// 1E3F: C3                  ret
    /// 1E40: 5E                  pop si
    /// 1E41: F9                  stc
    /// 1E42: C3                  ret
    /// </code>
    /// Direct C# call to <c>0x6906</c> (the find-record-by-index leaf). Mirrors asm push
    /// of <c>0x1E2F</c> for memdiff parity.
    /// </remarks>
    public Action ChainScanForFlag0400_1000_1E24_011E24(int gotoAddress) {
        byte al = UInt8[DS, (ushort)(DI + 0x09)];
        AL = al;
        if (al == 0) {
            // jz 1E3E → clc; ret
            CarryFlag = false;
            return NearRet();
        }
        // push si
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;

        while (true) {
            // call 0x6906 — direct C# invocation; mirror push of 0x1E2F
            SP = (ushort)(SP - 2);
            UInt16[SS, SP] = 0x1E2F;
            FindRecordByIndex08AA_1000_6906_016906(0);
            SP = (ushort)(SP + 2);
            // test word [si+0x12], 0x0400
            ushort v12 = UInt16[DS, (ushort)(SI + 0x12)];
            if ((v12 & 0x0400) != 0) {
                // jnz 1E40 — pop si; stc; ret
                SI = UInt16[SS, SP];
                SP = (ushort)(SP + 2);
                CarryFlag = true;
                return NearRet();
            }
            // mov al, [si+0x01]
            al = UInt8[DS, (ushort)(SI + 0x01)];
            AL = al;
            if (al == 0) {
                break;
            }
            // jnz 1E2C — loop
        }
        // pop si; clc; ret
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        CarryFlag = false;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x1EF3 — active-record elector (Tech/37). Walks the chain-record
    /// table at <c>ds:0x0100</c>, calling <see cref="ChainScanForFlag0400_1000_1E24_011E24"/>
    /// on each record to find the first one with the active flag set; stores the winner's
    /// pointer at <c>ds[0x11DB]</c>; if none, calls <see cref="ChainElectorInit_1000_11CB_0111CB"/>
    /// to reset elector state.
    /// </summary>
    /// <remarks>
    /// Asm (32 bytes, 1EF3..1F12):
    /// <code>
    /// 1EF3: BF 00 01           mov di, 0x0100
    /// 1EF6: E8 2B FF           call 0x1E24                ; (C#)
    /// 1EF9: 72 0A               jc  1F05                  ; CF=1 → found
    /// 1EFB: 83 C7 1C            add di, 0x1C
    /// 1EFE: 80 3D FF            cmp byte [di], 0xFF
    /// 1F01: 75 F3               jnz 1EF6                  ; loop
    /// 1F03: 33 FF               xor di, di
    /// 1F05: 89 3E DB 11         mov [0x11DB], di
    /// 1F09: 0B FF               or  di, di
    /// 1F0B: 75 03               jnz 1F10
    /// 1F0D: E8 BB F2            call 0x11CB                ; (C#)
    /// 1F10: 5E                  pop si
    /// 1F11: 59                  pop cx
    /// 1F12: C3                  ret
    /// </code>
    /// <para>
    /// <b>Unusual calling convention:</b> the trailing <c>pop si; pop cx</c> consumes
    /// two stack slots that the function itself never pushed — callers of <c>0x1EF3</c>
    /// must <c>push cx; push si</c> (in that order) before calling. The C# port mirrors
    /// the asm exactly; misuse manifests as a stack underflow / wrong-DS state.
    /// </para>
    /// </remarks>
    public Action ActiveRecordElector_1000_1EF3_011EF3(int gotoAddress) {
        DI = 0x0100;
        // Loop: scan records at stride 0x1C until either 0x1E24 returns CF=1 (found) or
        // we hit the 0xFF terminator at [di].
        while (true) {
            // call 0x1E24 — C# direct; mirror push of 0x1EF9
            SP = (ushort)(SP - 2);
            UInt16[SS, SP] = 0x1EF9;
            ChainScanForFlag0400_1000_1E24_011E24(0);
            SP = (ushort)(SP + 2);
            if (CarryFlag) {
                // jc 1F05 — found
                break;
            }
            // add di, 0x1C
            DI = (ushort)(DI + 0x1C);
            // cmp byte [di], 0xFF; jnz 1EF6 (loop)
            byte di0 = UInt8[DS, DI];
            if (di0 == 0xFF) {
                // xor di, di — no match
                DI = 0;
                break;
            }
        }

        // 1F05: mov [0x11DB], di
        UInt16[DS, 0x11DB] = DI;
        // or di, di; jnz 1F10
        if (DI == 0) {
            // call 0x11CB — C# direct; mirror push of 0x1F10
            SP = (ushort)(SP - 2);
            UInt16[SS, SP] = 0x1F10;
            ChainElectorInit_1000_11CB_0111CB(0);
            SP = (ushort)(SP + 2);
        }
        // pop si; pop cx — UNUSUAL: consumes caller-pushed cx and si
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        CX = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x6D5F — chain-merge best-candidate callback, invoked by
    /// <c>cs1:0x661D</c> via <c>call bp</c> from <c>cs1:0x6D19</c>.
    /// </summary>
    /// <remarks>
    /// Asm (28 bytes):
    /// <code>
    /// 6D5F: F6 44 03 A0         test byte [si+0x03], 0xA0
    /// 6D63: 75 15                jnz 6D7A
    /// 6D65: 81 FE E0 08          cmp si, 0x08E0
    /// 6D69: 74 0F                jz  6D7A
    /// 6D6B: 3B F2                cmp si, dx
    /// 6D6D: 74 0B                jz  6D7A
    /// 6D6F: 8A 44 1A             mov al, [si+0x1A]
    /// 6D72: 3A C8                cmp al, cl
    /// 6D74: 72 04                jc  6D7A
    /// 6D76: 8B DE                mov bx, si
    /// 6D78: 8A C8                mov cl, al
    /// 6D7A: C3                   ret
    /// </code>
    /// Pure leaf. Selects the chain record with the highest priority (at <c>[+0x1A]</c>)
    /// that's NOT flag-locked, NOT the sentinel <c>0x08E0</c>, and NOT the original SI.
    /// </remarks>
    public System.Action ChainMergeBestCallback_1000_6D5F_016D5F(int gotoAddress) {
        byte v3 = UInt8[DS, (ushort)(SI + 0x03)];
        if ((v3 & 0xA0) != 0) {
            return NearRet();
        }
        if (SI == 0x08E0) {
            return NearRet();
        }
        if (SI == DX) {
            return NearRet();
        }
        byte al = UInt8[DS, (ushort)(SI + 0x1A)];
        AL = al;
        // cmp al, cl; jc 6D7A → if AL < CL (unsigned), exit without update
        if (al < CL) {
            return NearRet();
        }
        BX = SI;
        CL = al;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x6D19 — chain merge (Tech/40).
    /// </summary>
    /// <remarks>
    /// Asm (entry through pre-call, 6D19..6D3C):
    /// <code>
    /// 6D19: F6 44 03 E3        test byte [si+0x03], 0xE3
    /// 6D1D: 75 3F                jnz 6D5E
    /// 6D1F: F6 44 10 80         test byte [si+0x10], 0x80
    /// 6D23: 75 39                jnz 6D5E
    /// 6D25: 81 FE E0 08          cmp si, 0x08E0
    /// 6D29: 74 33                jz  6D5E
    /// 6D2B: 8B 7C 04             mov di, [si+0x04]
    /// 6D2E: 33 DB                xor bx, bx
    /// 6D30: 8A 4C 1A             mov cl, [si+0x1A]
    /// 6D33: F6 D1                not cl
    /// 6D35: 8B D6                mov dx, si
    /// 6D37: BD 5F 6D             mov bp, 0x6D5F          ; callback (now C#)
    /// 6D3A: E8 E0 F8             call 0x661D             ; iterator (still asm; uses `call bp`)
    /// 6D3D: ...                  (continuation at 0x6D3D)
    /// </code>
    /// Iterates the chain looking for the best merge candidate (via the C# callback at
    /// <c>0x6D5F</c>), then chain-ports through asm <c>cs1:0x661D</c> to the continuation
    /// at <c>cs1:0x6D3D</c>.
    /// </remarks>
    public System.Action ChainMerge_1000_6D19_016D19(int gotoAddress) {
        byte v3 = UInt8[DS, (ushort)(SI + 0x03)];
        if ((v3 & 0xE3) != 0) {
            return NearRet();
        }
        byte v10 = UInt8[DS, (ushort)(SI + 0x10)];
        if ((v10 & 0x80) != 0) {
            return NearRet();
        }
        if (SI == 0x08E0) {
            return NearRet();
        }
        DI = UInt16[DS, (ushort)(SI + 0x04)];
        BX = 0;
        byte v1A = UInt8[DS, (ushort)(SI + 0x1A)];
        CL = (byte)(~v1A);
        DX = SI;
        BP = 0x6D5F;
        // call 0x661D — chain-port: synth push 0x6D3D, NearJump
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x6D3D;
        return NearJump(0x661D);
    }

    /// <summary>
    /// Override for cs1:0x6D3D — continuation of <see cref="ChainMerge_1000_6D19_016D19"/>
    /// after the asm-side <c>cs1:0x661D</c> chain-iterator returns.
    /// </summary>
    /// <remarks>
    /// Asm (6D3D..6D5E):
    /// <code>
    /// 6D3D: 0B DB              or  bx, bx
    /// 6D3F: 74 1D               jz  6D5E
    /// 6D41: 8A 44 1A             mov al, [si+0x1A]
    /// 6D44: 00 47 1A             add [bx+0x1A], al
    /// 6D47: 8A 44 19             mov al, [si+0x19]
    /// 6D4A: 8A E0                mov ah, al
    /// 6D4C: 22 47 19             and al, [bx+0x19]
    /// 6D4F: 88 44 19             mov [si+0x19], al
    /// 6D52: 08 67 19             or  [bx+0x19], ah
    /// 6D55: 81 4F 12 00 02       or  word [bx+0x12], 0x0200
    /// 6D5A: E8 54 F9             call 0x66B1                    ; (C#)
    /// 6D5D: F9                   stc
    /// 6D5E: C3                   ret
    /// </code>
    /// Calls <c>0x66B1</c> via chain-port (the C# function may itself NearJump to the asm
    /// <c>0xC58A</c>) — synth-push <c>0x6D5D</c> so the chain naturally lands on the
    /// asm <c>stc; ret</c> at the function tail. Doesn't need a separate continuation.
    /// </remarks>
    public System.Action ChainMergeContinuation_1000_6D3D_016D3D(int gotoAddress) {
        // or bx, bx; jz 6D5E
        if (BX == 0) {
            // ZF=1 path: just stc; ret (the asm at 6D5D..6D5E)
            CarryFlag = true;
            return NearRet();
        }

        // mov al, [si+0x1A]; add [bx+0x1A], al
        byte alA = UInt8[DS, (ushort)(SI + 0x1A)];
        AL = alA;
        byte sumA = (byte)(UInt8[DS, (ushort)(BX + 0x1A)] + alA);
        UInt8[DS, (ushort)(BX + 0x1A)] = sumA;

        // mov al, [si+0x19]; mov ah, al
        byte al19 = UInt8[DS, (ushort)(SI + 0x19)];
        AL = al19;
        AH = al19;

        // and al, [bx+0x19]
        byte bxAnd = UInt8[DS, (ushort)(BX + 0x19)];
        AL = (byte)(al19 & bxAnd);

        // mov [si+0x19], al
        UInt8[DS, (ushort)(SI + 0x19)] = AL;

        // or [bx+0x19], ah
        UInt8[DS, (ushort)(BX + 0x19)] = (byte)(bxAnd | AH);

        // or word [bx+0x12], 0x0200
        ushort bxV12 = UInt16[DS, (ushort)(BX + 0x12)];
        UInt16[DS, (ushort)(BX + 0x12)] = (ushort)(bxV12 | 0x0200);

        // call 0x66B1 — chain-port so 0x66B1's internal asm-chain (potentially through 0xC58A)
        // eventually rets to 0x6D5D (the asm `stc; ret` at the function tail).
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x6D5D;
        return NearJump(0x66B1);
    }

    /// <summary>
    /// Override for cs1:0x1EA1 — callback used by <c>cs1:0x661D</c> via the <c>call bp</c>
    /// pattern. Increments DX when the chain link's flag byte at <c>[si+0x03]</c> is below 0x08.
    /// </summary>
    /// <remarks>
    /// Asm (8 bytes):
    /// <code>
    /// 1EA1: 80 7C 03 08    cmp byte [si+0x03], 0x08
    /// 1EA5: 83 D2 00       adc dx, 0
    /// 1EA8: C3             ret
    /// </code>
    /// Pure leaf. The <c>cmp</c> sets CF=1 if <c>[si+0x03] &lt; 0x08</c>; <c>adc dx, 0</c>
    /// adds CF into DX. Caller <c>0x1E5B</c> (chain elector) uses this to count "active"
    /// chain links per record.
    /// </remarks>
    public System.Action ChainCountActiveCallback_1000_1EA1_011EA1(int gotoAddress) {
        byte flag = UInt8[DS, (ushort)(SI + 0x03)];
        // cmp sets CF=1 if flag < 0x08
        bool cf = flag < 0x08;
        CarryFlag = cf;
        // adc dx, 0 → DX += CF
        if (cf) {
            DX = (ushort)(DX + 1);
        }
        return NearRet();
    }

    public System.Action ChainDecommissionContinuation_1000_66BE_0166BE(int gotoAddress) {
        // pop di
        DI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        // pop si
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        // mov word [si+0x04], 0x0FBC
        UInt16[DS, (ushort)(SI + 0x04)] = 0x0FBC;
        // mov byte [si+0x03], 0xA0
        UInt8[DS, (ushort)(SI + 0x03)] = 0xA0;
        // mov byte [si+0x1A], 0
        UInt8[DS, (ushort)(SI + 0x1A)] = 0;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x858C — walks a follow-link chain starting at <c>ds:[SI+0x04]</c>,
    /// looking for a record whose <c>[+0x09]</c> matches the current record's first byte.
    /// On match, swaps a byte slot; on chain-end, falls through to the cleanup path.
    /// </summary>
    /// <remarks>
    /// Asm (64 bytes, 858C..85CB):
    /// <code>
    /// 858C: 8B EE             mov bp, si
    /// 858E: 8A 04             mov al, [si]         ; target id
    /// 8590: 8B 7C 04          mov di, [si+0x04]    ; follow-link
    /// 8593: 57                push di
    /// 8594: F6 44 03 40       test byte [si+0x03], 0x40
    /// 8598: 75 20             jnz 85BA              ; flag bit set → clean up
    /// 859A: 3A 45 09          cmp al, [di+0x09]
    /// 859D: 74 23             jz  85C2              ; direct match → alt exit
    /// 859F: 8A C8             mov cl, al           ; CL = target id (preserve)
    /// 85A1: 8A 45 09          mov al, [di+0x09]
    /// 85A4: E8 5F E3          call 0x6906           ; (C#) AL becomes new record idx; SI = its addr
    /// 85A7: 8A 44 01          mov al, [si+0x01]
    /// 85AA: 8B FE             mov di, si
    /// 85AC: 0A C0             or  al, al
    /// 85AE: 74 0A             jz  85BA              ; chain ended → cleanup
    /// 85B0: 3A C1             cmp al, cl
    /// 85B2: 75 F0             jnz 85A4              ; not target → loop
    /// 85B4: 8A 66 01          mov ah, [bp+0x01]
    /// 85B7: 88 64 01          mov [si+0x01], ah
    /// 85BA: 8B F5             mov si, bp           ; cleanup
    /// 85BC: C6 44 01 00       mov byte [si+0x01], 0
    /// 85C0: 5F                pop di
    /// 85C1: C3                ret
    /// 85C2: 32 E4             xor ah, ah           ; alt exit (direct match)
    /// 85C4: 86 64 01          xchg ah, [si+0x01]
    /// 85C7: 5F                pop di
    /// 85C8: 88 65 09          mov [di+0x09], ah
    /// 85CB: C3                ret
    /// </code>
    /// Internal call to <c>cs1:0x6906</c> is a direct C# invocation — mirror the asm's
    /// 2-byte stack push of <c>0x85A7</c> for memdiff parity.
    /// </remarks>
    public System.Action ChainTraverseAndPatch_1000_858C_01858C(int gotoAddress) {
        ushort bp = SI;
        BP = bp;
        byte al = UInt8[DS, SI];
        AL = al;
        ushort di = UInt16[DS, (ushort)(SI + 0x04)];
        DI = di;
        // push di
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = di;

        byte flag = UInt8[DS, (ushort)(SI + 0x03)];
        if ((flag & 0x40) != 0) {
            // jnz 85BA — cleanup path
            goto cleanup;
        }
        byte recId = UInt8[DS, (ushort)(DI + 0x09)];
        if (al == recId) {
            // jz 85C2 — direct match alt exit
            AH = 0;
            byte sub1 = UInt8[DS, (ushort)(SI + 0x01)];
            UInt8[DS, (ushort)(SI + 0x01)] = 0;  // xchg (ah=0) with [si+1]
            AH = sub1;
            // pop di
            DI = UInt16[SS, SP];
            SP = (ushort)(SP + 2);
            UInt8[DS, (ushort)(DI + 0x09)] = sub1;
            return NearRet();
        }

        // CL = al (target id)
        byte cl = al;
        CL = cl;
        al = UInt8[DS, (ushort)(DI + 0x09)];  // al = [di+9]
        AL = al;

        // Loop calling 0x6906
        while (true) {
            // call 0x6906 — mirror stack push of 0x85A7
            SP = (ushort)(SP - 2);
            UInt16[SS, SP] = 0x85A7;
            FindRecordByIndex08AA_1000_6906_016906(0);
            SP = (ushort)(SP + 2);

            // After 6906: SI = new record addr; AX = caller's original AL preserved at xchg
            al = UInt8[DS, (ushort)(SI + 0x01)];
            AL = al;
            DI = SI;
            if (al == 0) {
                // jz 85BA — chain ended
                goto cleanup;
            }
            if (al == cl) {
                // matched — write [bp+1] into [si+1]
                byte src = UInt8[DS, (ushort)(BP + 0x01)];
                AH = src;
                UInt8[DS, (ushort)(SI + 0x01)] = src;
                break;  // fall through to cleanup
            }
            // jnz 85A4 — loop
        }

    cleanup:
        SI = BP;
        UInt8[DS, (ushort)(SI + 0x01)] = 0;
        // pop di
        DI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }
}
