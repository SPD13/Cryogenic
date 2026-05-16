namespace Cryogenic.Overrides;

/// <summary>
/// Partial class containing input-subsystem overrides (Phase 30 — Tech/44, 45, 46).
/// </summary>
/// <remarks>
/// <para>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly.
/// </para>
/// <para>
/// Functions using INT 33 (mouse), INT 16 (keyboard BIOS), or direct I/O (IN/OUT to ports)
/// stay as asm for now — Spice86 emulates those at the CPU level. C# overrides here cover
/// only routines that read/write DS-resident state.
/// </para>
/// </remarks>
public partial class Overrides {
    /// <summary>
    /// Registers input-subsystem function overrides with Spice86.
    /// </summary>
    public void DefineInputCodeOverrides() {
        DefineFunction(cs1, 0xDD5A, GetKeyHit_1000_DD5A_01DD5A);
        DefineFunction(cs1, 0xDE54, EscConsumer_1000_DE54_01DE54);
        DefineFunction(cs1, 0xF08E, ClearKeyboardArray_1000_F08E_01F08E);
        DefineFunction(cs1, 0xDB4C, MouseStuff_1000_DB4C_01DB4C);
        DefineFunction(cs1, 0xE9F4, MouseFuncUncalled_1000_E9F4_01E9F4);
    }

    /// <summary>
    /// Override for cs1:0xE9F4 — <c>mouse_func_uncalled_ida</c>. The real-mode INT 33
    /// mouse event callback: reloads DS from the stored data segment at <c>cs:[0xEA30]</c>,
    /// latches the cursor position (CX→<c>ds[0xDC36]</c>, DX→<c>ds[0xDC38]</c>), and — when
    /// the event mask AL has further bits — folds the button-state bits into
    /// <c>ds[0xDC34]</c>. Far-returns (it is invoked by the mouse driver, not the engine).
    /// </summary>
    /// <remarks>
    /// Asm (60 bytes, far-return, E9F4..EA2F):
    /// <code>
    /// E9F4: 50               push ax
    /// E9F5: 1E               push ds
    /// E9F6: 2E 8E 1E 30 EA   mov  ds, cs:[0xEA30]
    /// E9FB: 89 0E 36 DC      mov  [0xDC36], cx
    /// E9FF: 89 16 38 DC      mov  [0xDC38], dx
    /// EA03: D0 E8            shr  al, 1            ; ZF → skip buttons
    /// EA05: 74 26            jz   EA2D
    /// EA07: 51               push cx
    /// EA08: 8A 0E 34 DC      mov  cl, [0xDC34]
    /// EA0C: D0 E8            shr  al, 1
    /// EA0E: 73 03            jnc  EA13
    /// EA10: 80 C9 01         or   cl, 0x01
    /// EA13: D0 E8            shr  al, 1
    /// EA15: 73 03            jnc  EA1A
    /// EA17: 80 E1 FE         and  cl, 0xFE
    /// EA1A: D0 E8            shr  al, 1
    /// EA1C: 73 03            jnc  EA21
    /// EA1E: 80 C9 02         or   cl, 0x02
    /// EA21: D0 E8            shr  al, 1
    /// EA23: 73 03            jnc  EA28
    /// EA25: 80 E1 FD         and  cl, 0xFD
    /// EA28: 88 0E 34 DC      mov  [0xDC34], cl
    /// EA2C: 59               pop  cx
    /// EA2D: 1F               pop  ds
    /// EA2E: 58               pop  ax
    /// EA2F: CB               retf
    /// </code>
    /// Pure leaf, far-return. "Uncalled" in this build (the driver never dispatches it)
    /// — ported for completeness per the plan's "replace every ASM routine" goal and
    /// annotated as dead code. AX, CX and DS are all restored before the far return;
    /// the only side effects are the three DS-resident mouse-state words. The
    /// <c>cs:[0xEA30]</c> source word is read live from the cs1 code image (the DS the
    /// driver stored at registration time).
    /// </remarks>
    public System.Action MouseFuncUncalled_1000_E9F4_01E9F4(int gotoAddress) {
        ushort savedAx = AX;
        ushort savedDs = DS;
        DS = UInt16[cs1, 0xEA30];
        UInt16[DS, 0xDC36] = CX;
        UInt16[DS, 0xDC38] = DX;
        byte al = (byte)(savedAx & 0xFF);
        al = (byte)(al >> 1);          // shr al,1 — ZF tested below
        if (al != 0) {
            byte cl = UInt8[DS, 0xDC34];
            bool cf;
            cf = (al & 1) != 0; al = (byte)(al >> 1);
            if (cf) { cl = (byte)(cl | 0x01); }
            cf = (al & 1) != 0; al = (byte)(al >> 1);
            if (cf) { cl = (byte)(cl & 0xFE); }
            cf = (al & 1) != 0; al = (byte)(al >> 1);
            if (cf) { cl = (byte)(cl | 0x02); }
            cf = (al & 1) != 0; al = (byte)(al >> 1);
            if (cf) { cl = (byte)(cl & 0xFD); }
            UInt8[DS, 0xDC34] = cl;
        }
        DS = savedDs;
        AX = savedAx;
        return FarRet();
    }

