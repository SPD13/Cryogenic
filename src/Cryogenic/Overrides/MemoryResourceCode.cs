namespace Cryogenic.Overrides;

/// <summary>
/// Partial class containing memory-allocator and resource/command-line helper leaves
/// (Phase 36 — Tech/01, 02, 09).
/// </summary>
/// <remarks>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly.
/// </remarks>
public partial class Overrides {
    /// <summary>
    /// Registers memory/resource helper overrides with Spice86.
    /// </summary>
    public void DefineMemoryResourceCodeOverrides() {
        DefineFunction(cs1, 0xE56B, ParseCmdIsEndOfArg_1000_E56B_01E56B);
        DefineFunction(cs1, 0xF2FC, StrcpyToFilenameBuf_1000_F2FC_01F2FC);
        DefineFunction(cs1, 0xF0FF, BumpAllocate_1000_F0FF_01F0FF);
    }

    /// <summary>
    /// Override for cs1:0xE56B — <c>parse_cmd_is_end_of_arg_ida</c>. Reads the next
    /// command-line char (uppercased if a lowercase letter, or 0x0D when at the buffer
    /// end SI&gt;=BP) and returns ZF=1 if it is a space (0x20).
    /// </summary>
    /// <remarks>
    /// Asm (16 bytes):
    /// <code>
    /// E56B: B0 0D       mov al, 0x0D
    /// E56D: 3B F5       cmp si, bp
    /// E56F: 73 07       jnc E578
    /// E571: AC          lodsb
    /// E572: 3C 61       cmp al, 0x61
    /// E574: 72 02       jc  E578
    /// E576: 24 DF       and al, 0xDF
    /// E578: 3C 20       cmp al, 0x20
    /// E57A: C3          ret
    /// </code>
    /// Pure leaf.
    /// </remarks>
    public System.Action ParseCmdIsEndOfArg_1000_E56B_01E56B(int gotoAddress) {
        byte al = 0x0D;
        AL = al;
        // cmp si, bp; jnc E578 — skip lodsb if SI >= BP (unsigned)
        if (SI < BP) {
            al = UInt8[DS, SI];
            SI = (ushort)(SI + 1);
            AL = al;
            // cmp al, 0x61; jc E578 — uppercase only when al >= 'a'
            if (al >= 0x61) {
                al = (byte)(al & 0xDF);
                AL = al;
            }
        }
        // cmp al, 0x20 — set flags for caller
        ZeroFlag = (al == 0x20);
        CarryFlag = al < 0x20;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xF2FC — <c>strcpy_to_filename_buf_ida</c>. Copies the
    /// NUL-terminated string at <c>ds:DX</c> into the filename buffer at
    /// <c>ds:[0x38A6]</c>; returns <c>DX = 0x3826</c>.
    /// </summary>
    /// <remarks>
    /// Asm (24 bytes, F2FC..F313):
    /// <code>
    /// F2FC: 56            push si
    /// F2FD: 57            push di
    /// F2FE: 8B F2         mov si, dx
    /// F300: 8B 3E A6 38   mov di, [0x38A6]
    /// F304: 8A 04         mov al, [si]
    /// F306: 46            inc si
    /// F307: 88 05         mov [di], al
    /// F309: 47            inc di
    /// F30A: 0A C0         or  al, al
    /// F30C: 75 F6         jnz F304
    /// F30E: 5F            pop di
    /// F30F: 5E            pop si
    /// F310: BA 26 38      mov dx, 0x3826
    /// F313: C3            ret
    /// </code>
    /// Pure leaf. The terminating NUL is copied too (loop tests AL after the store).
    /// </remarks>
    public System.Action StrcpyToFilenameBuf_1000_F2FC_01F2FC(int gotoAddress) {
        // push si; push di
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = DI;
        SI = DX;
        DI = UInt16[DS, 0x38A6];
        while (true) {
            byte al = UInt8[DS, SI];
            SI = (ushort)(SI + 1);
            AL = al;
            UInt8[DS, DI] = al;
            DI = (ushort)(DI + 1);
            if (al == 0) {
                break;
            }
        }
        // pop di; pop si
        DI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        DX = 0x3826;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xF0FF — <c>bump_allocate_bump_cx_bytes_ida</c>. Rounds CX bytes
    /// up to 16-byte paragraphs, advances the bump pointer at <c>ds[0x39B9]</c>, and
    /// tail-jumps to the out-of-memory error handler at <c>cs1:0xF131</c> if the new
    /// pointer exceeds the limit at <c>ds[0xCE68]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (29 bytes, F0FF..F11B):
    /// <code>
    /// F0FF: 8B C1         mov ax, cx
    /// F101: 05 0F 00      add ax, 0x0F          ; CF on 16-bit overflow
    /// F104: D1 D8         rcr ax, 1             ; rotate CF into bit 15
    /// F106: D1 E8         shr ax, 1
    /// F108: D1 E8         shr ax, 1
    /// F10A: D1 E8         shr ax, 1             ; net: (CF:AX) >> 4 = paragraphs
    /// F10C: 01 06 B9 39   add [0x39B9], ax
    /// F110: 50            push ax
    /// F111: A1 B9 39      mov ax, [0x39B9]
    /// F114: 3B 06 68 CE   cmp ax, [0xCE68]
    /// F118: 58            pop ax
    /// F119: 77 16         ja  F131              ; out-of-memory (asm error handler)
    /// F11B: C3            ret
    /// </code>
    /// Pure leaf except the OOM tail-jump to <c>cs1:0xF131</c>
    /// (<c>setErrorMessageToNotEnoughMemory</c>, still asm). The <c>rcr</c>+3×<c>shr</c>
    /// idiom is the 17-bit divide-by-16-with-rounding (handles cx near 0xFFFF).
    /// AX holds the paragraph count at exit (preserved across the limit check).
    /// </remarks>
    public System.Action BumpAllocate_1000_F0FF_01F0FF(int gotoAddress) {
        uint sum = (uint)CX + 0x0Fu;
        bool carry = sum > 0xFFFF;
        ushort ax = (ushort)(sum & 0xFFFF);
        AX = ax;
        // rcr ax,1 then shr ax,1 ×3  ≡  ((CF<<16)|ax) >> 4
        ushort r1 = (ushort)(((carry ? 1 : 0) << 15) | (ax >> 1));
        ushort paragraphs = (ushort)(r1 >> 3);
        AX = paragraphs;
        // add [0x39B9], ax
        ushort cur = UInt16[DS, 0x39B9];
        UInt16[DS, 0x39B9] = (ushort)(cur + paragraphs);
        // cmp [0x39B9], [0xCE68]; ja F131 (AX preserved by the asm's push/pop)
        ushort allocPtr = UInt16[DS, 0x39B9];
        ushort limit = UInt16[DS, 0xCE68];
        if (allocPtr > limit) {
            return NearJump(0xF131);
        }
        return NearRet();
    }
}
