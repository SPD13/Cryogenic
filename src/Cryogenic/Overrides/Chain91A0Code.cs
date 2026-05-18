namespace Cryogenic.Overrides;

using System;

/// <summary>
/// Partial class for the <c>cs1:0x91A0</c> (<c>sub_B070</c>) deep-chain
/// campaign — a bottom-up, byte-faithful port of the 9-node mutually-recursive
/// orchestrator subtree reached from the outer dialogue VM. Port order is the
/// topological order of the dependency tree (see <c>tools/chain_tree.py</c>):
/// CA96, C871, F08B, F35A, F4ED, C675, BA5B, B782, B070.
/// </summary>
/// <remarks>
/// Every method is a faithful reproduction of its asm, cross-verified against
/// the authoritative <c>DNCDPRG.ASM</c> (offset rule cs1 = ASM − 0x1ED0) and
/// byte-verified against <c>research/asm-nav/cs1.bin</c>. No stubs, no guesses.
/// Method names embed segment/offset/linear for disassembly traceability.
/// </remarks>
public partial class Overrides {
    /// <summary>Registers the 0x91A0 deep-chain campaign overrides with Spice86.</summary>
    public void DefineChain91A0CodeOverrides() {
        DefineFunction(cs1, 0xABC6, ClearDc2B_1000_ABC6_01ABC6);
        DefineFunction(cs1, 0xA9A1, CloseFileHandle3821_1000_A9A1_01A9A1);
        DefineFunction(cs1, 0xD1BB, GlyphStreamProcessor_1000_D1BB_01D1BB);
        DefineFunction(cs1, 0xD48A, RenderTextLineSetup_1000_D48A_01D48A);
        DefineFunction(cs1, 0xD61D, RefreshOnMenuTypeChange_1000_D61D_01D61D);
        DefineFunction(cs1, 0xA7A5, ResetAndRefresh_1000_A7A5_01A7A5);
        DefineFunction(cs1, 0x9B8B, TeardownDialogueState_1000_9B8B_019B8B);
        DefineFunction(cs1, 0x9B8E, TeardownDialogueStateCont_1000_9B8E_019B8E);
        DefineFunction(cs1, 0x98B2, ResetRenderStateThenChain_1000_98B2_0198B2);
        DefineFunction(cs1, 0x91A0, ComputeDialogueModeThenSetup_1000_91A0_0191A0);
    }

    /// <summary>
    /// cs1:0xABC6 — <c>sub_CA96</c>. Leaf: <c>mov byte ds:0DC2Bh,0; retn</c>.
    /// Byte-verified vs cs1.bin@0xABC6: <c>C6 06 2B DC 00 C3</c>.
    /// </summary>
    public Action ClearDc2B_1000_ABC6_01ABC6(int gotoAddress) {
        UInt8[DS, 0xDC2B] = 0;   // mov byte ptr ds:0DC2Bh, 0
        return NearRet();        // retn
    }

    /// <summary>
    /// cs1:0xA9A1 — <c>sub_C871</c>. Takes the file handle stashed at
    /// <c>ds:0x3821</c> (atomically clearing the slot), and if it is non-zero
    /// and not the shared DUNE.DAT handle <c>ds:0xDBBA</c>, closes it
    /// (<c>int 21h</c>/AH=3Eh → <see cref="DosFileManager.CloseFileOrDevice"/>).
    /// Always returns CF=0 (<c>clc</c>); AX is preserved across the close
    /// exactly as the asm's <c>push ax/pop ax</c> requires.
    /// </summary>
    /// <remarks>
    /// Asm (24 B), byte-verified vs cs1.bin@0xA9A1
    /// (<c>33 DB 87 1E 21 38 0B DB 74 0C 3B 1E BA DB 74 06 50 B4 3E CD 21 58 F8 C3</c>):
    /// <code>
    /// A9A1: 33 DB          xor bx, bx
    /// A9A3: 87 1E 21 38    xchg bx, ds:3821h        ; bx=old[0x3821]; [0x3821]=0
    /// A9A7: 0B DB          or  bx, bx
    /// A9A9: 74 0C          jz  loc_C887 (0xA9B7)
    /// A9AB: 3B 1E BA DB    cmp bx, ds:0DBBAh
    /// A9AF: 74 06          jz  loc_C887 (0xA9B7)
    /// A9B1: 50             push ax
    /// A9B2: B4 3E          mov ah, 3Eh
    /// A9B4: CD 21          int 21h                  ; DOS close, BX=handle
    /// A9B6: 58             pop ax
    /// A9B7: F8  loc_C887   clc
    /// A9B8: C3  locret     retn
    /// </code>
    /// </remarks>
    public Action CloseFileHandle3821_1000_A9A1_01A9A1(int gotoAddress) {
        ushort bx = UInt16[DS, 0x3821];        // xor bx,bx ; xchg bx,[0x3821]
        UInt16[DS, 0x3821] = 0;
        BX = bx;
        if (bx != 0 && bx != UInt16[DS, 0xDBBA]) {   // or bx,bx/jz ; cmp bx,[0xDBBA]/jz
            ushort axSave = AX;                       // push ax
            Machine.Dos.FileManager.CloseFileOrDevice(bx);   // mov ah,3Eh ; int 21h
            AX = axSave;                              // pop ax
        }
        CarryFlag = false;   // clc
        return NearRet();     // retn
    }

