namespace Cryogenic.Overrides;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Partial class containing helpers invoked by the outer dialogue VM at
/// <c>cs1:0x9F9E</c> (Tech/16 §3, Tech/28). These are leaf-up ports of
/// individual engine helpers — each is a faithful byte-for-byte reproduction
/// of its asm, with no stubs.
/// </summary>
/// <remarks>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly.
/// </remarks>
public partial class Overrides {
    /// <summary>
    /// Registers outer-VM helper overrides with Spice86.
    /// </summary>
    public void DefineOuterVmHelpersCodeOverrides() {
        DefineFunction(cs1, 0x2E98, OuterVmHelperRecordPointers_1000_2E98_012E98);
        DefineFunction(cs1, 0x94F3, OuterVmHelperPerScriptInit_1000_94F3_0194F3);
        DefineFunction(cs1, 0xA0F1, OuterVmSideEffectsPreSpeak_1000_A0F1_01A0F1);
        DefineFunction(cs1, 0xD00F, ResolveScriptIndexAndLoadResource_1000_D00F_01D00F);
        DefineFunction(cs1, 0xD035, ResolveScriptIndexLoadResourceContinuation_1000_D035_01D035);
        DefineFunction(cs1, 0xCF70, FetchSpriteRecord_1000_CF70_01CF70);
        DefineFunction(cs1, 0xCF7B, FetchSpriteRecordContinuation_1000_CF7B_01CF7B);
        DefineFunction(cs1, 0x88F1, DecodePhraseToBuffer_1000_88F1_0188F1);
        DefineFunction(cs1, 0x8A3B, Guard47DEBit4_1000_8A3B_018A3B);
        DefineFunction(cs1, 0x9197, GuardSceneNotTerminator_1000_9197_019197);
        DefineFunction(cs1, 0xD04E, StoreDxBxToD82CD832_1000_D04E_01D04E);
        DefineFunction(cs1, 0x8DF0, ScanBufferA9D0_1000_8DF0_018DF0);
        DefineFunction(cs1, 0x9A60, SkipNNullTerminatedStrings_1000_9A60_019A60);
        DefineFunction(cs1, 0xE3B7, LcgPrng_1000_E3B7_01E3B7);
        DefineFunction(cs1, 0x8AC3, Set47E0Clear47DEBit4_1000_8AC3_018AC3);
        // 0xD082 (SetFontToBook), 0xE270 (PushAll), 0xE283 (PopAll) are already in
        // DisplayCode.cs — no duplicate registration needed here.
        DefineFunction(cs1, 0x5D36, ScoreThresholdFlag_1000_5D36_015D36);
        DefineFunction(cs1, 0x6917, FindRecordByKey3CAF_1000_6917_016917);
        DefineFunction(cs1, 0x8E9E, EmitMassAndPair_1000_8E9E_018E9E);
        DefineFunction(cs1, 0x9123, ComputeTopicSlotFromAL_1000_9123_019123);
        DefineFunction(cs1, 0x9025, SetupAndIndirectFar38C9_1000_9025_019025);
        DefineFunction(cs1, 0x8944, ExpandDialogueMacroString_1000_8944_018944);
    }

    /// <summary>
    /// Override for cs1:0x8944 — recursive dialogue macro/escape-code string expander.
    /// Copies a string from <c>ds:si</c> into <c>es:di</c> (es=ds at entry), expanding
    /// control bytes ≥0x80 in place. Maintains a manual recursion stack of (SI, DS)
    /// pairs in a 0x32-byte SS-relative scratch frame (bp walks it, 4 bytes/level) so
    /// escape codes 0x80–0x8F can recurse into nested macro strings.
    /// </summary>
    /// <remarks>
    /// Asm (160 B, two exits — <c>ret</c> @89E3 and tail-jump @896E to the separate
    /// routine 0x89E4):
    /// <code>
    /// 8944: 83 EC 32      sub sp, 0x32          ; allocate frame
    /// 8947: 8B EC         mov bp, sp
    /// 8949: 1E 07         push ds / pop es      ; es = ds
    /// 894B: cmp [si],0x20 / jnz 8953 / inc si / jmp 894B   ; skip leading spaces
    /// 8953: AC            lodsb
    /// 8954: 0A C0 / 78 03 js 895B               ; ≥0x80 → control byte
    /// 8958: AA / EB F8                          ; else copy literal, loop
    /// 895B: A2 7F 47      mov [0x477F], al
    ///       cmp 0xF0/jnc 89B0 ; cmp 0xD0/jnc 899B ; cmp 0xA0/jnc 89AD
    ///       cmp 0x90/jc 8970 ; jmp 89E4          ; 0x90–0x9F → tail-jump
    /// 8970: cmp 0x80/jnz 8979
    /// 8974: AD / 86 E0 / jmp 8984               ; 0x80: ax = big-endian next word
    /// 8979: 25 0F 00 / D1 E0 / 8B D8 / 8B 87 EB 11   ; 0x81-8F: ax=[ (al&0xF)*2 +0x11EB ]
    /// 8984: mov [bp],si / mov [bp+2],ds / add bp,4 / mov si,ax   ; push frame, recurse
    /// 898F: E8 .. call 8A3B (C#) ; 8993 call CF70 (C#)
    ///       push es / call CF70 / push es / pop ds / pop es / jmp 8953
    /// 899B: AA / A4 / cmp 0xD2 / jc 89A3 / jmp 8953              ; 0xD0-EF run-copy
    /// 89A3: A4 / cmp 0xD0 / jnz(→8953) / A5 movsw / jmp 8953
    /// 89AD: AA / jmp 8953                                        ; 0xA0-CF: stosb
    /// 89B0: mov bx,sp / cmp bp,bx / jz 89C1                      ; ≥0xF0: pop frame…
    /// 89B6: sub bp,4 / mov si,[bp] / mov ds,[bp+2] / jmp 8953    ;   …or finalize
    /// 89C1: AA / cmp 0xFF / jnz 89C8 / xor si,si
    /// 89C8: mov [0x47B6],si / mov [0x47B8],ds / add sp,0x32
    /// 89D3: test [0x47DE],0x10 / jz 89E3
    /// 89DA: mov bx,3 / call E3B7 (C#) / call 8AC3 (C#)
    /// 89E3: C3            ret
    /// </code>
    /// All four callees are C# (<see cref="Guard47DEBit4_1000_8A3B_018A3B"/>,
    /// <see cref="FetchSpriteRecord_1000_CF70_01CF70"/>,
    /// <see cref="LcgPrng_1000_E3B7_01E3B7"/>,
    /// <see cref="Set47E0Clear47DEBit4_1000_8AC3_018AC3"/>) — invoked directly for side
    /// effects; their NearRet results are discarded. The frame and the
    /// <c>push es;call CF70;push es;pop ds;pop es</c> idiom are reproduced exactly via
    /// SS:SP so SP/BP/stack-memory match the asm bit-for-bit. Forward DF assumed
    /// (ambient cld). Net SP delta is 0 (sub 0x32 … add 0x32; balanced push/pop).
    /// </remarks>
    public System.Action ExpandDialogueMacroString_1000_8944_018944(int gotoAddress) {
        // 8944: sub sp,0x32 ; mov bp,sp ; push ds ; pop es
        SP = (ushort)(SP - 0x32);
        BP = SP;
        ES = DS;
        // 894B: skip leading spaces
        while (UInt8[DS, SI] == 0x20) {
            SI = (ushort)(SI + 1);
        }
        while (true) {
            // 8953: lodsb
            byte al = UInt8[DS, SI];
            SI = (ushort)(SI + 1);
            AL = al;
            // 8954: or al,al ; 8956: js 895B  (high bit = control byte)
            if ((al & 0x80) == 0) {
                // 8958: stosb ; 8959: jmp 8953
                UInt8[ES, DI] = al;
                DI = (ushort)(DI + 1);
                continue;
            }
            // 895B: mov [0x477F], al
            UInt8[DS, 0x477F] = al;
            if (al >= 0xF0) {
                // 89B0: mov bx,sp ; cmp bp,bx ; jz 89C1
                if (BP != SP) {
                    // 89B6: sub bp,4 ; mov si,[bp] ; mov ds,[bp+2] ; jmp 8953
                    BP = (ushort)(BP - 0x04);
                    SI = UInt16[SS, (ushort)(BP + 0)];
                    DS = UInt16[SS, (ushort)(BP + 2)];
                    continue;
                }
                // 89C1: finalize
                UInt8[ES, DI] = al;
                DI = (ushort)(DI + 1);
                if (al == 0xFF) {
                    SI = 0;
                }
                UInt16[DS, 0x47B6] = SI;
                UInt16[DS, 0x47B8] = DS;
                SP = (ushort)(SP + 0x32);
                if ((UInt8[DS, 0x47DE] & 0x10) != 0) {
                    BX = 0x0003;
                    LcgPrng_1000_E3B7_01E3B7(0);
                    Set47E0Clear47DEBit4_1000_8AC3_018AC3(0);
                }
                return NearRet();
            }
            if (al >= 0xD0) {
                // 899B: stosb ; movsb ; cmp al,0xD2 ; jc 89A3
                UInt8[ES, DI] = al;
                DI = (ushort)(DI + 1);
                UInt8[ES, DI] = UInt8[DS, SI];
                SI = (ushort)(SI + 1);
                DI = (ushort)(DI + 1);
                if (al >= 0xD2) {
                    continue;
                }
                // 89A3: movsb
                UInt8[ES, DI] = UInt8[DS, SI];
                SI = (ushort)(SI + 1);
                DI = (ushort)(DI + 1);
                if (al != 0xD0) {
                    // al == 0xD1 : jmp 8953
                    continue;
                }
                // 89AA: movsw
                UInt16[ES, DI] = UInt16[DS, SI];
                SI = (ushort)(SI + 2);
                DI = (ushort)(DI + 2);
                continue;
            }
            if (al >= 0xA0) {
                // 89AD: stosb ; jmp 8953
                UInt8[ES, DI] = al;
                DI = (ushort)(DI + 1);
                continue;
            }
            if (al < 0x90) {
                // 8970: cmp al,0x80 ; jnz 8979
                ushort ax;
                if (al == 0x80) {
                    // 8974: lodsw ; 8975: xchg ah,al  → ax = big-endian word
                    byte lo = UInt8[DS, SI];
                    byte hi = UInt8[DS, (ushort)(SI + 1)];
                    SI = (ushort)(SI + 2);
                    ax = (ushort)((lo << 8) | hi);
                } else {
                    // 8979: and ax,0x000F ; shl ax,1 ; mov bx,ax ; mov ax,[bx+0x11EB]
                    ushort idx = (ushort)((al & 0x0F) << 1);
                    BX = idx;
                    ax = UInt16[DS, (ushort)(idx + 0x11EB)];
                }
                AX = ax;
                // 8984: push frame (SI, DS) ; bp+=4 ; si=ax
                UInt16[SS, (ushort)(BP + 0)] = SI;
                UInt16[SS, (ushort)(BP + 2)] = DS;
                BP = (ushort)(BP + 0x04);
                SI = ax;
                // 898F: call 8A3B (C#)
                Guard47DEBit4_1000_8A3B_018A3B(0);
                // 8992: push es ; 8993: call CF70 (C#) ; 8996: push es ; pop ds ; pop es
                ushort esSaved = ES;
                FetchSpriteRecord_1000_CF70_01CF70(0);
                DS = ES;
                ES = esSaved;
                continue;
            }
            // 896E: 0x90–0x9F → jmp 89E4 (separate routine, still asm/dispatched)
            return NearJump(0x89E4);
        }
    }

