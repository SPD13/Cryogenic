namespace Cryogenic.Overrides;

/// <summary>
/// Partial class containing dialogue system related overrides.
/// </summary>
/// <remarks>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly.
/// </remarks>
public partial class Overrides {
    /// <summary>
    /// Registers dialogue system function overrides with Spice86.
    /// </summary>
    public void DefineDialoguesCodeOverrides() {
        // Dialogue-VM verb dispatch table entries at cs1:0xA107 (Tech/16 §3).
        DefineFunction(cs1, 0xA1D0, DialogueVerb1SetSpeakDone_1000_A1D0_01A1D0);
        DefineFunction(cs1, 0xA1D6, DialogueVerb2ClearSpeakDone_1000_A1D6_01A1D6);
        DefineFunction(cs1, 0xA1DC, DialogueVerb7SetSpeakPause_1000_A1DC_01A1DC);
        DefineFunction(cs1, 0xA1E8, IncUnknown47A8_1000_A1E8_01A1E8);     // verb 6
        DefineFunction(cs1, 0xA1ED, DialogueVerb14IncC2_1000_A1ED_01A1ED);
        DefineFunction(cs1, 0xA235, DialogueVerb12NextHouse_1000_A235_01A235);
        DefineFunction(cs1, 0xA125, DialogueVerb8SceneDispatchA_1000_A125_01A125);
        DefineFunction(cs1, 0xA157, DialogueVerb9SceneDispatchB_1000_A157_01A157);
        DefineFunction(cs1, 0xA172, DialogueVerb15SceneDispatchC_1000_A172_01A172);
        DefineFunction(cs1, 0xA1F7, DialogueVerb3HousePhraseBranch_1000_A1F7_01A1F7);
        DefineFunction(cs1, 0xA219, DialogueVerb11Inc2AKick_1000_A219_01A219);
        DefineFunction(cs1, 0xA22A, DialogueVerb11Continuation_1000_A22A_01A22A);
        DefineFunction(cs1, 0xB17A, KickHelper_1000_B17A_01B17A);
        DefineFunction(cs1, 0xB186, KickHelperContinuation_1000_B186_01B186);
        DefineFunction(cs1, 0x96B5, ContextSavingOuterVmCall_1000_96B5_0196B5);
        DefineFunction(cs1, 0x96CF, ContextSavingOuterVmCallContinuation_1000_96CF_0196CF);
        DefineFunction(cs1, 0xA244, DialogueVerb4DialogueEndV0_1000_A244_01A244);
        DefineFunction(cs1, 0xA248, DialogueVerb5DialogueEndV1_1000_A248_01A248);
        DefineFunction(cs1, 0xA25B, DialogueVerb10LoadRecordPtr_1000_A25B_01A25B);
        DefineFunction(cs1, 0xA8B1, Unknown_1000_A8B1_01A8B1);
        DefineFunction(cs1, 0xC85B, InitDialogue_1000_C85B_01C85B);
    }