    /// <summary>
    /// cs1:0xD1BB — <c>sub_F08B</c>. Glyph/escape stream processor loop:
    /// reads bytes from <c>es:si</c> (<c>lods es:[si]</c>) and per byte —
    /// <c>0xFF</c> ends (returns), <c>0x0D</c> does the carriage-return
    /// line-advance bookkeeping on <c>[0xD82C..0xD832]</c>, otherwise
    /// maps high-bit bytes to <c>'@'</c> (0x40) and dispatches the char
    /// through the near callback slot <c>ds:[0x2518]</c> — then loops.
    /// </summary>
    /// <remarks>
    /// One C# invocation == one loop iteration; the asm's two
    /// <c>jmp short sub_F08B</c> loop-backs and the post-callback
    /// continuation all return to cs1:0xD1BB (this very override), so
    /// iteration is expressed as re-dispatch, never C# recursion. The
    /// near-indirect <c>call word ptr ds:2518h</c> is modelled with the
    /// established near-call-continuation idiom (push the raw-asm
    /// continuation IP 0xD1CF — which is <c>EB EA</c> = <c>jmp 0xD1BB</c> —
    /// then <see cref="NearJump"/> to <c>[0x2518]</c>; the callee's
    /// <c>retn</c> pops 0xD1CF, the raw jmp re-enters here). The
    /// <c>jz locret_F075</c> exit targets cs1:0xD1A5 which is a bare
    /// <c>C3</c>, hence modelled as <see cref="NearRet"/>.
    /// Asm (50 B), byte-verified vs cs1.bin@0xD1BB
    /// (<c>26 AC 3C FF 74 E4 3C 0D 74 0C 0A C0 79 02 B0 40 FF 16 18 25
    /// EB EA A1 30 D8 A3 2C D8 B8 0A 00 81 3E 18 25 2F D1 75 03 B8 07 00
    /// 01 06 32 D8 01 06 2E D8 EB CC</c>):
    /// <code>
    /// D1BB: 26 AC          lods es:[si]
    /// D1BD: 3C FF / 74 E4  cmp al,0FFh ; jz locret_F075 (0xD1A5 = retn)
    /// D1C1: 3C 0D / 74 0C  cmp al,0Dh  ; jz loc_F0A1 (0xD1D1)
    /// D1C5: 0A C0 / 79 02  or al,al    ; jns loc_F09B (0xD1CB)
    /// D1C9: B0 40          mov al,40h
    /// D1CB: FF 16 18 25    call word ptr ds:2518h     ; near indirect
    /// D1CF: EB EA          jmp sub_F08B (0xD1BB)        ; continuation
    /// D1D1: A1 30 D8       mov ax,[0xD830]              ; loc_F0A1
    /// D1D4: A3 2C D8       mov [0xD82C],ax
    /// D1D7: B8 0A 00       mov ax,0Ah
    /// D1DA: 81 3E 18 25 2F D1  cmp word [0x2518],0D12Fh
    /// D1E0: 75 03          jnz loc_F0B5 (0xD1E5)
    /// D1E2: B8 07 00       mov ax,7
    /// D1E5: 01 06 32 D8    add [0xD832],ax              ; loc_F0B5
    /// D1E9: 01 06 2E D8    add [0xD82E],ax
    /// D1ED: EB CC          jmp sub_F08B (0xD1BB)
    /// </code>
    /// </remarks>
    public Action GlyphStreamProcessor_1000_D1BB_01D1BB(int gotoAddress) {
        AL = UInt8[ES, SI];                 // lods byte ptr es:[si]
        SI = (ushort)(SI + 1);
        if (AL == 0xFF) {                   // cmp al,0FFh ; jz locret_F075 (0xD1A5=retn)
            return NearRet();
        }
        if (AL == 0x0D) {                   // cmp al,0Dh ; jz loc_F0A1
            ushort prev = UInt16[DS, 0xD830];   // mov ax,[0xD830]
            UInt16[DS, 0xD82C] = prev;          // mov [0xD82C],ax
            ushort ax = (UInt16[DS, 0x2518] == 0xD12F) ? (ushort)0x0007 : (ushort)0x000A;
            AX = ax;                             // mov ax,0Ah / (jnz) / mov ax,7
            UInt16[DS, 0xD832] = (ushort)(UInt16[DS, 0xD832] + ax);  // add [0xD832],ax
            UInt16[DS, 0xD82E] = (ushort)(UInt16[DS, 0xD82E] + ax);  // add [0xD82E],ax
            return NearJump(0xD1BB);             // jmp sub_F08B (next iteration)
        }
        if ((AL & 0x80) != 0) {             // or al,al ; jns loc_F09B ; mov al,40h
            AL = 0x40;
        }
        // loc_F09B: call word ptr ds:2518h  (near indirect, continuation @0xD1CF)
        ushort target = UInt16[DS, 0x2518];
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xD1CF;            // push near-return IP (raw `jmp 0xD1BB`)
        return NearJump(target);
    }

