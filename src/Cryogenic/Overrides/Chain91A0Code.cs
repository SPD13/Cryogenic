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
}