    /// <summary>
    /// Override for cs1:0x9025 — pre-call state setup + indirect far call through
    /// <c>ds:[0x38C9]</c>. Called from the 9FD8 side-effects path of the outer dialogue VM.
    /// </summary>
    /// <remarks>
    /// Asm (33 bytes):
    /// <code>
    /// 9025: 8B 0E 93 47        mov cx, [0x4793]
    /// 9029: BB 92 00            mov bx, 0x0092
    /// 902C: 2B D9                sub bx, cx
    /// 902E: 33 D2                xor dx, dx
    /// 9030: B5 FF                mov ch, 0xFF
    /// 9032: BF 40 01              mov di, 0x0140
    /// 9035: 8B 36 FC 22           mov si, [0x22FC]
    /// 9039: 8E 06 DA DB           mov es, [0xDBDA]
    /// 903D: 89 1E 82 47           mov [0x4782], bx
    /// 9041: FF 1E C9 38           call far [0x38C9]
    /// 9045: C3                    ret
    /// </code>
    /// Sets up DI/SI/ES, computes BX = 0x92 - [0x4793], stores BX at <c>[0x4782]</c>,
    /// then indirect-far-calls through the function-pointer slot at <c>ds:0x38C9</c>.
    /// Simulated via <c>FarJump</c> with synthetic CS:IP stack push of <c>cs1:0x9045</c>.
    /// </remarks>
    public Action SetupAndIndirectFar38C9_1000_9025_019025(int gotoAddress) {
        ushort cx = (ushort)globalsOnDs.Get1138_4793_Word16();
        CX = cx;
        ushort bx = (ushort)(0x0092 - cx);
        BX = bx;
        DX = 0;
        CH = 0xFF;
        DI = 0x0140;
        SI = (ushort)globalsOnDs.Get1138_22FC_Word16();
        ES = globalsOnDs.Get1138_DBDA_Word16_framebufferActive();
        UInt16[DS, 0x4782] = bx;
        // call far [0x38C9]
        ushort targetOff = UInt16[DS, 0x38C9];
        ushort targetSeg = UInt16[DS, 0x38CB];
        // Far-call: push CS, push IP (0x9045 — the bare-ret byte)
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x9045;
        return FarJump(targetSeg, targetOff);
    }