    /// <summary>
    /// cs1:0xD48A — <c>sub_F35A</c>. Text-line render setup. Ports the full
    /// head/compute/branch logic in C# — the framebuffer/font selection
    /// (C# <see cref="SetTextBufferAsActiveFrameBuffer_1000_C08E_01C08E"/>,
    /// <see cref="SetFrontBufferAsActiveFrameBuffer_1000_C07C_01C07C"/>,
    /// <see cref="SetFontToMenu_1000_D075_01D075"/>), the
    /// <c>di = cl*0x0E + 0x1B48</c> record address, the
    /// <see cref="StoreDxBxToD82CD832_1000_D04E_01D04E"/> call, and the
    /// <c>and si,0x3FFF</c> / <c>test ah,0x40</c> / <c>or ah,ah</c> /
    /// <c>xchg al,[0xDBE5]</c> glyph-mode branch — then <see cref="NearJump"/>s
    /// into the emulated call-tail (loc_F3AA cs1:0xD4DA, or loc_F3B9
    /// cs1:0xD4E9 on the <c>si&amp;0x3FFF==0</c> path).
    /// </summary>
    /// <remarks>
    /// The tail is delegated, not call-and-discarded, on purpose: it is four
    /// consecutive continuation-hazard boundaries —
    /// <c>call sub_EE40</c> (0xCF70 FetchSpriteRecord, has a
    /// <c>push 0xCF7B; NearJump(0xD00F)</c> path), the near-indirect
    /// <c>call word ptr ds:[0x2518]</c>, <c>call sub_F08B</c> (0xD1BB, the
    /// iteration-via-redispatch loop override), then the tail far-indirect
    /// <c>call dword ptr ds:[0x38DD]</c> — whose stack effects are only
    /// faithful when run through the real emulated stream (same proven
    /// technique as <c>sub_A77F</c>'s loc_A79A). The entry
    /// <c>push word ptr ds:[0xDBDA]</c> is done here on the emulated stack;
    /// the delegated tail's <c>pop word ptr ds:[0xDBDA]; retn</c> balances it.
    /// Asm (~128 B), byte-verified vs cs1.bin@0xD48A. Continuation map:
    /// EE40 call@0xD4DD, [0x2518]@0xD4E2, F08B call@0xD4E6, loc_F3B9=0xD4E9,
    /// far[0x38DD]@0xD506, pop/ret@0xD50A; loc_F3AA=0xD4DA.
    /// <code>
    /// D48A: FF 36 DA DB    push word ptr ds:0DBDAh
    /// D48E: E8 ..          call sub_DF5E (0xC08E)
    /// D491: 80 3E E6 DC 00 cmp byte ds:0DCE6h,0
    /// D496: 7E 03          jle loc_F36B
    /// D498: E8 ..          call sub_DF4C (0xC07C)
    /// D49B: E8 ..          call sub_EF45 (0xD075)        ; loc_F36B
    /// D49E: 8B F0          mov si,ax
    /// D4A0: B0 0E          mov al,0Eh
    /// D4A2: F6 E1          mul cl
    /// D4A4: 8B F8          mov di,ax
    /// D4A6: 81 C7 48 1B    add di,1B48h
    /// D4AA: 8B 5D 02       mov bx,[di+2]
    /// D4AD: 43             inc bx
    /// D4AE: BA 5D 00       mov dx,5Dh
    /// D4B1: E8 ..          call sub_EF1E (0xD04E)
    /// D4B4: C6 06 E5 DB F3 mov byte ds:0DBE5h,0F3h
    /// D4B9: 80 65 08 7F    and byte [di+8],7Fh
    /// D4BD: 8B C6          mov ax,si
    /// D4BF: 81 E6 FF 3F    and si,3FFFh
    /// D4C3: 74 24          jz loc_F3B9 (0xD4E9)
    /// D4C5: B0 F5          mov al,0F5h
    /// D4C7: F6 C4 40       test ah,40h
    /// D4CA: 75 0E          jnz loc_F3AA (0xD4DA)
    /// D4CC: 80 4D 08 80    or byte [di+8],80h
    /// D4D0: B0 FA          mov al,0FAh
    /// D4D2: 0A E4          or ah,ah
    /// D4D4: 79 04          jns loc_F3AA (0xD4DA)
    /// D4D6: 86 06 E5 DB    xchg al,ds:0DBE5h
    /// D4DA: loc_F3AA  (emulated tail begins: mov [0xDBE4],al; call EE40; ...)
    /// </code>
    /// </remarks>
    public Action RenderTextLineSetup_1000_D48A_01D48A(int gotoAddress) {
        SP = (ushort)(SP - 2);                        // push word ptr ds:0DBDAh
        UInt16[SS, SP] = UInt16[DS, 0xDBDA];
        SetTextBufferAsActiveFrameBuffer_1000_C08E_01C08E(0);   // call sub_DF5E
        if ((sbyte)UInt8[DS, 0xDCE6] > 0) {           // cmp byte [0xDCE6],0 ; jle loc_F36B
            SetFrontBufferAsActiveFrameBuffer_1000_C07C_01C07C(0);   // call sub_DF4C
        }
        SetFontToMenu_1000_D075_01D075(0);            // loc_F36B: call sub_EF45
        SI = AX;                                       // mov si,ax
        AL = 0x0E;                                     // mov al,0Eh
        AX = (ushort)(0x0E * CL);                      // mul cl  (AX = AL*CL)
        DI = (ushort)(AX + 0x1B48);                    // mov di,ax ; add di,1B48h
        BX = (ushort)(UInt16[DS, (ushort)(DI + 2)] + 1);   // mov bx,[di+2] ; inc bx
        DX = 0x005D;                                   // mov dx,5Dh
        StoreDxBxToD82CD832_1000_D04E_01D04E(0);       // call sub_EF1E
        UInt8[DS, 0xDBE5] = 0xF3;                       // mov byte [0xDBE5],0F3h
        UInt8[DS, (ushort)(DI + 8)] =
            (byte)(UInt8[DS, (ushort)(DI + 8)] & 0x7F); // and byte [di+8],7Fh
        ushort origSi = SI;                            // mov ax,si
        AX = origSi;
        SI = (ushort)(SI & 0x3FFF);                    // and si,3FFFh
        if (SI == 0) {                                 // jz loc_F3B9
            return NearJump(0xD4E9);
        }
        AL = 0xF5;                                     // mov al,0F5h
        if ((AH & 0x40) != 0) {                        // test ah,40h ; jnz loc_F3AA
            return NearJump(0xD4DA);
        }
        UInt8[DS, (ushort)(DI + 8)] =
            (byte)(UInt8[DS, (ushort)(DI + 8)] | 0x80); // or byte [di+8],80h
        AL = 0xFA;                                     // mov al,0FAh
        if ((AH & 0x80) == 0) {                        // or ah,ah ; jns loc_F3AA
            return NearJump(0xD4DA);
        }
        byte tmp = UInt8[DS, 0xDBE5];                  // xchg al,ds:0DBE5h
        UInt8[DS, 0xDBE5] = AL;
        AL = tmp;
        return NearJump(0xD4DA);                        // loc_F3AA (emulated tail)
    }