    /// <summary>
    /// Override for cs1:0xDB4C — <c>mouse_stuff_ida</c>. Derives the packed mouse-button
    /// state from <c>ds[0xDC34]</c> (storing the low 2 bits at <c>ds[0xDC35]</c>) and
    /// returns the cursor X/Y in DX/BX from <c>ds[0xDC36]</c> / <c>ds[0xDC38]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (27 bytes):
    /// <code>
    /// DB4C: A1 34 DC      mov  ax, [0xDC34]
    /// DB4F: 24 03         and  al, 0x03
    /// DB51: A2 35 DC      mov  [0xDC35], al
    /// DB54: 32 E0         xor  ah, al
    /// DB56: 02 E4         add  ah, ah
    /// DB58: 02 E4         add  ah, ah
    /// DB5A: 0A C4         or   al, ah
    /// DB5C: 32 E4         xor  ah, ah
    /// DB5E: 8B 16 36 DC   mov  dx, [0xDC36]
    /// DB62: 8B 1E 38 DC   mov  bx, [0xDC38]
    /// DB66: C3            ret
    /// </code>
    /// Pure leaf. AL ends as <c>(raw&amp;3) | (((ah^(raw&amp;3))&lt;&lt;2)&amp;0xFF)</c>,
    /// AH cleared; DX = cursor X, BX = cursor Y.
    /// </remarks>
    public System.Action MouseStuff_1000_DB4C_01DB4C(int gotoAddress) {
        ushort raw = UInt16[DS, 0xDC34];
        byte al = (byte)((raw & 0xFF) & 0x03);
        byte ah = (byte)(raw >> 8);
        UInt8[DS, 0xDC35] = al;
        ah = (byte)(ah ^ al);
        ah = (byte)(ah + ah);
        ah = (byte)(ah + ah);
        al = (byte)(al | ah);
        AX = al;
        DX = UInt16[DS, 0xDC36];
        BX = UInt16[DS, 0xDC38];
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xF08E — <c>clear_keyboard_array_ida</c>. Zeroes the latched key
    /// byte at <c>ds[0xCEE8]</c> and the 0x67-byte key-state array at <c>ds[0xCE81]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (18 bytes):
    /// <code>
    /// F08E: 06               push es
    /// F08F: 1E               push ds
    /// F090: 07               pop es                ; ES = DS
    /// F091: 33 C0            xor ax, ax
    /// F093: A2 E8 CE         mov [0xCEE8], al
    /// F096: BF 81 CE         mov di, 0xCE81
    /// F099: B9 67 00         mov cx, 0x67
    /// F09C: F3 AA            rep stosb
    /// F09E: 07               pop es
    /// F09F: C3               ret
    /// </code>
    /// Pure leaf. The push/pop es brackets the rep-stosb; ES is restored to its caller
    /// value at exit, so we just mirror the net memory clear.
    /// </remarks>
    public System.Action ClearKeyboardArray_1000_F08E_01F08E(int gotoAddress) {
        // push es; push ds; pop es → ES = DS (saved es restored at the end)
        ushort savedEs = ES;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = savedEs;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = DS;
        ES = UInt16[SS, SP];
        SP = (ushort)(SP + 2);

        AX = 0;
        UInt8[DS, 0xCEE8] = 0;
        DI = 0xCE81;
        CX = 0x67;
        // rep stosb: fill CX bytes at ES:DI with AL (0)
        for (ushort n = 0; n < 0x67; n++) {
            UInt8[ES, (ushort)(0xCE81 + n)] = 0;
        }
        DI = (ushort)(0xCE81 + 0x67);
        CX = 0;
        // pop es
        ES = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xDD5A — <c>get_key_hit_ida</c>. Atomically consumes the latched
    /// key scancode at <c>ds[0xCEE8]</c> and returns it in AL.
    /// </summary>
    /// <remarks>
    /// Asm (9 bytes):
    /// <code>
    /// DD5A: 32 C0          xor al, al
    /// DD5C: 86 06 E8 CE    xchg al, [0xCEE8]
    /// DD60: 0A C0          or  al, al
    /// DD62: C3             ret
    /// </code>
    /// Side-effects: ZF = (key == 0); CF = 0; SF = bit 7 of scancode (always 0 in practice
    /// since scancodes are 0..0x7F). Pure leaf.
    /// </remarks>
    public System.Action GetKeyHit_1000_DD5A_01DD5A(int gotoAddress) {
        // xchg al, [0xCEE8] — AL was 0 from `xor al, al`, so this puts the latched key
        // into AL and clears the latch.
        byte cee8 = (byte)globalsOnDs.Get1138_CEE8_Byte8_keyHit();
        globalsOnDs.Set1138_CEE8_Byte8_keyHit(0);
        AL = cee8;
        // or al, al — set flags from AL
        ZeroFlag = (cee8 == 0);
        CarryFlag = false;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xDE54 — Esc-key consumer (Tech/45).
    /// </summary>
    /// <remarks>
    /// Asm (20 bytes):
    /// <code>
    /// DE54: C6 06 E9 CE 00   mov byte [0xCEE9], 0
    /// DE59: 80 3E E8 CE 01   cmp byte [0xCEE8], 0x01
    /// DE5E: 75 07             jnz DE67
    /// DE60: C6 06 E9 CE 01   mov byte [0xCEE9], 0x01
    /// DE65: EB E7             jmp 0xDE4E         ; tail-jump to SetCEE8To0 (C#)
    /// DE67: C3                ret
    /// </code>
    /// First clears the "Esc-consumed" flag at <c>[0xCEE9]</c>. If the keyboard latch
    /// <c>[0xCEE8]</c> holds scancode 1 (Esc), sets the consumed flag to 1 and tail-jumps
    /// into <see cref="SetCEE8To0_1000_DE4E_01DE4E"/> (already C# in UnknownCode.cs),
    /// which clears the keyboard latch. Pure leaf — the tail-jump target is C#.
    /// </remarks>
    public System.Action EscConsumer_1000_DE54_01DE54(int gotoAddress) {
        UInt8[DS, 0xCEE9] = 0;
        byte cee8 = (byte)globalsOnDs.Get1138_CEE8_Byte8_keyHit();
        if (cee8 != 0x01) {
            return NearRet();
        }
        UInt8[DS, 0xCEE9] = 0x01;
        return NearJump(0xDE4E);
    }
}
