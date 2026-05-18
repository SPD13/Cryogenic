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
}