    /// <summary>
    /// cs1:0xD61D — <c>sub_F4ED</c>. On a menu-type change (the value from
    /// <see cref="SetBpToCurrentMenuTypeForScreenAction_1000_D41B_01D41B"/>
    /// differs from <c>[0x1F80]</c>) and when <c>bp == 0x1F7E</c>, runs the
    /// full screen-rebuild sequence; otherwise just the
    /// <see cref="PushAll_1000_E270_01E270"/>/<see cref="PopAll_1000_E283_01E283"/>
    /// bracket. Prologue, the two call-and-discard-safe (NearRet-inline) calls,
    /// the <c>cmp [0x1F80],ax / mov / jz / cmp bp,si / jnz</c> double-branch
    /// and the <c>loc_F519</c> PopAll+pop+ret tail are ported in C#; only the
    /// 3-consecutive-continuation-hazard cluster (<c>call sub_FA82</c> §A,
    /// <c>call sub_F35A</c> NearJump-delegating, <c>call sub_FABC</c> §A) is
    /// delegated to the emulated stream via <see cref="NearJump"/>(0xD636) —
    /// same proven exact technique as <c>sub_A77F</c>/<c>sub_F35A</c>. The
    /// <c>push ax</c> is on the emulated stack; the C# tail's
    /// <c>PopAll;pop ax;ret</c> (or the delegated path's identical
    /// loc_F519@0xD649) balances it.
    /// </summary>
    /// <remarks>
    /// Asm (49 B), byte-verified vs cs1.bin@0xD61D
    /// (<c>50 B8 9F 00 E8 4C 0C E8 F4 FD BE 7E 1F 39 44 02 89 44 02 74 17
    /// 3B EE 75 13 E8 79 05 33 C9 E8 16 FE E8 49 FE C6 06 E7 DC FF E8 A3 05
    /// E8 37 0C 58 C3</c>):
    /// <code>
    /// D61D: 50          push ax
    /// D61E: B8 9F 00    mov ax,9Fh
    /// D621: E8 ..       call sub_10140 (0xE270 PushAll)     ; loc_F4F1
    /// D624: E8 ..       call sub_F2EB  (0xD41B)             ; -> ax
    /// D627: BE 7E 1F    mov si,1F7Eh
    /// D62A: 39 44 02    cmp [si+2],ax
    /// D62D: 89 44 02    mov [si+2],ax
    /// D630: 74 17       jz  loc_F519 (0xD649)
    /// D632: 3B EE       cmp bp,si
    /// D634: 75 13       jnz loc_F519 (0xD649)
    /// D636: E8 ..       call sub_FA82 (0xDBB2 §A)  ─┐ hazard
    /// D639: 33 C9       xor cx,cx                    │ cluster
    /// D63B: E8 ..       call sub_F324 (0xD454)       │ (delegated
    /// D63E: E8 ..       call sub_F35A (0xD48A)       │  to asm)
    /// D641: C6 06 E7 DC FF mov byte [0xDCE7],0FFh    │
    /// D646: E8 ..       call sub_FABC (0xDBEC §A)  ─┘
    /// D649: E8 ..       call sub_10153 (0xE283 PopAll) ; loc_F519
    /// D64C: 58          pop ax
    /// D64D: C3          retn
    /// </code>
    /// </remarks>
    public Action RefreshOnMenuTypeChange_1000_D61D_01D61D(int gotoAddress) {
        SP = (ushort)(SP - 2);                         // push ax
        UInt16[SS, SP] = AX;
        AX = 0x009F;                                   // mov ax,9Fh
        PushAll_1000_E270_01E270(0);                    // call sub_10140  (NearRet-inline)
        SetBpToCurrentMenuTypeForScreenAction_1000_D41B_01D41B(0);  // call sub_F2EB -> ax
        SI = 0x1F7E;                                    // mov si,1F7Eh
        ushort slot = UInt16[DS, (ushort)(SI + 2)];     // cmp [si+2],ax
        bool equal = slot == AX;
        UInt16[DS, (ushort)(SI + 2)] = AX;              // mov [si+2],ax
        if (equal || BP != SI) {                        // jz loc_F519 ; cmp bp,si ; jnz loc_F519
            PopAll_1000_E283_01E283(0);                 // loc_F519: call sub_10153 (NearRet-inline)
            AX = UInt16[SS, SP];                        // pop ax
            SP = (ushort)(SP + 2);
            return NearRet();                            // retn
        }
        return NearJump(0xD636);                          // hazard cluster (emulated tail)
    }