    /// <summary>
    /// Override for cs1:0xA1D0 — dialogue verb 1 (set_speak_done, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm: <c>C6 06 A5 47 FF C3</c> — <c>mov byte [0x47A5], 0xFF; ret</c>.
    /// Sets the speak-done UI flag to 0xFF.
    /// </remarks>
    public Action DialogueVerb1SetSpeakDone_1000_A1D0_01A1D0(int gotoAddress) {
        globalsOnDs.Set1138_47A5_Byte8(0xFF);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xA1D6 — dialogue verb 2 (clear_speak_done, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm: <c>C6 06 A5 47 00 C3</c> — <c>mov byte [0x47A5], 0x00; ret</c>.
    /// Clears the speak-done UI flag.
    /// </remarks>
    public Action DialogueVerb2ClearSpeakDone_1000_A1D6_01A1D6(int gotoAddress) {
        globalsOnDs.Set1138_47A5_Byte8(0x00);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xA1DC — dialogue verb 7 (set_speak_pause, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm: <c>C6 06 A5 47 80 C3</c> — <c>mov byte [0x47A5], 0x80; ret</c>.
    /// Intermediate "speak pause" state between set/clear.
    /// </remarks>
    public Action DialogueVerb7SetSpeakPause_1000_A1DC_01A1DC(int gotoAddress) {
        globalsOnDs.Set1138_47A5_Byte8(0x80);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xB17A — the "kick" helper (Tech/16 §3 / Tech/38).
    /// </summary>
    /// <remarks>
    /// Asm (17 bytes, B17A..B18A):
    /// <code>
    /// B17A: A0 C6 00       mov al, [0x00C6]
    /// B17D: 50             push ax
    /// B17E: 0C 80          or al, 0x80
    /// B180: A2 C6 00       mov [0x00C6], al
    /// B183: E8 2F E5       call 0x96B5
    /// B186: 58             pop ax            (kick-helper continuation)
    /// B187: A2 C6 00       mov [0x00C6], al
    /// B18A: C3             ret
    /// </code>
    /// Sets bit 7 of <c>ds[0x00C6]</c>, calls the context-saving outer-VM caller, then
    /// restores the original byte. <c>cs1:0x96B5</c> is the next layer down (now also
    /// ported, see <see cref="ContextSavingOuterVmCall_1000_96B5_0196B5"/>). The
    /// continuation at <c>0xB186</c> runs once <c>0x96B5</c>'s chain returns.
    /// </remarks>
    public Action KickHelper_1000_B17A_01B17A(int gotoAddress) {
        byte c6 = (byte)globalsOnDs.Get1138_00C6_Byte8();
        AL = c6;
        // push ax — preserves AH as well (matches asm push word)
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = AX;
        byte modified = (byte)(c6 | 0x80);
        AL = modified;
        UInt8[DS, 0x00C6] = modified;
        // Push B186 (continuation), then NearJump(0x96B5).
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xB186;
        return NearJump(0x96B5);
    }

    /// <summary>
    /// Override for cs1:0xB186 — kick helper continuation (post-<c>call 0x96B5</c> tail).
    /// </summary>
    /// <remarks>
    /// Asm: <c>58</c> pop ax; <c>A2 C6 00</c> mov [0x00C6], al; <c>C3</c> ret.
    /// </remarks>
    public Action KickHelperContinuation_1000_B186_01B186(int gotoAddress) {
        // pop ax — restores the AX we pushed in KickHelper.
        AX = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        UInt8[DS, 0x00C6] = AL;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x96B5 — context-saving outer-VM caller (Tech/38 helper).
    /// </summary>
    /// <remarks>
    /// Asm (35 bytes, 96B5..96D7):
    /// <code>
    /// 96B5: FF 36 C4 47        push word [0x47C4]   ; save scene_id
    /// 96B9: FF 36 C2 47        push word [0x47C2]   ; save context byte (saved as word)
    /// 96BD: C7 06 C4 47 10 00  mov word [0x47C4], 0x0010
    /// 96C3: C6 06 C2 47 80     mov byte [0x47C2], 0x80
    /// 96C8: 8B 36 84 AB        mov si, [0xAB84]
    /// 96CC: E8 CF 08           call 0x9F9E                    ← outer VM (still asm)
    /// 96CF: 8F 06 C2 47        pop word [0x47C2]              ← continuation
    /// 96D3: 8F 06 C4 47        pop word [0x47C4]
    /// 96D7: C3                 ret
    /// </code>
    /// Re-enters the outer dialogue VM at <c>0x9F9E</c> with a synthetic scene/context
    /// (<c>scene_id=0x10</c>, <c>ctx=0x80</c>). The outer VM remains asm; control returns
    /// to <see cref="ContextSavingOuterVmCallContinuation_1000_96CF_0196CF"/> via the
    /// pushed <c>0x96CF</c> on the stack.
    /// </remarks>
    public Action ContextSavingOuterVmCall_1000_96B5_0196B5(int gotoAddress) {
        // push word [0x47C4]
        ushort sceneId = (ushort)globalsOnDs.Get1138_47C4_Word16();
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = sceneId;
        // push word [0x47C2] — note the asm only writes the LOW byte later, but pushes
        // the word so the pop restores [0x47C3] verbatim.
        ushort ctx = (ushort)globalsOnDs.Get1138_47C2_Word16();
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = ctx;
        // mov word [0x47C4], 0x0010   (word write)
        UInt16[DS, 0x47C4] = 0x0010;
        // mov byte [0x47C2], 0x80     (byte write — high byte unchanged)
        UInt8[DS, 0x47C2] = 0x80;
        // mov si, [0xAB84]
        SI = (ushort)globalsOnDs.Get1138_AB84_Word16();
        // Push 0x96CF (continuation) then NearJump to outer VM at 0x9F9E.
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x96CF;
        return NearJump(0x9F9E);
    }

    /// <summary>
    /// Override for cs1:0x96CF — context-saving caller's continuation (Tech/38 helper).
    /// </summary>
    /// <remarks>
    /// Asm: <c>8F 06 C2 47</c> pop word [0x47C2]; <c>8F 06 C4 47</c> pop word [0x47C4];
    /// <c>C3</c> ret. Restores the saved scene-id and context word, then rets to the
    /// caller of <c>cs1:0x96B5</c>.
    /// </remarks>
    public Action ContextSavingOuterVmCallContinuation_1000_96CF_0196CF(int gotoAddress) {
        // pop word [0x47C2]
        ushort ctx = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        UInt16[DS, 0x47C2] = ctx;
        // pop word [0x47C4]
        ushort sceneId = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        UInt16[DS, 0x47C4] = sceneId;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xA219 — dialogue verb 11 (inc_2A_kick, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm:
    /// <code>
    /// A219: F6 04 80          test byte [si], 0x80
    /// A21C: 75 16              jnz A234 (ret)
    /// A21E: FE 06 2A 00        inc byte [0x002A]
    /// A222: C6 06 FF 00 00     mov byte [0x00FF], 0x00
    /// A227: E8 50 0F           call 0xB17A
    /// A22A: …                  (verb-11 continuation, see <see cref="DialogueVerb11Continuation_1000_A22A_01A22A"/>)
    /// </code>
    /// <c>cs1:0xB17A</c> is unported (it itself calls <c>cs1:0x96B5</c> which calls the
    /// still-asm outer VM at <c>cs1:0x9F9E</c>). Rather than stub it, this port stages the
    /// pre-call writes in C#, then arranges the stack so <c>0xB17A</c>'s eventual RET lands
    /// on <c>cs1:0xA22A</c> — which is itself a C# override that finishes verb 11's body.
    /// The asm at <c>0xB17A → 0x96B5 → 0x9F9E</c> runs unchanged.
    /// </remarks>
    public Action DialogueVerb11Inc2AKick_1000_A219_01A219(int gotoAddress) {
        byte runFlags = UInt8[DS, SI];
        if ((runFlags & 0x80) != 0) {
            return NearRet();
        }
        byte topic = (byte)globalsOnDs.Get1138_002A_Byte8();
        globalsOnDs.Set1138_002A_Byte8((byte)(topic + 1));
        UInt8[DS, 0x00FF] = 0;
        // Push A22A as the continuation address so 0xB17A's RET lands there.
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xA22A;
        return NearJump(0xB17A);
    }

    /// <summary>
    /// Override for cs1:0xA22A — verb 11 continuation (post-<c>call 0xB17A</c> tail).
    /// </summary>
    /// <remarks>
    /// Asm:
    /// <code>
    /// A22A: 80 3E 2A 00 01     cmp byte [0x002A], 0x01
    /// A22F: 75 03              jnz A234 (ret)
    /// A231: E8 D7 6D           call 0x100B
    /// A234: C3                 ret
    /// </code>
    /// Fires after <c>0xB17A</c>'s call chain returns (the asm pops the <c>A22A</c> we pushed
    /// in <see cref="DialogueVerb11Inc2AKick_1000_A219_01A219"/>). If the house counter just
    /// rolled to 1, fires <c>cs1:0x100B</c> (the "new_game_init" leaf — still asm, also
    /// observed by the dump-on-first-scene hook in <c>Overrides.cs</c>). The push of
    /// <c>A234</c> mimics the asm's <c>call 0x100B</c> exactly so that <c>0x100B</c>'s RET
    /// pops <c>A234</c>'s bare <c>C3</c>, which then rets to the outer VM caller — same
    /// instruction sequence the asm produces, byte-identical stack/memdiff.
    /// </remarks>
    public Action DialogueVerb11Continuation_1000_A22A_01A22A(int gotoAddress) {
        byte topic = (byte)globalsOnDs.Get1138_002A_Byte8();
        if (topic != 0x01) {
            return NearRet();
        }
        // Mimic `call 0x100B` exactly: push A234 (the asm's bare-ret byte) before the jump
        // so that 0x100B's RET pops A234, executes `C3`, and rets to the outer VM caller.
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xA234;
        return NearJump(0x100B);
    }

    /// <summary>
    /// Override for cs1:0xA244 — dialogue verb 4 (dialogue_end_v0, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm at A244 (shared body with verb 5 at A248):
    /// <code>
    /// A244: 32 C0              xor al, al
    /// A246: EB 02              jmp A24A
    /// A248: B0 01              mov al, 0x01     ; (verb 5 entry point)
    /// A24A: A2 6D 47           mov [0x476D], al
    /// A24D: C6 06 9F 00 00     mov byte [0x009F], 0x00
    /// A252: BD FE 1F           mov bp, 0x1FFE
    /// A255: BB 66 0F           mov bx, 0x0F66
    /// A258: E8 C8 30           call 0xD323
    /// </code>
    /// After the call to <c>0xD323</c>, control naturally falls into the next byte
    /// at <c>A25B</c> — which is the entry point of verb 10 (<c>load_record_ptr</c>).
    /// This is intentional engine code-sharing: verbs 4/5 execute their setup,
    /// invoke the dialogue-flush helper at <c>0xD323</c> (still asm), and then run
    /// verb 10's record-table walk before returning to the outer VM.
    ///
    /// <c>cs1:0xD323</c> is an unported ~256-byte multi-entry routine; rather than
    /// stub it, this port preserves the asm exactly. We arrange the stack so that
    /// <c>0xD323</c>'s RET lands on <c>A25B</c> (the verb-10 entry, which is itself
    /// a C# override). Sequence:
    ///   1. Stage <c>AL</c>, <c>[0x476D]</c>, <c>[0x009F]</c>, <c>BP</c>, <c>BX</c>.
    ///   2. Push <c>A25B</c> onto <c>SS:SP</c>.
    ///   3. <c>NearJump(0xD323)</c>.
    /// When <c>0xD323</c> finishes its internal call ladder (D316/D338/D280) and
    /// reaches the D410 tail (<c>mov bp,[0x21DA]; mov bp,[bp]; ret</c>), the ret pops
    /// <c>A25B</c> and Spice86 fires our verb-10 override. Verb 10 then rets to the
    /// outer VM's <c>A05D</c>, completing the call-chain identically to the asm.
    /// </remarks>
    public Action DialogueVerb4DialogueEndV0_1000_A244_01A244(int gotoAddress) {
        return DialogueEndShared(0);
    }

    /// <summary>
    /// Override for cs1:0xA248 — dialogue verb 5 (dialogue_end_v1, Tech/16 §3).
    /// Variant of verb 4: writes <c>1</c> instead of <c>0</c> to <c>ds[0x476D]</c>.
    /// </summary>
    public Action DialogueVerb5DialogueEndV1_1000_A248_01A248(int gotoAddress) {
        return DialogueEndShared(1);
    }

    /// <summary>
    /// Common body for verbs 4 and 5 (cs1:0xA24A..0xA25A). See <see cref="DialogueVerb4DialogueEndV0_1000_A244_01A244"/>
    /// for the rationale behind the stack arrangement.
    /// </summary>
    private Action DialogueEndShared(byte aHolds) {
        AL = aHolds;
        globalsOnDs.Set1138_476D_Byte8(aHolds);
        globalsOnDs.Set1138_009F_Byte8(0);
        BP = 0x1FFE;
        BX = 0x0F66;
        // Push A25B onto SS:SP so that 0xD323's eventual RET lands on the verb-10
        // entry — mirroring the asm's "call 0xD323; fall through to A25B" pattern
        // (where the call pushes A25B as the return address).
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xA25B;
        return NearJump(0xD323);
    }

    /// <summary>
    /// Override for cs1:0xA1F7 — dialogue verb 3 (house_phrase_branch, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm (34 bytes, A1F7..A218):
    /// <code>
    /// A1F7: 8A 1E 2A 00      mov bl, [0x002A]      ; BL = current_topic
    /// A1FB: B8 F8 12         mov ax, 0x12F8        ; default = early-game param block
    /// A1FE: 80 FB 14         cmp bl, 0x14
    /// A201: 72 13            jc  A216              ; bl &lt; 0x14 → keep early block, jmp
    /// A203: B8 4F 13         mov ax, 0x134F        ; mid-game block
    /// A206: 80 FB 18         cmp bl, 0x18
    /// A209: 72 0B            jc  A216              ; bl &lt; 0x18 → keep mid, jmp
    /// A20B: B8 70 13         mov ax, 0x1370        ; late-game block
    /// A20E: 80 FB 30         cmp bl, 0x30
    /// A211: 72 03            jc  A216              ; bl &lt; 0x30 → keep late, jmp
    /// A213: B8 DB 12         mov ax, 0x12DB        ; endgame block
    /// A216: E9 58 75         jmp 0x1771            ; tail-jump (= cs1:0x171F + 0x52)
    /// </code>
    /// Selects one of four scene-param block addresses based on the topic byte at
    /// <c>[0x002A]</c>, stages it in AX, and tail-jumps to <c>cs1:0x1771</c> (a callsite
    /// that hands AX to the scene-param dispatcher). Fully tail-call portable.
    /// </remarks>
    public Action DialogueVerb3HousePhraseBranch_1000_A1F7_01A1F7(int gotoAddress) {
        byte topic = (byte)globalsOnDs.Get1138_002A_Byte8();
        BL = topic;
        ushort block;
        if (topic < 0x14) {
            block = 0x12F8;
        } else if (topic < 0x18) {
            block = 0x134F;
        } else if (topic < 0x30) {
            block = 0x1370;
        } else {
            block = 0x12DB;
        }
        AX = block;
        return NearJump(0x1771);
    }

    /// <summary>
    /// Override for cs1:0xA25B — dialogue verb 10 (load_record_ptr, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm (51 bytes, A25B..A28D):
    /// <code>
    /// A25B: 56            push si
    /// A25C: 1E            push ds
    /// A25D: 07            pop es                ; ES = DS
    /// A25E: A1 80 47      mov ax, [0x4780]      ; AX = search key
    /// A261: BB 01 0A      mov bx, 0x0A01
    /// A264: BF 7C 19      mov di, 0x197C        ; record table base
    /// A267: 83 C7 08      add di, 0x08          ; advance to offset-8 field
    /// A26A: AF            scasw                  ; ax ?= [es:di]; di += 2
    /// A26B: 77 FA         ja A267                ; loop while ax &gt; record[8]
    /// A26D: 75 1D         jnz A28C               ; ax &lt; record[8] → bail (no match)
    /// A26F: 83 3D 38      cmp word [di], 0x0038  ; check next field
    /// A272: 75 02         jnz A276
    /// A274: B7 10         mov bh, 0x10           ; flag: "next-field == 0x38"
    /// A276: A0 D0 47      mov al, [0x47D0]
    /// A279: FE C8         dec al
    /// A27B: 78 07         js  A284               ; if AL was 0, skip the math
    /// A27D: D0 E0         shl al, 1
    /// A27F: 40            inc ax                 ; (16-bit; can carry into AH)
    /// A280: D0 E0         shl al, 1
    /// A282: 02 F8         add bh, al
    /// A284: 89 1E E1 47   mov [0x47E1], bx
    /// A288: 89 3E E4 47   mov [0x47E4], di
    /// A28C: 5E            pop si
    /// A28D: C3            ret
    /// </code>
    /// Walks the 28-record table at <c>ds:0x197C</c> (10-byte stride; search field at
    /// record offset 8) looking for a record whose [+8] word matches <c>ds[0x4780]</c>.
    /// On match, stores BX at <c>ds[0x47E1]</c> and DI at <c>ds[0x47E4]</c>. No external
    /// helpers. Stack push/pop emulated on <c>SS:SP</c> for memdiff parity.
    /// </remarks>
    public Action DialogueVerb10LoadRecordPtr_1000_A25B_01A25B(int gotoAddress) {
        // push si
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;
        // push ds; pop es  →  ES = DS, with the stack write/read happening on SS:SP
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = DS;
        ES = UInt16[SS, SP];
        SP = (ushort)(SP + 2);

        ushort ax = (ushort)globalsOnDs.Get1138_4780_Word16();
        AX = ax;
        ushort bx = 0x0A01;
        BX = bx;
        ushort di = 0x197C;
        DI = di;

        // Search loop. Each iteration: di += 8 (advance to next record's offset-8 field),
        // scasw (cmp ax,[es:di]; di+=2), ja → loop.
        while (true) {
            di = (ushort)(di + 0x08);
            ushort rec = UInt16[ES, di];
            di = (ushort)(di + 2);  // scasw post-increment
            DI = di;
            // ja taken when ax > rec (ZF=0 && CF=0)
            if (ax > rec) {
                continue;
            }
            // jnz taken when ax != rec (ZF=0): bail to A28C
            if (ax != rec) {
                // pop si; ret
                SI = UInt16[SS, SP];
                SP = (ushort)(SP + 2);
                return NearRet();
            }
            // Match found.
            break;
        }

        // A26F: cmp word [di], 0x0038
        ushort nextField = UInt16[ES, DI];
        if (nextField == 0x0038) {
            BH = 0x10;
        }

        // A276: AL = [0x47D0]; AL--
        byte al = (byte)((byte)globalsOnDs.Get1138_47D0_Byte8() - 1);
        AL = al;
        if ((sbyte)al >= 0) {
            // shl al, 1
            al = (byte)(al << 1);
            AL = al;
            // inc ax — full 16-bit increment; preserves AH except on AL=0xFF carry
            AX = (ushort)(AX + 1);
            al = AL;
            // shl al, 1
            al = (byte)(al << 1);
            AL = al;
            // add bh, al
            BH = (byte)(BH + al);
        }

        // A284: [0x47E1] = BX, [0x47E4] = DI  (use indexer; generated setters are byte-typed)
        UInt16[DS, 0x47E1] = BX;
        UInt16[DS, 0x47E4] = DI;

        // A28C: pop si; ret
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xA125 — dialogue verb 8 (scene_dispatch_a, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm (50 bytes, A125..A156):
    /// <code>
    /// A125: A1 C4 47           mov ax, [0x47C4]    ; AX = scene_id
    /// A128: 3D 01 00           cmp ax, 0x0001
    /// A12B: 74 59              jz  A186            ; scene 1: complex sub-handler
    /// A12D: 3D 03 00           cmp ax, 0x0003
    /// A130: 75 03              jnz A135
    /// A132: E9 04 81           jmp 0x2239          ; scene 3 tail-jump
    /// A135: 3D 05 00           cmp ax, 0x0005
    /// A138: 75 07              jnz A141
    /// A13A: C7 06 7E 22 CF 2C  mov word [0x227E], 0x2CCF
    /// A140: C3                 ret                 ; scene 5 done
    /// A141: 3D 0C 00           cmp ax, 0x000C
    /// A144: 75 08              jnz A14E
    /// A146: 8B 3E CE 11        mov di, [0x11CE]
    /// A14A: 80 65 0A 7F        and byte [di+0x0A], 0x7F
    ///                          (falls through to A14E)
    /// A14E: 3D 0D 00           cmp ax, 0x000D
    /// A151: 75 03              jnz A156
    /// A153: E9 32 82           jmp 0x2388          ; scene 13 tail-jump
    /// A156: C3                 ret                 ; default / scene 12 / scene 13-miss
    /// </code>
    /// The scene-1 sub-handler at <c>cs1:0xA186</c> calls <c>cs1:0x6F78</c> twice — that
    /// helper is unported, so the scene-1 case stays as a tail-jump into the asm sub-handler.
    /// All other scenes are fully expressible in C# (mutation + ret, or tail-jump).
    /// </remarks>
    public Action DialogueVerb8SceneDispatchA_1000_A125_01A125(int gotoAddress) {
        ushort sceneId = (ushort)globalsOnDs.Get1138_47C4_Word16();
        AX = sceneId;
        switch (sceneId) {
            case 0x0001:
                return NearJump(0xA186);  // scene 1: asm sub-handler (calls unported 0x6F78)
            case 0x0003:
                return NearJump(0x2239);  // scene 3 tail-jump
            case 0x0005:
                // Generated Set1138_227E_Word16 takes byte (generator bug); use indexer directly.
                UInt16[DS, 0x227E] = 0x2CCF;
                return NearRet();
            case 0x000C: {
                ushort di = (ushort)globalsOnDs.Get1138_11CE_Word16();
                DI = di;
                byte v = UInt8[DS, (ushort)(di + 0x0A)];
                UInt8[DS, (ushort)(di + 0x0A)] = (byte)(v & 0x7F);
                return NearRet();
            }
            case 0x000D:
                return NearJump(0x2388);  // scene 13 tail-jump
            default:
                return NearRet();
        }
    }

    /// <summary>
    /// Override for cs1:0xA157 — dialogue verb 9 (scene_dispatch_b, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm (27 bytes, A157..A171):
    /// <code>
    /// A157: A1 C4 47   mov ax, [0x47C4]
    /// A15A: 3D 03 00   cmp ax, 0x0003
    /// A15D: 75 03      jnz A162
    /// A15F: E9 8C 83   jmp 0x24EE          ; scene 3
    /// A162: 3D 05 00   cmp ax, 0x0005
    /// A165: 75 03      jnz A16A
    /// A167: E9 C2 8B   jmp 0x2D2C          ; scene 5
    /// A16A: 3D 0D 00   cmp ax, 0x000D
    /// A16D: 75 E7      jnz A156 (ret)
    /// A16F: E9 A7 82   jmp 0x2419          ; scene 13
    /// </code>
    /// Entirely tail-jumps. Fully portable without any unported helper.
    /// </remarks>
    public Action DialogueVerb9SceneDispatchB_1000_A157_01A157(int gotoAddress) {
        ushort sceneId = (ushort)globalsOnDs.Get1138_47C4_Word16();
        AX = sceneId;
        switch (sceneId) {
            case 0x0003: return NearJump(0x24EE);
            case 0x0005: return NearJump(0x2D2C);
            case 0x000D: return NearJump(0x2419);
            default: return NearRet();
        }
    }

    /// <summary>
    /// Override for cs1:0xA172 — dialogue verb 15 (scene_dispatch_c, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm (20 bytes, A172..A185):
    /// <code>
    /// A172: A1 C4 47        mov ax, [0x47C4]
    /// A175: 3D 01 00        cmp ax, 0x0001
    /// A178: 75 04           jnz A17E
    /// A17A: FE 06 F5 00     inc byte [0x00F5]    ; scene 1: increment + fall through
    /// A17E: 3D 03 00        cmp ax, 0x0003
    /// A181: 75 D3           jnz A156 (ret)
    /// A183: E9 1D 83        jmp 0x24A3           ; scene 3
    /// </code>
    /// Scene 1 increments <c>[0x00F5]</c> then falls through to scene-3 check, which fails
    /// (scene_id is still 1 ≠ 3) and lands on the shared ret at A156. Scene 3 tail-jumps
    /// to 0x24A3. Other scenes return immediately. Fully leaf-portable.
    /// </remarks>
    public Action DialogueVerb15SceneDispatchC_1000_A172_01A172(int gotoAddress) {
        ushort sceneId = (ushort)globalsOnDs.Get1138_47C4_Word16();
        AX = sceneId;
        if (sceneId == 0x0001) {
            byte v = (byte)globalsOnDs.Get1138_00F5_Byte8();
            globalsOnDs.Set1138_00F5_Byte8((byte)(v + 1));
            return NearRet();
        }
        if (sceneId == 0x0003) {
            return NearJump(0x24A3);
        }
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xA235 — dialogue verb 12 (2A_next_house, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm:
    /// <code>
    /// A235: F6 04 80     test byte [si], 0x80   ; SI = current dialogue record
    /// A238: 75 FA        jnz A234               ; A234 is a bare ret
    /// A23A: A0 2A 00     mov al, [0x002A]
    /// A23D: 24 FC        and al, 0xFC
    /// A23F: 04 04        add al, 0x04
    /// A241: E9 DB 6F     jmp 0x121F             ; near tail-jump
    /// </code>
    /// Advances house id to the next 4-aligned slot and tail-jumps into the engine's
    /// main dispatch at <c>cs1:0x121F</c>, which consumes AL. The write to
    /// <c>[0x002A]</c> itself happens inside <c>0x121F</c>, not here — the asm only
    /// stages AL. Run-flag bit 7 suppresses the verb entirely.
    /// </remarks>
    public Action DialogueVerb12NextHouse_1000_A235_01A235(int gotoAddress) {
        byte runFlags = UInt8[DS, SI];
        if ((runFlags & 0x80) != 0) {
            return NearRet();
        }
        byte current = (byte)globalsOnDs.Get1138_002A_Byte8();
        AL = (byte)((current & 0xFC) + 0x04);
        return NearJump(0x121F);
    }

    /// <summary>
    /// Override for cs1:0xA1ED — dialogue verb 14 (inc_C2, Tech/16 §3).
    /// </summary>
    /// <remarks>
    /// Asm:
    /// <code>
    /// A1ED: F6 04 80     test byte [si], 0x80   ; SI = current dialogue record
    /// A1F0: 75 42        jnz A234               ; A234 is a bare ret
    /// A1F2: FE 06 C2 00  inc byte [0x00C2]
    /// A1F6: C3           ret
    /// </code>
    /// Topic-visit counter increment, suppressed when the run-flag bit 7 of the
    /// originating record is set (Tech/16 §3: "side-effect suppression flag").
    /// </remarks>
    public Action DialogueVerb14IncC2_1000_A1ED_01A1ED(int gotoAddress) {
        byte runFlags = UInt8[DS, SI];
        if ((runFlags & 0x80) == 0) {
            byte c2 = (byte)globalsOnDs.Get1138_00C2_Byte8();
            globalsOnDs.Set1138_00C2_Byte8((byte)(c2 + 1));
        }
        return NearRet();
    }

    /// <summary>
    /// Override for CS1:A1E8 - Increments an unknown dialogue-related counter at DS:47A8.
    /// </summary>
    /// <param name="gotoAddress">Target address for potential jumps (unused in this override).</param>
    /// <returns>A near return action to exit the function.</returns>
    /// <remarks>
    /// Called during dialogues, sometimes before the first text display and sometimes before
    /// the last text. The exact purpose of this counter is not yet fully understood.
    /// </remarks>
    public Action IncUnknown47A8_1000_A1E8_01A1E8(int gotoAddress) {
        // Called in dialogues, sometimes before first text display, sometimes before last text
        globalsOnDs.Set1138_47A8_Byte8((byte)(globalsOnDs.Get1138_47A8_Byte8() + 1));
        return NearRet();
    }

    /// <summary>
    /// Override for CS1:C85B - Initializes dialogue state variables.
    /// </summary>
    /// <param name="gotoAddress">Target address for potential jumps (unused in this override).</param>
    /// <returns>A near return action to exit the function.</returns>
    /// <remarks>
    /// Sets up initial dialogue state by copying a video-related index and configuring
    /// the time between face zoom animations (0x1770). This prepares the dialogue system
    /// for character conversations with animated portrait zooms.
    /// </remarks>
    public Action InitDialogue_1000_C85B_01C85B(int gotoAddress) {
        LogDialogueInitEntry();
        ushort value = this.globalsOnDs.Get1138_CE7A_Word16_VideoPlayRelatedIndex();
        this.globalsOnDs.Set1138_476E_Word16(value);
        this.globalsOnDs.Set1138_4772_Word16_TimeBetweenFaceZooms(0x1770);
        return NearRet();
    }

    /// <summary>
    /// Override for CS1:A8B1 - Performs a character conversion operation (possibly hex digit formatting).
    /// </summary>
    /// <param name="gotoAddress">Target address for potential jumps (unused in this override).</param>
    /// <returns>A near return action to exit the function.</returns>
    /// <remarks>
    /// Called when dialogue text changes (at the beginning and during dialogue), and when
    /// entering an ornithopter. Converts the lower 4 bits of AL to an ASCII character,
    /// likely for hex digit display (0-9, A-F). The resulting value doesn't appear to
    /// have a significant effect on gameplay.
    /// </remarks>
    public Action Unknown_1000_A8B1_01A8B1(int gotoAddress) {
        // Called when a dialogue text changes (beginning and during dialogue), and when entering an orni
        // Value does not seem to have any effect
        byte value = AL;
        value &= 0xF;
        value += 0x30;
        if (value > 0x39) {
            value += 0x7;
        }

        AL = value;
        return NearRet();
    }
}