    /// <summary>
    /// Override for cs1:0x2E98 — outer-VM helper that publishes a record's lookup keys.
    /// </summary>
    /// <remarks>
    /// Asm (26 bytes, 2E98..2EB1):
    /// <code>
    /// 2E98: 89 3E E6 47    mov [0x47E6], di
    /// 2E9C: 32 E4          xor ah, ah
    /// 2E9E: 8A 05          mov al, [di]
    /// 2EA0: 05 00 00       add ax, 0x0000
    /// 2EA3: A3 ED 11       mov [0x11ED], ax
    /// 2EA6: 8A 45 01       mov al, [di+0x01]
    /// 2EA9: 32 E4          xor ah, ah
    /// 2EAB: 05 0C 00       add ax, 0x000C
    /// 2EAE: A3 EF 11       mov [0x11EF], ax
    /// 2EB1: C3             ret
    /// </code>
    /// Pure leaf (no external calls). Writes <c>DI</c> to <c>ds[0x47E6]</c>, then derives
    /// two zero-extended-byte values from <c>[DI]</c> and <c>[DI+1]</c> (adding 0 and
    /// <c>0x0C</c> respectively) and publishes them at <c>ds[0x11ED]</c> / <c>ds[0x11EF]</c>.
    /// The <c>add ax, 0</c> at 2EA0 looks like dead code but its flag side-effects are
    /// preserved here via <c>Alu16.Add</c>.
    /// </remarks>
    public System.Action OuterVmHelperRecordPointers_1000_2E98_012E98(int gotoAddress) {
        UInt16[DS, 0x47E6] = DI;
        AH = 0;
        byte b0 = UInt8[DS, DI];
        AL = b0;
        // add ax, 0 — preserves the asm's flag-setting side effect
        AX = Alu16.Add(AX, 0);
        UInt16[DS, 0x11ED] = AX;
        byte b1 = UInt8[DS, (ushort)(DI + 1)];
        AL = b1;
        AH = 0;
        AX = Alu16.Add(AX, 0x000C);
        UInt16[DS, 0x11EF] = AX;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x94F3 — outer-VM per-script init helper (Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm (~64 bytes, 94F3..9532):
    /// <code>
    /// 94F3: 83 3E C4 47 10        cmp word [0x47C4], 0x10
    /// 94F8: 73 38                  jnc 9532                       ; early return when scene_id ≥ 0x10
    /// 94FA: 56                     push si
    /// 94FB: 8B 36 A2 47            mov si, [0x47A2]
    /// 94FF: 8A 44 0F               mov al, [si+0x0F]
    /// 9502: A2 18 00               mov [0x0018], al
    /// 9505: A8 40                  test al, 0x40
    /// 9507: 8B 44 08               mov ax, [si+0x08]
    /// 950A: 75 03                  jnz 950F                       ; if bit 6 set, keep [si+8]; else use [si+0xA]
    /// 950C: 8B 44 0A               mov ax, [si+0x0A]
    /// 950F: 2B 06 02 00            sub ax, [0x0002]
    /// 9513: F7 D8                  neg ax
    /// 9515: A3 16 00               mov [0x0016], ax
    /// 9518: 5E                     pop si
    /// 9519: 80 3E 2A 00 64         cmp byte [0x002A], 0x64
    /// 951E: 73 12                  jnc 9532                       ; early return when topic ≥ 0x64
    /// 9520: 83 3E C4 47 09         cmp word [0x47C4], 0x09
    /// 9525: 73 0B                  jnc 9532                       ; early return when scene_id ≥ 0x09
    /// 9527: 8B 3E DB 11            mov di, [0x11DB]
    /// 952B: 0B FF                  or di, di
    /// 952D: 74 03                  jz 9532                        ; early return when [0x11DB] is zero
    /// 952F: E8 66 99                call 0x2E98                    ; pushes 0x9532 as return addr
    /// 9532: C3                     ret
    /// </code>
    /// Three early-return guards (`scene_id ≥ 0x10`, `topic ≥ 0x64`, `scene_id ≥ 0x09`,
    /// `[0x11DB]==0`) all jump straight to the trailing <c>ret</c>. The one external
    /// call goes to <c>0x2E98</c> (now also ported in C#); we invoke it directly as a
    /// C# method since both live in the same <c>partial class</c>. The asm's
    /// <c>call 0x2E98</c> would push <c>0x9532</c> as the return address; we mirror
    /// that single 2-byte stack write for memdiff parity.
    /// </remarks>
    /// <summary>
    /// Override for cs1:0xA0F1 — outer-VM side-effects pre-speak helper.
    /// </summary>
    /// <remarks>
    /// Asm (19 bytes, A0F1..A103):
    /// <code>
    /// A0F1: 80 3E E7 28 02   cmp byte [0x28E7], 0x02
    /// A0F6: 75 0B            jnz A103
    /// A0F8: F6 44 02 10      test byte [si+0x02], 0x10
    /// A0FC: 74 06            jz  A104                  ; (A104 = trampoline `jmp 0x8C8A`)
    /// A0FE: C6 06 E7 28 01   mov byte [0x28E7], 0x01
    /// A103: C3               ret
    /// </code>
    /// Pure leaf. When the speak-state at <c>ds[0x28E7]</c> is exactly 2 AND bit 4 of the
    /// current record's <c>byte[2]</c> (verb byte) is set, demotes the speak state to 1.
    /// When bit 4 is clear, tail-jumps to <c>cs1:0xA104</c> — a 3-byte trampoline whose
    /// <c>jmp rel16</c> lands at <c>cs1:0x8C8A</c>. We preserve the asm-faithful path by
    /// jumping to <c>0xA104</c>, not directly to <c>0x8C8A</c>.
    /// </remarks>
    public System.Action OuterVmSideEffectsPreSpeak_1000_A0F1_01A0F1(int gotoAddress) {
        byte speakState = UInt8[DS, 0x28E7];
        if (speakState != 0x02) {
            return NearRet();
        }
        byte verbByte = UInt8[DS, (ushort)(SI + 0x02)];
        if ((verbByte & 0x10) == 0) {
            // jz A104 — tail-jump to the asm trampoline that does `jmp 0x8C8A`.
            return NearJump(0xA104);
        }
        UInt8[DS, 0x28E7] = 0x01;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xD00F — resolves a per-language script resource index and
    /// loads it. Called from <c>cs1:0xCF70</c> on the SI-bit-11-set path.
    /// </summary>
    /// <remarks>
    /// Asm (45 bytes, D00F..D03B):
    /// <code>
    /// D00F: A1 7C 47        mov ax, [0x477C]        ; script base
    /// D012: 3B 06 D6 AA     cmp ax, [0xAAD6]
    /// D016: B0 93           mov al, 0x93
    /// D018: 72 02           jc D01C                 ; (skip mov al, 0x9A if AX &lt; [AAD6])
    /// D01A: B0 9A           mov al, 0x9A
    /// D01C: 02 06 EB CE     add al, [0xCEEB]        ; per-language offset
    /// D020: 3A 06 7E 47     cmp al, [0x477E]
    /// D024: 74 15           jz D03B                 ; same resource cached → ret
    /// D026: 56              push si
    /// D027: A2 7E 47        mov [0x477E], al
    /// D02A: 32 E4           xor ah, ah
    /// D02C: 8B F0           mov si, ax
    /// D02E: C4 3E B0 47     les di, [0x47B0]
    /// D032: E8 84 20        call 0xF0B9             ; open_resource_by_index (still asm)
    /// D035: 51              push cx                 ; (continuation entry)
    /// D036: E8 5F 30        call 0x0098             ; ConvertIndexTableToPointerTable (C#)
    /// D039: 59              pop cx
    /// D03A: 5E              pop si
    /// D03B: C3              ret
    /// </code>
    /// Picks <c>0x93</c> or <c>0x9A</c> as the base resource index (depending on whether
    /// the current script base lives before/after <c>[0xAAD6]</c>), adds the language byte
    /// at <c>[0xCEEB]</c>, and if the resulting index differs from the cached value at
    /// <c>[0x477E]</c>, loads the corresponding resource. <c>cs1:0xF0B9</c>
    /// (<c>open_resource_by_index_si</c>) is still asm; we chain-port through it with the
    /// continuation at <c>cs1:0xD035</c>.
    /// </remarks>
    public System.Action ResolveScriptIndexAndLoadResource_1000_D00F_01D00F(int gotoAddress) {
        ushort scriptBase = (ushort)globalsOnDs.Get1138_477C_Word16();
        AX = scriptBase;
        ushort aad6 = (ushort)globalsOnDs.Get1138_AAD6_Word16();
        // cmp ax, [0xAAD6]; mov al, 0x93; jc D01C; mov al, 0x9A
        byte alVal = (byte)(scriptBase < aad6 ? 0x93 : 0x9A);
        AL = alVal;
        // add al, [0xCEEB]
        byte langOffset = (byte)globalsOnDs.Get1138_CEEB_Byte8_languageSetting();
        alVal = (byte)(alVal + langOffset);
        AL = alVal;
        // cmp al, [0x477E]; jz D03B
        byte cached = (byte)globalsOnDs.Get1138_477E_Byte8();
        if (alVal == cached) {
            return NearRet();
        }
        // push si
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;
        // mov [0x477E], al
        UInt8[DS, 0x477E] = alVal;
        // xor ah, ah; mov si, ax  (AX is now alVal zero-extended)
        AH = 0;
        AX = alVal;
        SI = alVal;
        // les di, [0x47B0]
        SegmentedAddress p47b0 = globalsOnDs.GetPtr1138_47B0_Dword32();
        ES = p47b0.Segment;
        DI = p47b0.Offset;
        // call 0xF0B9 — chain-port: push D035 continuation
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xD035;
        return NearJump(0xF0B9);
    }

    /// <summary>
    /// Override for cs1:0xD035 — continuation of <see cref="ResolveScriptIndexAndLoadResource_1000_D00F_01D00F"/>
    /// after the asm <c>cs1:0xF0B9</c> returns.
    /// </summary>
    /// <remarks>
    /// Asm (7 bytes):
    /// <code>
    /// D035: 51              push cx
    /// D036: E8 5F 30        call 0x0098   ; ConvertIndexTableToPointerTable (C# override)
    /// D039: 59              pop cx
    /// D03A: 5E              pop si
    /// D03B: C3              ret
    /// </code>
    /// We mirror the asm <c>call 0x0098</c>'s 2-byte stack push of <c>0xD039</c> for
    /// memdiff parity, then invoke the C# override directly.
    /// </remarks>
    public System.Action ResolveScriptIndexLoadResourceContinuation_1000_D035_01D035(int gotoAddress) {
        // push cx
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = CX;
        // call 0x0098 — mirror the asm's call-stack push of 0xD039 for memdiff parity.
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xD039;
        ConvertIndexTableToPointerTable_1000_0098_010098(0);
        SP = (ushort)(SP + 2);
        // pop cx
        CX = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        // pop si
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xCF70 — fetches a sprite/record from one of two resource tables
    /// (sprite sheets at <c>[0x47AC]</c> or per-language records at <c>[0x47B0]</c>),
    /// indexed by the SI register.
    /// </summary>
    /// <remarks>
    /// Asm (~50 bytes, CF70..CF9F):
    /// <code>
    /// CF70: 53              push bx
    /// CF71: 4E              dec si
    /// CF72: F7 C6 00 08     test si, 0x0800
    /// CF76: 74 1D           jz CF95
    /// CF78: E8 94 00        call 0xD00F             ; resolve script index + load
    /// CF7B: C4 1E B0 47     les bx, [0x47B0]        ; (continuation entry)
    /// CF7F: 81 E6 FF 07     and si, 0x07FF
    /// CF83: D1 E6           shl si, 1
    /// CF85: 26 8B 30        mov si, [es:bx+si]
    /// CF88: 26 8B 1F        mov bx, [es:bx]
    /// CF8B: 26 8B 5F FE     mov bx, [es:bx-2]
    /// CF8F: 89 1E B4 47     mov [0x47B4], bx
    /// CF93: 5B              pop bx
    /// CF94: C3              ret
    /// CF95: D1 E6           shl si, 1
    /// CF97: C4 1E AC 47     les bx, [0x47AC]
    /// CF9B: 26 8B 30        mov si, [es:bx+si]
    /// CF9E: 5B              pop bx
    /// CF9F: C3              ret
    /// </code>
    /// Two paths:
    /// <list type="bullet">
    /// <item><description><b>Bit 11 of SI clear</b> (CF95 path) — sprite-sheet path. Pure leaf: shl si,1; load via <c>[0x47AC]</c> far-ptr; ret.</description></item>
    /// <item><description><b>Bit 11 of SI set</b> — call <c>0xD00F</c> (now C# with continuation), then the post-call work at <c>CF7B</c> stays as a separate continuation override.</description></item>
    /// </list>
    /// </remarks>
    public System.Action FetchSpriteRecord_1000_CF70_01CF70(int gotoAddress) {
        // push bx
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = BX;
        // dec si
        SI = (ushort)(SI - 1);
        // test si, 0x0800
        if ((SI & 0x0800) == 0) {
            // jz CF95 — sprite-sheet path (pure leaf, inline)
            SI = (ushort)(SI << 1);
            SegmentedAddress p47ac = globalsOnDs.GetPtr1138_47AC_Dword32();
            ES = p47ac.Segment;
            BX = p47ac.Offset;
            // mov si, [es:bx+si]
            SI = UInt16[ES, (ushort)(BX + SI)];
            // pop bx
            BX = UInt16[SS, SP];
            SP = (ushort)(SP + 2);
            return NearRet();
        }
        // CF78: call 0xD00F — D00F is now a C# override; we call it via the emulator
        // by pushing the asm-faithful continuation (CF7B) and NearJumping.
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xCF7B;
        return NearJump(0xD00F);
    }

    /// <summary>
    /// Override for cs1:0xCF7B — continuation of <see cref="FetchSpriteRecord_1000_CF70_01CF70"/>
    /// after <c>0xD00F</c> returns.
    /// </summary>
    /// <remarks>
    /// Asm (CF7B..CF94):
    /// <code>
    /// CF7B: C4 1E B0 47    les bx, [0x47B0]
    /// CF7F: 81 E6 FF 07    and si, 0x07FF
    /// CF83: D1 E6          shl si, 1
    /// CF85: 26 8B 30       mov si, [es:bx+si]
    /// CF88: 26 8B 1F       mov bx, [es:bx]
    /// CF8B: 26 8B 5F FE    mov bx, [es:bx-2]
    /// CF8F: 89 1E B4 47    mov [0x47B4], bx
    /// CF93: 5B             pop bx
    /// CF94: C3             ret
    /// </code>
    /// </remarks>
    public System.Action FetchSpriteRecordContinuation_1000_CF7B_01CF7B(int gotoAddress) {
        // les bx, [0x47B0]
        SegmentedAddress p47b0 = globalsOnDs.GetPtr1138_47B0_Dword32();
        ES = p47b0.Segment;
        BX = p47b0.Offset;
        // and si, 0x07FF
        SI = (ushort)(SI & 0x07FF);
        // shl si, 1
        SI = (ushort)(SI << 1);
        // mov si, [es:bx+si]
        SI = UInt16[ES, (ushort)(BX + SI)];
        // mov bx, [es:bx]
        BX = UInt16[ES, BX];
        // mov bx, [es:bx-2]
        BX = UInt16[ES, (ushort)(BX - 2)];
        // mov [0x47B4], bx
        UInt16[DS, 0x47B4] = BX;
        // pop bx
        BX = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x88F1 — phrase decoder. Expands a byte-stream from caller's
    /// <c>ES:SI</c> into a text buffer at <c>ds:0xA840</c>.
    /// </summary>
    /// <remarks>
    /// Asm (83 bytes, 88F1..8943):
    /// <code>
    /// 88F1: 1E             push ds
    /// 88F2: 06             push es
    /// 88F3: 1F             pop ds        ; DS = caller's ES (source data + substitution table)
    /// 88F4: 07             pop es        ; ES = caller's DS = 0x1138 (target buffer)
    /// 88F5: BF 40 A8       mov di, 0xA840
    /// 88F8: AC             lodsb
    /// 88F9: 3C FF          cmp al, 0xFF
    /// 88FB: 74 40          jz 893D       ; terminator → finalize
    /// 88FD: 3C FE          cmp al, 0xFE
    /// 88FF: 74 04          jz 8905       ; literal-0xFE path
    /// 8901: 3C E0          cmp al, 0xE0
    /// 8903: 73 0B          jnc 8910      ; AL ≥ 0xE0 → substitution path
    /// 8905: AA             stosb         ; literal byte path
    /// 8906: B0 FF          mov al, 0xFF
    /// 8908: 81 FF CF A9    cmp di, 0xA9CF
    /// 890C: 73 2F          jnc 893D      ; buffer-full → finalize
    /// 890E: EB E8          jmp 88F8      ; loop
    /// 8910: 24 0F          and al, 0x0F
    /// 8912: 8A E8          mov ch, al    ; CH = low nibble of byte 0
    /// 8914: AD             lodsw         ; AX = next word
    /// 8915: 8A CC          mov cl, ah    ; CL_init = AH of word
    /// 8917: 25 FF 3F       and ax, 0x3FFF
    /// 891A: D0 E9          shr cl, 1
    /// 891C: D0 E9          shr cl, 1
    /// 891E: 80 E1 30       and cl, 0x30
    /// 8921: 0A CD          or  cl, ch    ; CL = (orig_AH bits 5-4 &lt;&lt; 4) | low_nibble
    /// 8923: 32 ED          xor ch, ch    ; CX = CL only
    /// 8925: 56             push si       ; save script SI
    /// 8926: 36 8B 36 B4 47 mov si, [ss:0x47B4]   ; SI = substitution-table base (in caller's ES)
    /// 892B: 03 F0          add si, ax            ; SI += AX (offset into table)
    /// 892D: F3 A4          rep movsb              ; copy CX bytes ds:si → es:di
    /// 892F: 5E             pop si
    /// 8930: 80 3C FF       cmp byte [si], 0xFF
    /// 8933: 74 07          jz 893C       ; substitution-string was FF-terminated → drop space
    /// 8935: 26 C6 05 20    mov byte [es:di], 0x20
    /// 8939: 47             inc di
    /// 893A: EB BC          jmp 88F8      ; loop
    /// 893C: AC             lodsb         ; consume the FF
    /// 893D: AA             stosb         ; emit (FF terminator or last byte)
    /// 893E: BE 40 A8       mov si, 0xA840
    /// 8941: 16             push ss
    /// 8942: 1F             pop ds        ; DS = SS  (caller's original DS, since SS == DS in this engine)
    /// 8943: C3             ret
    /// </code>
    /// At runtime SS == DS == 0x1138, so the <c>ss:</c> override on 8926 simply reads from
    /// the game data segment. <c>[0x47B4]</c> holds the offset (within caller's ES) of the
    /// per-language substitution table. CX is computed from the script byte's low nibble
    /// plus the upper 2 bits of the next word's high byte; AX is the table-offset of the
    /// substitution string (which itself ends with 0xFF — appended space suppressed when so).
    /// Pure leaf — no external calls.
    /// </remarks>
    public System.Action DecodePhraseToBuffer_1000_88F1_0188F1(int gotoAddress) {
        // Swap DS and ES via push/pop (asm: push ds; push es; pop ds; pop es).
        // Mirror the 2 stack writes on SS:SP for memdiff parity.
        ushort savedDs = DS;
        ushort savedEs = ES;
        // push ds
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = savedDs;
        // push es
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = savedEs;
        // pop ds (gets the most-recently-pushed = saved ES)
        DS = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        // pop es (gets the next = saved DS)
        ES = UInt16[SS, SP];
        SP = (ushort)(SP + 2);

        DI = 0xA840;

        while (true) {
            // lodsb — AL = [ds:si], si++
            byte al = UInt8[DS, SI];
            SI = (ushort)(SI + 1);
            AL = al;

            if (al == 0xFF) {
                break;  // jz 893D
            }
            if (al == 0xFE || al < 0xE0) {
                // literal byte path (8905)
                UInt8[ES, DI] = al;
                DI = (ushort)(DI + 1);
                AL = 0xFF;
                if (DI >= 0xA9CF) {
                    // buffer-full → finalize
                    al = 0xFF;
                    AL = al;
                    break;
                }
                continue;
            }
            // Substitution path (8910..893B)
            // ch = al & 0x0F
            byte ch = (byte)(al & 0x0F);
            CH = ch;
            // lodsw
            ushort axWord = UInt16[DS, SI];
            SI = (ushort)(SI + 2);
            AX = axWord;
            // cl = ah (high byte of the word we just loaded)
            byte clInit = (byte)(axWord >> 8);
            CL = clInit;
            // and ax, 0x3FFF
            axWord = (ushort)(axWord & 0x3FFF);
            AX = axWord;
            // shr cl, 1; shr cl, 1 → cl = clInit >> 2
            byte cl = (byte)(clInit >> 2);
            // and cl, 0x30 — keeps only bits 5,4 of original clInit
            cl = (byte)(cl & 0x30);
            // or cl, ch
            cl = (byte)(cl | ch);
            // xor ch, ch → CX = cl
            ushort cx = cl;
            CX = cx;
            CH = 0;
            CL = cl;
            // push si
            SP = (ushort)(SP - 2);
            UInt16[SS, SP] = SI;
            // mov si, [ss:0x47B4] (SS override; both SS and DS-original point at 0x1138)
            ushort tableBase = UInt16[SS, 0x47B4];
            SI = tableBase;
            // add si, ax
            SI = (ushort)(SI + axWord);
            // rep movsb — copy CX bytes from [ds:si] to [es:di]
            while (cx != 0) {
                byte b = UInt8[DS, SI];
                UInt8[ES, DI] = b;
                SI = (ushort)(SI + 1);
                DI = (ushort)(DI + 1);
                cx = (ushort)(cx - 1);
            }
            CX = 0;
            // pop si — restore script-stream SI
            SI = UInt16[SS, SP];
            SP = (ushort)(SP + 2);
            // cmp byte [si], 0xFF; jz 893C
            byte peek = UInt8[DS, SI];
            if (peek == 0xFF) {
                // 893C: lodsb (consume FF); then stosb at 893D → emit FF; then finalize
                AL = peek;
                SI = (ushort)(SI + 1);
                UInt8[ES, DI] = peek;
                DI = (ushort)(DI + 1);
                break;
            }
            // else: insert space separator
            UInt8[ES, DI] = 0x20;
            DI = (ushort)(DI + 1);
            // jmp 88F8 — loop
        }

        // Common finalize at 893D..8943:
        // (893D's stosb was handled inline above when we broke out; the buffer-full path
        //  already set AL=0xFF before break — stosb hasn't fired for that path. Handle it.)
        // Actually the asm always stosb's AL at 893D; we mirror that:
        // - terminator path (AL=0xFF at break point): emit 0xFF (caller knows the terminator)
        // - buffer-full path (AL=0xFF): emit 0xFF too
        // - 0xFE/literal path inside the loop already emitted; we keep that.
        // For the 0xFF terminator from the script, we need to also stosb the 0xFF.
        // Simpler: always stosb the current AL after the break (matches asm flow).
        UInt8[ES, DI] = AL;
        DI = (ushort)(DI + 1);
        // 893E: mov si, 0xA840 — reset SI to the buffer start (caller can read from there)
        SI = 0xA840;
        // 8941..8942: push ss; pop ds → DS = SS
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SS;
        DS = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x8A3B — bit-4 guard at <c>ds[0x47DE]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (8 bytes):
    /// <code>
    /// 8A3B: F6 06 DE 47 10   test byte [0x47DE], 0x10
    /// 8A40: 75 01             jnz 8A43          ; tail-jump into next function
    /// 8A42: C3                ret
    /// </code>
    /// Pure leaf. When bit 4 of <c>ds[0x47DE]</c> is set, tail-jumps into the body
    /// immediately after this function (cs1:0x8A43).
    /// </remarks>
    public System.Action Guard47DEBit4_1000_8A3B_018A3B(int gotoAddress) {
        byte v = UInt8[DS, 0x47DE];
        if ((v & 0x10) != 0) {
            return NearJump(0x8A43);
        }
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x9197 — scene-id-not-terminator guard wrapping cs1:0x91A0.
    /// </summary>
    /// <remarks>
    /// Asm (9 bytes):
    /// <code>
    /// 9197: A1 C4 47   mov ax, [0x47C4]
    /// 919A: 3D FF FF   cmp ax, 0xFFFF
    /// 919D: 75 01      jnz 91A0
    /// 919F: C3         ret
    /// </code>
    /// Pure leaf. Reads scene_id; tail-jumps into <c>cs1:0x91A0</c> when scene_id is not
    /// the 0xFFFF terminator. <c>0x91A0</c> stays as asm.
    /// </remarks>
    public System.Action GuardSceneNotTerminator_1000_9197_019197(int gotoAddress) {
        ushort sceneId = (ushort)globalsOnDs.Get1138_47C4_Word16();
        AX = sceneId;
        if (sceneId != 0xFFFF) {
            return NearJump(0x91A0);
        }
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xD04E — publishes <c>DX</c>/<c>BX</c> to four HUD pointer slots.
    /// </summary>
    /// <remarks>
    /// Asm (17 bytes):
    /// <code>
    /// D04E: 89 16 2C D8   mov [0xD82C], dx
    /// D052: 89 1E 2E D8   mov [0xD82E], bx
    /// D056: 89 16 30 D8   mov [0xD830], dx
    /// D05A: 89 1E 32 D8   mov [0xD832], bx
    /// D05E: C3            ret
    /// </code>
    /// Pure leaf. Writes DX into <c>[0xD82C]</c> and <c>[0xD830]</c>; BX into
    /// <c>[0xD82E]</c> and <c>[0xD832]</c>. Called three times from <c>cs1:0x8B11</c>.
    /// </remarks>
    public System.Action StoreDxBxToD82CD832_1000_D04E_01D04E(int gotoAddress) {
        UInt16[DS, 0xD82C] = DX;
        UInt16[DS, 0xD82E] = BX;
        UInt16[DS, 0xD830] = DX;
        UInt16[DS, 0xD832] = BX;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x8DF0 — scans the buffer at <c>ds:0xA9D0</c> for an entry whose
    /// next word is &gt;= 0x1E. On hit, clears bit 0 of <c>ds[0x4799]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (~32 bytes, 8DF0..8E15):
    /// <code>
    /// 8DF0: F6 06 99 47 01    test byte [0x4799], 0x01
    /// 8DF5: 74 1E              jz 8E15            ; bit clear → ret
    /// 8DF7: 56                 push si
    /// 8DF8: BE D0 A9           mov si, 0xA9D0
    /// 8DFB: AD                  lodsw              ; CX = count
    /// 8DFC: 8B C8              mov cx, ax
    /// 8DFE: AD                  lodsw              ; AX = entry-marker
    /// 8DFF: 0B C0              or  ax, ax
    /// 8E01: 74 05              jz  8E08            ; marker == 0 → advance only
    /// 8E03: 83 3C 1E           cmp word [si], 0x1E
    /// 8E06: 73 07              jnc 8E0F            ; next-word ≥ 0x1E → clear flag, ret
    /// 8E08: 83 C6 04           add si, 4
    /// 8E0B: E2 F1              loop 8DFE           ; CX--; jump if non-zero
    /// 8E0D: 5E                 pop si
    /// 8E0E: C3                 ret
    /// 8E0F: 80 26 99 47 FE     and byte [0x4799], 0xFE   ; clear bit 0
    /// 8E14: 5E                 pop si
    /// 8E15: C3                 ret
    /// </code>
    /// Pure leaf. The <c>loop</c> at 8E0B has the standard quirk: when entered with CX=0,
    /// the decrement underflows to 0xFFFF and iterates 65536 times. We mirror it via a
    /// do-while.
    /// </remarks>
    public System.Action ScanBufferA9D0_1000_8DF0_018DF0(int gotoAddress) {
        byte flags = UInt8[DS, 0x4799];
        if ((flags & 0x01) == 0) {
            return NearRet();
        }
        // push si
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;
        SI = 0xA9D0;
        // lodsw → CX
        ushort count = UInt16[DS, SI];
        SI = (ushort)(SI + 2);
        AX = count;
        CX = count;
        do {
            // lodsw → AX
            ushort entry = UInt16[DS, SI];
            SI = (ushort)(SI + 2);
            AX = entry;
            if (entry != 0) {
                ushort next = UInt16[DS, SI];
                if (next >= 0x001E) {
                    // 8E0F: clear bit 0 of [0x4799]
                    UInt8[DS, 0x4799] = (byte)(flags & 0xFE);
                    // pop si; ret
                    SI = UInt16[SS, SP];
                    SP = (ushort)(SP + 2);
                    return NearRet();
                }
            }
            // 8E08: add si, 4
            SI = (ushort)(SI + 4);
            // loop: CX--; if CX != 0 continue
            CX = (ushort)(CX - 1);
        } while (CX != 0);
        // 8E0D: pop si; ret
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x9A60 — advances <c>SI</c> past <c>AL</c> NUL-terminated strings,
    /// scanning forward in ES:DI via <c>repne scasb</c>.
    /// </summary>
    /// <remarks>
    /// Asm (27 bytes, 9A60..9A7A):
    /// <code>
    /// 9A60: 0A C0          or  al, al
    /// 9A62: 74 10          jz  9A74              ; AL == 0: skip scan, only update [0x47CE]
    /// 9A64: 8B D8          mov bx, ax            ; BX = count
    /// 9A66: 32 C0          xor al, al            ; AL = 0 (scasb target)
    /// 9A68: B9 FF FF       mov cx, 0xFFFF
    /// 9A6B: 8B FE          mov di, si
    /// 9A6D: F2 AE          repne scasb           ; scan ES:DI for NUL
    /// 9A6F: 4B             dec bx
    /// 9A70: 75 FB          jnz 9A6D
    /// 9A72: 8B F7          mov si, di
    /// 9A74: C7 06 CE 47 08 00  mov word [0x47CE], 8
    /// 9A7A: C3             ret
    /// </code>
    /// Pure leaf. Mirrors the asm's CX/DI/AL updates exactly. The scasb loop is bounded
    /// by CX (0xFFFF guard) but in practice always finds a NUL inside the string table.
    /// </remarks>
    public System.Action SkipNNullTerminatedStrings_1000_9A60_019A60(int gotoAddress) {
        if (AL == 0) {
            UInt16[DS, 0x47CE] = 0x0008;
            return NearRet();
        }
        BX = AX;
        AL = 0;
        CX = 0xFFFF;
        DI = SI;
        // Outer loop: BX = count; each iteration runs repne scasb to find one NUL.
        while (BX != 0) {
            // repne scasb: while CX > 0 AND [es:di] != AL, advance di
            while (CX != 0) {
                byte b = UInt8[ES, DI];
                DI = (ushort)(DI + 1);
                CX = (ushort)(CX - 1);
                if (b == AL) {
                    break;
                }
            }
            BX = (ushort)(BX - 1);
        }
        SI = DI;
        UInt16[DS, 0x47CE] = 0x0008;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xE3B7 — linear-congruential PRNG. State at <c>ds[0xD824]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (21 bytes):
    /// <code>
    /// E3B7: 52              push dx
    /// E3B8: A1 24 D8        mov ax, [0xD824]
    /// E3BB: BA 6D E5        mov dx, 0xE56D
    /// E3BE: F7 E2           mul dx               ; DX:AX = AX * 0xE56D
    /// E3C0: 40              inc ax
    /// E3C1: A3 24 D8        mov [0xD824], ax     ; state' = (state * 0xE56D + 1) low16
    /// E3C4: 8A C4           mov al, ah
    /// E3C6: 8A E2           mov ah, dl
    /// E3C8: 23 C3           and ax, bx           ; mask result by BX
    /// E3CA: 5A              pop dx
    /// E3CB: C3              ret
    /// </code>
    /// Standard 16-bit LCG: <c>state = state * 0xE56D + 1 (mod 2^16)</c>. The returned
    /// value is a 16-bit shuffle of mul's high/low bytes, masked by <c>BX</c>. Pure leaf.
    /// </remarks>
    public System.Action LcgPrng_1000_E3B7_01E3B7(int gotoAddress) {
        // push dx
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = DX;
        ushort state = (ushort)globalsOnDs.Get1138_D824_Word16();
        AX = state;
        DX = 0xE56D;
        // mul dx → DX:AX = AX * DX
        uint product = (uint)state * 0xE56DU;
        ushort axMul = (ushort)(product & 0xFFFFU);
        ushort dxMul = (ushort)(product >> 16);
        // inc ax
        ushort newState = (ushort)(axMul + 1);
        AX = newState;
        // mov [0xD824], ax
        UInt16[DS, 0xD824] = newState;
        // mov al, ah  →  AL becomes high byte of newState
        AL = (byte)(newState >> 8);
        // mov ah, dl  →  AH becomes low byte of DX (high-word of mul before inc applied)
        AH = (byte)(dxMul & 0xFF);
        // and ax, bx
        AX = (ushort)(AX & BX);
        // pop dx
        DX = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x8AC3 — publish AL to <c>ds[0x47E0]</c> and clear bit 4 of <c>ds[0x47DE]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (9 bytes):
    /// <code>
    /// 8AC3: A2 E0 47          mov [0x47E0], al
    /// 8AC6: 80 26 DE 47 EF    and byte [0x47DE], 0xEF
    /// 8ACB: C3                ret
    /// </code>
    /// Pure leaf. Mirror of <see cref="Guard47DEBit4_1000_8A3B_018A3B"/> in reverse —
    /// clears the same bit that 8A3B tests.
    /// </remarks>
    public System.Action Set47E0Clear47DEBit4_1000_8AC3_018AC3(int gotoAddress) {
        UInt8[DS, 0x47E0] = AL;
        byte v = UInt8[DS, 0x47DE];
        UInt8[DS, 0x47DE] = (byte)(v & 0xEF);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xD082 — installs a pair of far/near pointers at fixed slots.
    /// </summary>
    /// <remarks>
    /// Asm (13 bytes):
    /// <code>
    /// D082: C7 06 18 25 FF D0    mov word [0x2518], 0xD0FF
    /// D088: C7 06 A0 47 EC CE    mov word [0x47A0], 0xCEEC
    /// D08E: C3                    ret
    /// </code>
    /// Pure leaf. <c>[0x2518]</c> is an indirect call target slot used elsewhere
    /// (multiple cs1 functions write different values here — D082, D086, D08F, etc.);
    /// <c>[0x47A0]</c> is a near pointer into the engine state.
    /// </remarks>
    public System.Action InstallFarPtr2518Reset_1000_D082_01D082(int gotoAddress) {
        UInt16[DS, 0x2518] = 0xD0FF;
        UInt16[DS, 0x47A0] = 0xCEEC;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xE283 — register-restore exit pair for <c>cs1:0xE270</c>'s
    /// callee-saved-regs stack convention.
    /// </summary>
    /// <remarks>
    /// Asm (13 bytes):
    /// <code>
    /// E283: 58           pop ax
    /// E284: 8B EC        mov bp, sp
    /// E286: 87 46 0C     xchg ax, [bp+0x0C]
    /// E289: 5D           pop bp
    /// E28A: 5F           pop di
    /// E28B: 5E           pop si
    /// E28C: 5A           pop dx
    /// E28D: 59           pop cx
    /// E28E: 5B           pop bx
    /// E28F: C3           ret
    /// </code>
    /// The byte-faithful port. Caller-context semantics (paired with the
    /// <c>cs1:0xE270</c> save-regs trampoline) are documented in Plans/09 task #11;
    /// any future port of E270 must remain consistent with this exit pair. Pure leaf.
    /// </remarks>
    /// <summary>
    /// Override for cs1:0xE270 — save-pair of the callee-saved-regs convention; pairs
    /// with <see cref="RestoreCalleeSavedRegisters_1000_E283_01E283"/>.
    /// </summary>
    /// <remarks>
    /// Asm (19 bytes):
    /// <code>
    /// E270: 53           push bx
    /// E271: 51           push cx
    /// E272: 52           push dx
    /// E273: 56           push si
    /// E274: 57           push di
    /// E275: 55           push bp
    /// E276: 8B EC        mov bp, sp
    /// E278: 87 46 0C     xchg ax, [bp+0x0C]
    /// E27B: 50           push ax
    /// E27C: 8B 46 0C     mov ax, [bp+0x0C]
    /// E27F: 8B 6E 00     mov bp, [bp+0]
    /// E282: C3           ret
    /// </code>
    /// Byte-faithful port. The asm leaves the caller's stack 14 bytes "below" where the
    /// caller's call-pop sequence would normally restore it — the 6 saved registers and
    /// a swapped AX slot occupy that space. The matching <c>cs1:0xE283</c> drains them on
    /// the corresponding exit. Both halves are now C#, so the pair maintains the asm's
    /// exact memory and SP semantics.
    /// </remarks>
    public System.Action SaveCalleeSavedRegisters_1000_E270_01E270(int gotoAddress) {
        // push bx; push cx; push dx; push si; push di; push bp
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BX;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = CX;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DX;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DI;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BP;
        // mov bp, sp
        BP = SP;
        // xchg ax, [bp+0x0C]
        ushort tmp = UInt16[SS, (ushort)(BP + 0x0C)];
        UInt16[SS, (ushort)(BP + 0x0C)] = AX;
        AX = tmp;
        // push ax
        SP = (ushort)(SP - 2); UInt16[SS, SP] = AX;
        // mov ax, [bp+0x0C]
        AX = UInt16[SS, (ushort)(BP + 0x0C)];
        // mov bp, [bp+0]
        BP = UInt16[SS, BP];
        return NearRet();
    }

    public System.Action RestoreCalleeSavedRegisters_1000_E283_01E283(int gotoAddress) {
        // pop ax
        AX = UInt16[SS, SP]; SP = (ushort)(SP + 2);
        // mov bp, sp
        BP = SP;
        // xchg ax, [bp+0x0C]
        ushort temp = UInt16[SS, (ushort)(BP + 0x0C)];
        UInt16[SS, (ushort)(BP + 0x0C)] = AX;
        AX = temp;
        // pop bp; pop di; pop si; pop dx; pop cx; pop bx
        BP = UInt16[SS, SP]; SP = (ushort)(SP + 2);
        DI = UInt16[SS, SP]; SP = (ushort)(SP + 2);
        SI = UInt16[SS, SP]; SP = (ushort)(SP + 2);
        DX = UInt16[SS, SP]; SP = (ushort)(SP + 2);
        CX = UInt16[SS, SP]; SP = (ushort)(SP + 2);
        BX = UInt16[SS, SP]; SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x5D36 — predicate that returns CF reflecting a 2-condition test on the record at <c>ds:DI</c>.
    /// </summary>
    /// <remarks>
    /// Asm (14 bytes):
    /// <code>
    /// 5D36: 80 7D 08 28      cmp byte [di+0x08], 0x28
    /// 5D3A: 72 07            jc 5D43
    /// 5D3C: F6 45 0A 08      test byte [di+0x0A], 0x08
    /// 5D40: 74 01            jz 5D43
    /// 5D42: F9               stc
    /// 5D43: C3               ret
    /// </code>
    /// Returns CF=1 when <c>[di+8] &lt; 0x28</c> OR (<c>[di+8] ≥ 0x28</c> AND <c>[di+A] &amp; 0x08 != 0</c>);
    /// CF=0 otherwise. Pure leaf.
    /// </remarks>
    public System.Action ScoreThresholdFlag_1000_5D36_015D36(int gotoAddress) {
        byte v8 = UInt8[DS, (ushort)(DI + 0x08)];
        if (v8 < 0x28) {
            // cmp set CF=1; jc jumps; CF=1 preserved at ret
            CarryFlag = true;
            return NearRet();
        }
        // cmp set CF=0; jc not taken
        byte vA = UInt8[DS, (ushort)(DI + 0x0A)];
        if ((vA & 0x08) == 0) {
            // test clears CF; jz jumps; CF=0 at ret
            CarryFlag = false;
            return NearRet();
        }
        // test clears CF; jz not taken; stc sets CF=1
        CarryFlag = true;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x6917 — searches the 17-byte-stride table at <c>ds:0x3CAF</c>
    /// for a record whose word at <c>[+0x0A]</c> matches <c>SI</c>, skipping records with
    /// bit 6 set in <c>[+0x0C]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (~32 bytes, 6917..6938):
    /// <code>
    /// 6917: 80 3E EB 46 00    cmp byte [0x46EB], 0
    /// 691C: BF AF 3C          mov di, 0x3CAF
    /// 691F: 79 17              jns 6938                ; high-bit clear → tail at 6938
    /// 6921: 8B 0E BE 3C       mov cx, [0x3CBE]
    /// 6925: E3 11              jcxz 6938
    /// 6927: 83 C7 11          add di, 0x11
    /// 692A: 39 75 0A          cmp [di+0x0A], si
    /// 692D: E0 F8              loopne 6927
    /// 692F: 75 06              jnz 6937
    /// 6931: F6 45 0C 40       test byte [di+0x0C], 0x40
    /// 6935: 75 EE              jnz 6927
    /// 6937: C3                 ret
    /// 6938: 0B FF              or di, di
    /// 693A: C3                 ret
    /// </code>
    /// Pure leaf. The "or di, di" at 6938 is solely a flag-setter (DI is fixed at 0x3CAF
    /// at this point — non-zero, so ZF=0); we mirror the flag side effect.
    /// </remarks>
    public System.Action FindRecordByKey3CAF_1000_6917_016917(int gotoAddress) {
        byte v46EB = (byte)globalsOnDs.Get1138_46EB_Byte8();
        DI = 0x3CAF;
        if ((v46EB & 0x80) == 0) {
            // jns 6938: or di, di; ret. DI non-zero → ZF=0.
            ZeroFlag = false;
            return NearRet();
        }
        ushort cx = globalsOnDs.Get1138_3CBE_Word16();
        CX = cx;
        if (cx == 0) {
            // jcxz 6938
            ZeroFlag = false;
            return NearRet();
        }
        while (true) {
            // 6927: add di, 0x11; cmp [di+A], si; loopne 6927
            DI = (ushort)(DI + 0x11);
            ushort recKey = UInt16[DS, (ushort)(DI + 0x0A)];
            bool match = (recKey == SI);
            cx = (ushort)(cx - 1);
            CX = cx;
            if (!match) {
                if (cx != 0) continue;  // loopne taken
                // No match found within CX iterations — asm reaches ret at 6937 via
                // `jnz 6937` whose ZF came from the last `cmp` (mismatch → ZF=0).
                ZeroFlag = false;
                return NearRet();
            }
            // Match. Check bit 6 of [di+C]
            byte vC = UInt8[DS, (ushort)(DI + 0x0C)];
            if ((vC & 0x40) == 0) {
                // asm: `test [di+C], 0x40; jnz 6927` — bit clear → ZF=1, jnz NOT taken,
                // fall through to ret at 6937. ZF=1 signals "valid match" to callers.
                ZeroFlag = true;
                return NearRet();
            }
            // Bit set — jnz 6927 unconditionally; loop continues with current CX
            // (which may now be 0, causing loopne's CX-- to wrap to 0xFFFF — matching asm).
        }
    }

    /// <summary>
    /// Override for cs1:0x8E9E — phrase-template helper: emit a byte pair, then on
    /// non-zero AL do a divide-and-emit pattern.
    /// </summary>
    /// <remarks>
    /// Asm (44 bytes, 8E9E..8EC9):
    /// <code>
    /// 8E9E: 8B C2            mov ax, dx
    /// 8EA0: 32 E4            xor ah, ah        ; AX = DL (zero-extended)
    /// 8EA2: AB               stosw              ; [es:di] = AX; di += 2
    /// 8EA3: 00 06 8C 47      add [0x478C], al   ; bump counter
    /// 8EA7: 0B C0            or  ax, ax
    /// 8EA9: 74 1F            jz  8ECA           ; DL == 0 → tail-jump to next function
    /// 8EAB: 52               push dx
    /// 8EAC: 8B C3            mov ax, bx        ; AX = BX (dividend low)
    /// 8EAE: 8B DA            mov bx, dx
    /// 8EB0: 32 FF            xor bh, bh        ; BX = DL (divisor)
    /// 8EB2: 33 D2            xor dx, dx        ; DX = 0 (dividend high)
    /// 8EB4: 4B               dec bx
    /// 8EB5: 74 02            jz  8EB9           ; divisor was 1 → skip divide
    /// 8EB7: F7 F3            div bx             ; AX:DX = (DX:AX) / BX, remainder DX
    /// 8EB9: 05 06 00         add ax, 6
    /// 8EBC: AB               stosw              ; [es:di] = quotient + 6; di += 2
    /// 8EBD: 8B C2            mov ax, dx
    /// 8EBF: AB               stosw              ; [es:di] = remainder; di += 2
    /// 8EC0: 5A               pop dx
    /// 8EC1: FE C6            inc dh
    /// 8EC3: 32 D2            xor dl, dl
    /// 8EC5: 8B 1E 8F 47      mov bx, [0x478F]
    /// 8EC9: C3               ret
    /// </code>
    /// Pure leaf (within its body — but tail-jumps to <c>cs1:0x8ECA</c> when the input byte
    /// is zero). The asm <c>dec bx; jz</c> idiom skips the divide when the divisor is 1.
    /// </remarks>
    public System.Action EmitMassAndPair_1000_8E9E_018E9E(int gotoAddress) {
        AX = DX;
        AH = 0;
        // stosw
        UInt16[ES, DI] = AX;
        DI = (ushort)(DI + 2);
        // add [0x478C], al
        byte v478C = (byte)globalsOnDs.Get1138_478C_Byte8();
        globalsOnDs.Set1138_478C_Byte8((byte)(v478C + AL));
        // or ax, ax; jz 8ECA
        if (AX == 0) {
            return NearJump(0x8ECA);
        }
        // push dx
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = DX;
        AX = BX;
        BX = DX;
        BH = 0;
        DX = 0;
        BX = (ushort)(BX - 1);
        if (BX != 0) {
            // div bx
            uint dividend = ((uint)DX << 16) | AX;
            ushort quotient = (ushort)(dividend / BX);
            ushort remainder = (ushort)(dividend % BX);
            AX = quotient;
            DX = remainder;
        }
        // add ax, 6
        AX = (ushort)(AX + 6);
        // stosw (quotient + 6)
        UInt16[ES, DI] = AX;
        DI = (ushort)(DI + 2);
        AX = DX;
        // stosw (remainder)
        UInt16[ES, DI] = AX;
        DI = (ushort)(DI + 2);
        // pop dx
        DX = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        DH = (byte)(DH + 1);
        DL = 0;
        BX = (ushort)globalsOnDs.Get1138_478F_Word16();
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x9123 — computes a topic-slot byte from <c>AL</c> and writes to
    /// <c>ds[0x47D0]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (87 bytes, 9123..9179):
    /// <code>
    /// 9123: 3C 11             cmp al, 0x11
    /// 9125: 73 53             jnc 917A             ; AL ≥ 0x11 → tail-jump to next function
    /// 9127: 32 E4             xor ah, ah
    /// 9129: 3C 0D             cmp al, 0x0D
    /// 912B: 72 46             jc  9173             ; AL &lt; 0x0D → finalize with AH=0
    /// 912D: 75 0C             jnz 913B             ; AL != 0x0D → general path
    /// 912F: 8B 3E 4E 11       mov di, [0x114E]     ; AL == 0x0D path
    /// 9133: 8A 25             mov ah, [di]
    /// 9135: D0 EC             shr ah, 1
    /// 9137: FE C4             inc ah
    /// 9139: EB 38             jmp 9173
    /// 913B: 8B 36 56 47       mov si, [0x4756]     ; general path (0x0E..0x10)
    /// 913F: 3C 0E             cmp al, 0x0E
    /// 9141: 74 12             jz  9155
    /// 9143: 80 3E 2A 00 C8    cmp byte [0x002A], 0xC8
    /// 9148: 74 29             jz  9173             ; topic == 0xC8 → finalize
    /// 914A: A0 6C 47          mov al, [0x476C]
    /// 914D: D1 E0             shl ax, 1
    /// 914F: 8B F0             mov si, ax
    /// 9151: 8B B4 58 47       mov si, [si+0x4758]
    /// 9155: 8A 04             mov al, [si]         ; convergence — read byte at SI
    /// 9157: 52                push dx
    /// 9158: B2 03             mov dl, 3
    /// 915A: F6 F2             div dl                ; AL = AL/3, AH = AL%3
    /// 915C: B2 0F             mov dl, 0x0F
    /// 915E: 0A E4             or  ah, ah
    /// 9160: 74 02             jz  9164
    /// 9162: B2 11             mov dl, 0x11         ; if AH != 0 (remainder), use modulus 0x11
    /// 9164: 3A C2             cmp al, dl
    /// 9166: 72 04             jc  916C
    /// 9168: 2A C2             sub al, dl
    /// 916A: EB F8             jmp 9164             ; modular reduction
    /// 916C: 5A                pop dx
    /// 916D: 86 C4             xchg ah, al
    /// 916F: 04 0E             add al, 0x0E
    /// 9171: FE C4             inc ah
    /// 9173: 88 26 D0 47       mov [0x47D0], ah
    /// 9177: 32 E4             xor ah, ah
    /// 9179: C3                ret
    /// </code>
    /// Pure leaf (only out-edge is <c>NearJump(0x917A)</c> when AL ≥ 0x11). Modular
    /// reduction at 9164 is equivalent to <c>AL %= DL</c>.
    /// </remarks>
    public System.Action ComputeTopicSlotFromAL_1000_9123_019123(int gotoAddress) {
        if (AL >= 0x11) {
            return NearJump(0x917A);
        }
        AH = 0;
        byte al = AL;
        byte ahFinal;
        if (al < 0x0D) {
            // jc 9173: just write [0x47D0] = AH = 0
            ahFinal = 0;
        } else if (al == 0x0D) {
            // mov di, [0x114E]; mov ah, [di]; shr ah, 1; inc ah; jmp 9173
            ushort di = (ushort)globalsOnDs.Get1138_114E_Word16();
            DI = di;
            byte b = UInt8[DS, di];
            byte ah = (byte)(b >> 1);
            ah = (byte)(ah + 1);
            AH = ah;
            ahFinal = ah;
        } else {
            // General path 913B
            ushort si = (ushort)globalsOnDs.Get1138_4756_Word16();
            SI = si;
            if (al != 0x0E) {
                // cmp [0x002A], 0xC8; jz 9173
                byte topic = (byte)globalsOnDs.Get1138_002A_Byte8();
                if (topic == 0xC8) {
                    // finalize with AH=0 (we never modified AH past xor ah,ah)
                    ahFinal = 0;
                    goto finalize;
                }
                // AL = [0x476C]; shl ax, 1; SI = AX; SI = [SI + 0x4758]
                byte v476C = (byte)globalsOnDs.Get1138_476C_Byte8();
                AL = v476C;
                AX = (ushort)(AX << 1);  // AX-wide shift; AH was 0
                SI = AX;
                ushort tableBase = 0x4758;
                SI = UInt16[DS, (ushort)(SI + tableBase)];
            }
            // 9155: AL = [SI]; div dl; modular; xchg; add; inc
            byte siByte = UInt8[DS, SI];
            AL = siByte;
            AX = siByte;  // AH was 0 from earlier xor ah, ah
            // push dx
            SP = (ushort)(SP - 2);
            UInt16[SS, SP] = DX;
            DL = 3;
            // div dl: AX / DL → AL = quotient, AH = remainder
            byte quotient = (byte)(AX / 3);
            byte remainder = (byte)(AX % 3);
            AL = quotient;
            AH = remainder;
            // mov dl, 0xF; or ah, ah; jz 9164; mov dl, 0x11
            byte dl = 0x0F;
            if (remainder != 0) {
                dl = 0x11;
            }
            DL = dl;
            // Modular reduction: AL %= DL
            byte alMod = (byte)(AL % dl);
            AL = alMod;
            // pop dx
            DX = UInt16[SS, SP];
            SP = (ushort)(SP + 2);
            // xchg ah, al — AH/AL swap
            byte tmp = AL;
            AL = AH;
            AH = tmp;
            // add al, 0x0E
            AL = (byte)(AL + 0x0E);
            // inc ah
            AH = (byte)(AH + 1);
            ahFinal = AH;
        }
finalize:
        // 9173: mov [0x47D0], ah
        UInt8[DS, 0x47D0] = ahFinal;
        AH = 0;  // xor ah, ah
        return NearRet();
    }

    public System.Action OuterVmHelperPerScriptInit_1000_94F3_0194F3(int gotoAddress) {
        // cmp word [0x47C4], 0x10; jnc 9532
        ushort sceneId = (ushort)globalsOnDs.Get1138_47C4_Word16();
        if (sceneId >= 0x10) {
            return NearRet();
        }
        // push si — preserves caller's SI on SS:SP
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;
        // mov si, [0x47A2]
        SI = (ushort)globalsOnDs.Get1138_47A2_Word16();
        // mov al, [si+0x0F]; mov [0x0018], al
        byte flagByte = UInt8[DS, (ushort)(SI + 0x0F)];
        AL = flagByte;
        globalsOnDs.Set1138_0018_Byte8(flagByte);
        // test al, 0x40; (defer the jnz semantic — both branches read [si+8] / [si+0xA])
        ushort axCandidate;
        if ((flagByte & 0x40) != 0) {
            // jnz path → ax = [si+0x08]
            axCandidate = UInt16[DS, (ushort)(SI + 0x08)];
        } else {
            // fall-through → mov ax, [si+0x0A]
            axCandidate = UInt16[DS, (ushort)(SI + 0x0A)];
        }
        // sub ax, [0x0002]; neg ax — yields [0x0002] - axCandidate
        ushort gameTime = globalsOnDs.Get1138_0002_Word16_GameElapsedTime();
        ushort diff = (ushort)(axCandidate - gameTime);
        ushort negated = (ushort)(-(short)diff);
        AX = negated;
        UInt16[DS, 0x0016] = negated;
        // pop si — restore caller's SI
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        // cmp byte [0x002A], 0x64; jnc 9532
        byte topic = (byte)globalsOnDs.Get1138_002A_Byte8();
        if (topic >= 0x64) {
            return NearRet();
        }
        // cmp word [0x47C4], 0x09; jnc 9532
        if (sceneId >= 0x09) {
            return NearRet();
        }
        // mov di, [0x11DB]; or di, di; jz 9532
        ushort di = (ushort)globalsOnDs.Get1138_11DB_Word16();
        DI = di;
        if (di == 0) {
            return NearRet();
        }
        // call 0x2E98 — mirror the stack push of 0x9532 for memdiff parity
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x9532;
        // Invoke 0x2E98's C# port directly (same partial class). It runs, sets
        // DX/AX/flags + memory, returns a NearRet() Action that we discard — the
        // asm would have rolled back via its own ret pop, which now happens via
        // our manual pop here.
        OuterVmHelperRecordPointers_1000_2E98_012E98(0);
        // Pop the 0x9532 we just pushed (mirrors 0x2E98's ret popping it).
        SP = (ushort)(SP + 2);
        return NearRet();
    }
}