    /// <summary>
    /// cs1:0xA7A5 — <c>sub_C675</c>. Removes the record at <c>si=0xA7C2</c>
    /// from its list (<see cref="ListRemoveRecordBySi_1000_DA5F_1DA5F"/>,
    /// NearRet-inline so call-and-discard-safe), clears <c>[0xDC26]</c>, then
    /// runs the screen-refresh / conditional-teardown tail
    /// (<c>call sub_F4ED; call sub_CA9C; jz locret_C658; call sub_CA96;
    /// call sub_C871; jmp sub_CCBD</c>). The non-call prologue is ported in
    /// C#; the call-chain tail is delegated to the emulated stream via
    /// <see cref="NearJump"/>(0xA7B1) because the very first tail call
    /// <c>sub_F4ED</c> (cs1:0xD61D) is itself a NearJump-delegating port whose
    /// effect is only faithful through a real emulated <c>call</c> (same
    /// proven technique as <c>sub_A77F</c>); the rest (CA9C's ZF result for
    /// <c>jz locret_C658</c>, CA96/C871 C# leaves, the <c>jmp sub_CCBD</c>
    /// §A tail) all dispatch correctly there.
    /// </summary>
    /// <remarks>
    /// Asm (10 instr), byte-verified vs cs1.bin@0xA7A5
    /// (<c>BE C2 A7 E8 B4 32 C7 06 26 DC 00 00 E8 69 2E E8 15 04 74 CF
    /// E8 0A 04 E8 E2 01 E9 2B 06</c>):
    /// <code>
    /// A7A5: BE C2 A7          mov si,0A7C2h
    /// A7A8: E8 B4 32          call sub_F92F (0xDA5F)
    /// A7AB: C7 06 26 DC 00 00 mov word ds:0DC26h,0
    /// A7B1: E8 69 2E          call sub_F4ED (0xD61D)        ; delegate from here
    /// A7B4: E8 15 04          call sub_CA9C (0xABCC)
    /// A7B7: 74 CF             jz locret_C658 (0xA788=retn)
    /// A7B9: E8 0A 04          call sub_CA96 (0xABC6)
    /// A7BC: E8 E2 01          call sub_C871 (0xA9A1)
    /// A7BF: E9 2B 06          jmp sub_CCBD (0xADED)
    /// </code>
    /// </remarks>
    public Action ResetAndRefresh_1000_A7A5_01A7A5(int gotoAddress) {
        SI = 0xA7C2;                              // mov si,0A7C2h
        ListRemoveRecordBySi_1000_DA5F_1DA5F(0);   // call sub_F92F (NearRet-inline)
        UInt16[DS, 0xDC26] = 0;                     // mov word ds:0DC26h,0
        return NearJump(0xA7B1);                     // call sub_F4ED onward (emulated tail)
    }

    /// <summary>
    /// cs1:0x9B8B — <c>sub_BA5B</c> entry. Models <c>call sub_C675</c>
    /// (cs1:0xA7A5, itself a NearJump-delegating port — so it must run through
    /// a real emulated call) with the established near-call-continuation
    /// idiom: push the post-call IP 0x9B8E and
    /// <see cref="NearJump"/>(0xA7A5). When the sub_C675 chain returns it
    /// pops 0x9B8E → <see cref="TeardownDialogueStateCont_1000_9B8E_019B8E"/>.
    /// </summary>
    /// <remarks>
    /// Asm: <c>9B8B: E8 17 0C  call sub_C675</c> (byte-verified
    /// vs cs1.bin@0x9B8B; <c>0x9B8E+0x0C17=0xA7A5</c>).
    /// </remarks>
    public Action TeardownDialogueState_1000_9B8B_019B8B(int gotoAddress) {
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x9B8E;     // push return IP (continuation)
        return NearJump(0xA7A5);     // call sub_C675
    }

    /// <summary>
    /// cs1:0x9B8E — continuation of <see cref="TeardownDialogueState_1000_9B8B_019B8B"/>
    /// after <c>sub_C675</c> returns. Pure-compute teardown: zero
    /// <c>[0x47C3]</c>/<c>[0x47CE]</c>, clear bit 7 of <c>[0x47D1]</c>,
    /// take-and-clear <c>[0x47C6]</c>; if it was zero return
    /// (<c>locret_BA7B</c>, 0x9BAB = <c>retn</c>), else <c>si=0x99BE</c> and
    /// tail-jump <c>sub_F92F</c> (cs1:0xDA5F, whose <c>retn</c> returns on
    /// this routine's behalf).
    /// </summary>
    /// <remarks>
    /// Asm (post-call body, byte-verified vs cs1.bin@0x9B8E:
    /// <c>33 C0 C6 06 C3 47 00 A3 CE 47 80 26 D1 47 7F 87 06 C6 47 0B C0
    /// 74 06 BE BE 99 E9 B4 3E</c>):
    /// <code>
    /// 9B8E: 33 C0          xor ax,ax
    /// 9B90: C6 06 C3 47 00 mov byte ds:47C3h,0
    /// 9B95: A3 CE 47       mov ds:47CEh,ax
    /// 9B98: 80 26 D1 47 7F and byte ds:47D1h,7Fh
    /// 9B9D: 87 06 C6 47    xchg ax,ds:47C6h
    /// 9BA1: 0B C0          or ax,ax
    /// 9BA3: 74 06          jz locret_BA7B (0x9BAB=retn)
    /// 9BA5: BE BE 99       mov si,99BEh
    /// 9BA8: E9 B4 3E       jmp sub_F92F (0xDA5F)
    /// </code>
    /// </remarks>
    public Action TeardownDialogueStateCont_1000_9B8E_019B8E(int gotoAddress) {
        AX = 0;                                         // xor ax,ax
        UInt8[DS, 0x47C3] = 0;                           // mov byte ds:47C3h,0
        UInt16[DS, 0x47CE] = 0;                          // mov ds:47CEh,ax (ax=0)
        UInt8[DS, 0x47D1] = (byte)(UInt8[DS, 0x47D1] & 0x7F); // and byte ds:47D1h,7Fh
        ushort prev = UInt16[DS, 0x47C6];                // xchg ax,ds:47C6h
        UInt16[DS, 0x47C6] = 0;                           //   (ax was 0)
        AX = prev;
        if (AX == 0) {                                   // or ax,ax ; jz locret_BA7B
            return NearRet();
        }
        SI = 0x99BE;                                     // mov si,99BEh
        return NearJump(0xDA5F);                          // jmp sub_F92F
    }

    /// <summary>
    /// cs1:0x98B2 — <c>sub_B782</c>. Early-out if <c>[0x47C3]!=0</c>; else
    /// resets the dialogue render state (<c>[0x4540]=0</c>, clears the top
    /// two bits of <c>[0x47D1]</c>, take-and-clear <c>[0x47C8]</c> — early-out
    /// if it was zero, <c>[0x1BF8]=0</c>, <c>[0x1C06]=0</c>) then runs the
    /// blit/rect/teardown chain (<c>call sub_E316; mov si,0x1BF0;
    /// call sub_E3C0; jmp sub_BA5B</c>). The whole head + both early returns
    /// (<c>locret_B7B5</c> 0x98E5 = <c>retn</c>) are ported in C#; the chain
    /// tail is delegated to the emulated stream via <see cref="NearJump"/>(0x98D9)
    /// because all three tail targets are continuation-unsafe to
    /// call-and-discard: <c>sub_E316</c> (0xC446) has a <c>FarJump</c> §A path,
    /// <c>sub_E3C0</c> (0xC4F0) returns <c>NearJump(0xC4FB)</c>, and
    /// <c>sub_BA5B</c> (0x9B8B) is a continuation-split port — all dispatch
    /// correctly when reached via the real emulated calls (sub_A77F technique).
    /// </summary>
    /// <remarks>
    /// Asm (20 instr), byte-verified vs cs1.bin@0x98B2
    /// (<c>80 3E C3 47 00 75 2C 33 C0 A3 40 45 80 26 D1 47 3F 87 06 C8 47
    /// 0B C0 74 1A BE F0 1B C7 44 08 00 00 C7 06 06 1C 00 00 E8 6A 2B
    /// BE F0 1B E8 0E 2C E9 A6 02 C3</c>):
    /// <code>
    /// 98B2: 80 3E C3 47 00 cmp byte ds:47C3h,0
    /// 98B7: 75 2C          jnz locret_B7B5 (0x98E5=retn)
    /// 98B9: 33 C0          xor ax,ax
    /// 98BB: A3 40 45       mov ds:4540h,ax
    /// 98BE: 80 26 D1 47 3F and byte ds:47D1h,3Fh
    /// 98C3: 87 06 C8 47    xchg ax,ds:47C8h
    /// 98C7: 0B C0          or ax,ax
    /// 98C9: 74 1A          jz locret_B7B5 (0x98E5)
    /// 98CB: BE F0 1B       mov si,1BF0h
    /// 98CE: C7 44 08 00 00 mov word [si+8],0
    /// 98D3: C7 06 06 1C 00 00 mov word ds:1C06h,0
    /// 98D9: E8 6A 2B       call sub_E316 (0xC446)   ; delegate from here
    /// 98DC: BE F0 1B       mov si,1BF0h
    /// 98DF: E8 0E 2C       call sub_E3C0 (0xC4F0)
    /// 98E2: E9 A6 02       jmp sub_BA5B (0x9B8B)
    /// 98E5: C3             retn                      ; locret_B7B5
    /// </code>
    /// </remarks>
    public Action ResetRenderStateThenChain_1000_98B2_0198B2(int gotoAddress) {
        if (UInt8[DS, 0x47C3] != 0) {                 // cmp byte [0x47C3],0 ; jnz locret_B7B5
            return NearRet();
        }
        AX = 0;                                        // xor ax,ax
        UInt16[DS, 0x4540] = 0;                         // mov ds:4540h,ax
        UInt8[DS, 0x47D1] = (byte)(UInt8[DS, 0x47D1] & 0x3F);   // and byte ds:47D1h,3Fh
        ushort prev = UInt16[DS, 0x47C8];              // xchg ax,ds:47C8h
        UInt16[DS, 0x47C8] = 0;                          //   (ax was 0)
        AX = prev;
        if (AX == 0) {                                  // or ax,ax ; jz locret_B7B5
            return NearRet();
        }
        SI = 0x1BF0;                                    // mov si,1BF0h
        UInt16[DS, (ushort)(SI + 8)] = 0;               // mov word [si+8],0
        UInt16[DS, 0x1C06] = 0;                          // mov word ds:1C06h,0
        return NearJump(0x98D9);                          // call sub_E316 onward (emulated tail)
    }

    /// <summary>
    /// cs1:0x91A0 — <c>sub_B070</c>, the campaign root. Computes the dialogue
    /// display mode into <c>[0x00F0]</c> (default 0; <c>0x0A</c> iff
    /// <c>ax==0x0C</c> <b>and</b> <c>[0x10A7] &amp; 0x10</c>), then enters
    /// <c>loc_B088</c> which resolves the topic slot
    /// (<c>call sub_AFF3</c>), conditionally tail-jumps <c>sub_B0DF</c>,
    /// rebuilds render state (<c>call sub_B782</c>, <c>call sub_B0DF</c>) and
    /// performs the <c>lds [0xDBB0]</c> record-pointer / <c>movsw×4</c> copy
    /// into <c>[0x1BF0]</c> with the <c>[0x47CC]/[0x47CA]/[0x47D2]</c> fixups.
    /// The mode-decision head is ported in C#; <c>loc_B088</c> onward is
    /// delegated to the emulated stream via <see cref="NearJump"/>(0x91B8) —
    /// every one of its three callees is continuation-unsafe to
    /// call-and-discard (<c>sub_AFF3</c>/0x9123 has a <c>NearJump(0x917A)</c>
    /// path, <c>sub_B782</c>/0x98B2 is a NearJump-delegating port,
    /// <c>sub_B0DF</c>/0x920F is an always-<c>NearJump(0xC13E)</c> thunk), so
    /// the only exact model is to run them through real emulated calls
    /// (uniform with the rest of this campaign and the sub_A77F precedent).
    /// </summary>
    /// <remarks>
    /// Asm head, byte-verified vs cs1.bin@0x91A0
    /// (<c>C7 06 F0 00 00 00 3D 0C 00 75 0D F6 06 A7 10 10 74 06
    /// C7 06 F0 00 0A 00 E8 68 FF ...</c>):
    /// <code>
    /// 91A0: C7 06 F0 00 00 00 mov word ds:0F0h,0
    /// 91A6: 3D 0C 00          cmp ax,0Ch
    /// 91A9: 75 0D             jnz loc_B088 (0x91B8)
    /// 91AB: F6 06 A7 10 10    test byte ds:10A7h,10h
    /// 91B0: 74 06             jz loc_B088 (0x91B8)
    /// 91B2: C7 06 F0 00 0A 00 mov word ds:0F0h,0Ah
    /// 91B8: E8 68 FF          call sub_AFF3 (0x9123)   ; loc_B088 (delegated)
    /// </code>
    /// </remarks>
    public Action ComputeDialogueModeThenSetup_1000_91A0_0191A0(int gotoAddress) {
        UInt16[DS, 0x00F0] = 0;                          // mov word ds:0F0h,0
        if (AX == 0x000C && (UInt8[DS, 0x10A7] & 0x10) != 0) {  // cmp ax,0Ch/jnz ; test [0x10A7],10h/jz
            UInt16[DS, 0x00F0] = 0x000A;                 // mov word ds:0F0h,0Ah
        }
        return NearJump(0x91B8);                          // loc_B088 (emulated tail)
    }
}
