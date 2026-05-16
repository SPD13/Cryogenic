namespace Cryogenic.Overrides;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.CPU.Registers;

/// <summary>
/// Partial class containing display and framebuffer management overrides.
/// </summary>
/// <remarks>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly. This file handles video buffer
/// switching, clearing, font selection, and character coordinate management.
/// </remarks>
public partial class Overrides {
    /// <summary>
    /// Registers display and framebuffer management function overrides with Spice86.
    /// </summary>
    public void DefineDisplayCodeOverrides() {
        //DefineFunction(cs1, 0x0579, ClearGlobalVgaOffset_1000_0579_010579);
        DefineFunction(cs1, 0x98F5, ClearUnknownValuesAndAX_1000_98F5_0198F5);
        DefineFunction(cs1, 0x9901, Set479ETo0_1000_9901_019901);
        DefineFunction(cs1, 0xC07C, SetFrontBufferAsActiveFrameBuffer_1000_C07C_01C07C);
        DefineFunction(cs1, 0xC085, SetBackBufferAsActiveFrameBuffer_1000_C085_01C085);
        DefineFunction(cs1, 0xC08E, SetTextBufferAsActiveFrameBuffer_1000_C08E_01C08E);
        DefineFunction(cs1, 0xC0AD, ClearCurrentVideoBuffer_1000_C0AD_01C0AD);
        DefineFunction(cs1, 0xD05F, GetCharacterCoordsXY_1000_D05F_01D05F);
        DefineFunction(cs1, 0xD068, SetFontToIntro_1000_D068_01D068);
        DefineFunction(cs1, 0xD075, SetFontToMenu_1000_D075_01D075);
        DefineFunction(cs1, 0xD082, SetFontToBook_1ED_D082_EF52);
        DefineFunction(cs1, 0xE270, PushAll_1000_E270_01E270);
        DefineFunction(cs1, 0xE283, PopAll_1000_E283_01E283);
        DefineFunction(cs1, 0xC0F4, MaybeBlitFrontToScreen_1000_C0F4_01C0F4);
        DefineFunction(cs1, 0xC446, FarBlitTo38ED_1000_C446_01C446);
        DefineFunction(cs1, 0xC46B, FarBlitTo38EDContinuation_1000_C46B_01C46B);
        DefineFunction(cs1, 0xCF4B, IRULxDrawOrClearSubtitle_1000_CF4B_01CF4B);
    }

    /// <summary>
    /// Override for cs1:0xCF4B — <c>IRULx_draw_or_clear_subtitle_ida</c>. Records the
    /// subtitle resource pointer (SI) at <c>ds[0x3622]</c>. If bit 1 of
    /// <c>(SI − 0x35A8)</c> is set it tail-jumps to the subtitle draw routine at
    /// <c>cs1:0xC22F</c> (with BX=0x00BE, DX=0); otherwise it zero-fills the
    /// 0x0B40-word subtitle buffer at <c>[ds:0xDBD8]:0xED80</c> and returns.
    /// </summary>
    /// <remarks>
    /// Asm (two exits, CF4B..CF6F):
    /// <code>
    /// CF4B: 8B C6         mov  ax, si
    /// CF4D: A3 22 36      mov  [0x3622], ax
    /// CF50: 2D A8 35      sub  ax, 0x35A8
    /// CF53: D1 E8         shr  ax, 1
    /// CF55: D1 E8         shr  ax, 1            ; CF = bit1 of (si-0x35A8)
    /// CF57: 73 08         jnc  CF61
    /// CF59: BB BE 00      mov  bx, 0x00BE
    /// CF5C: 33 D2         xor  dx, dx
    /// CF5E: E9 CE F2      jmp  C22F             ; near tail-jump (draw)
    /// CF61: BF 80 ED      mov  di, 0xED80
    /// CF64: 8E 06 D8 DB   mov  es, [0xDBD8]
    /// CF68: 33 C0         xor  ax, ax
    /// CF6A: B9 40 0B      mov  cx, 0x0B40
    /// CF6D: F3 AB         rep  stosw            ; clear buffer (DF=0)
    /// CF6F: C3            ret
    /// </code>
    /// Clean two-exit leaf (no calls/hardware). The <c>jmp C22F</c> is a near
    /// tail-jump within cs1 — Spice86 dispatches whatever (asm or C#) is registered
    /// at <c>cs1:0xC22F</c> (see Plans/09 §D). AX holds <c>(si-0x35A8)&gt;&gt;2</c>
    /// at the tail-jump.
    /// </remarks>
    public System.Action IRULxDrawOrClearSubtitle_1000_CF4B_01CF4B(int gotoAddress) {
        ushort ax = SI;
        AX = ax;
        UInt16[DS, 0x3622] = ax;
        ax = (ushort)(ax - 0x35A8);
        ax = (ushort)(ax >> 1);
        bool cf = (ax & 1) != 0;       // bit1 of (si-0x35A8)
        ax = (ushort)(ax >> 1);
        AX = ax;
        if (cf) {
            BX = 0x00BE;
            DX = 0;
            return NearJump(0xC22F);
        }
        DI = 0xED80;
        ES = UInt16[DS, 0xDBD8];
        AX = 0;
        ushort cx = 0x0B40;
        while (cx != 0) {
            UInt16[ES, DI] = 0;
            DI = (ushort)(DI + 2);
            cx--;
        }
        CX = 0;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xC446 — clipped blit via indirect-far dispatch through
    /// <c>ds:[0x38ED]</c>. Reads source rect (4 words) from <c>[SI..SI+6]</c>; if either
    /// dimension is ≤ 0 (after subtracting offsets) the function returns early.
    /// </summary>
    /// <remarks>
    /// Asm (41 bytes, C446..C46E):
    /// <code>
    /// C446: A1 DE DB         mov ax, [0xDBDE]
    /// C449: 51                push cx
    /// C44A: 8B C8             mov cx, ax
    /// C44C: 8B 14             mov dx, [si]
    /// C44E: 8B 5C 02          mov bx, [si+0x02]
    /// C451: 8B 6C 04          mov bp, [si+0x04]
    /// C454: 8B 44 06          mov ax, [si+0x06]
    /// C457: 2B EA             sub bp, dx
    /// C459: 76 12              jbe C46D
    /// C45B: 2B C3              sub ax, bx
    /// C45D: 76 0E              jbe C46D
    /// C45F: 8E 06 D6 DB        mov es, [0xDBD6]
    /// C463: 56                  push si
    /// C464: 1E                  push ds
    /// C465: 8B F1               mov si, cx
    /// C467: FF 1E ED 38         call far [0x38ED]
    /// C46B: 1F                  pop ds                 ; continuation entry
    /// C46C: 5E                  pop si
    /// C46D: 59                  pop cx
    /// C46E: C3                  ret
    /// </code>
    /// </remarks>
    public Action FarBlitTo38ED_1000_C446_01C446(int gotoAddress) {
        ushort dbde = UInt16[DS, 0xDBDE];
        AX = dbde;
        // push cx
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = CX;
        CX = dbde;

        ushort origDx = UInt16[DS, SI];
        DX = origDx;
        ushort origBx = UInt16[DS, (ushort)(SI + 0x02)];
        BX = origBx;
        ushort origBp = UInt16[DS, (ushort)(SI + 0x04)];
        BP = origBp;
        ushort origAx = UInt16[DS, (ushort)(SI + 0x06)];
        AX = origAx;

        // sub bp, dx; jbe C46D
        BP = (ushort)(BP - DX);
        if (origBp <= origDx) {
            // jbe — early exit: pop cx; ret
            CX = UInt16[SS, SP];
            SP = (ushort)(SP + 2);
            return NearRet();
        }
        // sub ax, bx; jbe C46D
        AX = (ushort)(AX - BX);
        if (origAx <= origBx) {
            CX = UInt16[SS, SP];
            SP = (ushort)(SP + 2);
            return NearRet();
        }
        // mov es, [0xDBD6]
        ES = globalsOnDs.Get1138_DBD6_Word16_framebufferFront();
        // push si
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;
        // push ds
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = DS;
        // mov si, cx
        SI = CX;
        // call far [0x38ED]
        ushort targetOff = UInt16[DS, 0x38ED];
        ushort targetSeg = UInt16[DS, 0x38EF];
        // Far call: push CS, push IP (0xC46B continuation)
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xC46B;
        return FarJump(targetSeg, targetOff);
    }

    /// <summary>
    /// Override for cs1:0xC46B — continuation of <see cref="FarBlitTo38ED_1000_C446_01C446"/>
    /// after the far-call returns.
    /// </summary>
    public Action FarBlitTo38EDContinuation_1000_C46B_01C46B(int gotoAddress) {
        // pop ds, pop si, pop cx, ret
        DS = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        SI = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        CX = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xC0F4 — when the front buffer differs from the screen buffer,
    /// dispatches via the function-pointer slot at <c>ds:[0x3935]</c> (typically the
    /// front-to-screen blit). Otherwise returns immediately.
    /// </summary>
    /// <remarks>
    /// Asm (14 bytes):
    /// <code>
    /// C0F4: A1 D6 DB         mov ax, [0xDBD6]
    /// C0F7: 3B 06 D8 DB      cmp ax, [0xDBD8]
    /// C0FB: 74 04             jz  C101
    /// C0FD: FF 1E 35 39       call far [0x3935]
    /// C101: C3                ret
    /// </code>
    /// The indirect <c>call far [DS:0x3935]</c> is simulated by reading the 4-byte
    /// far pointer at <c>ds:0x3935</c>, pushing <c>cs1:0xC101</c> (the bare-near-ret byte
    /// at the function tail) as the far-return address, and issuing <c>FarJump</c>. When
    /// the dispatched function does <c>retf</c>, control lands on the asm <c>C3</c> at
    /// <c>0xC101</c> which pops the outer caller's near return.
    /// </remarks>
    public Action MaybeBlitFrontToScreen_1000_C0F4_01C0F4(int gotoAddress) {
        ushort a = globalsOnDs.Get1138_DBD6_Word16_framebufferFront();
        ushort b = globalsOnDs.Get1138_DBD8_Word16_screenBuffer();
        AX = a;
        if (a == b) {
            return NearRet();
        }
        // call far [0x3935] — read 4-byte far ptr at DS:0x3935
        ushort targetOff = UInt16[DS, 0x3935];
        ushort targetSeg = UInt16[DS, 0x3937];
        // Simulate far call: push CS, push IP (0xC101 — the bare-ret byte)
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xC101;
        return FarJump(targetSeg, targetOff);
    }

    public Action ClearCurrentVideoBuffer_1000_C0AD_01C0AD(int gotoAddress) {
        State.ES = globalsOnDs.Get1138_DBDA_Word16_framebufferActive();
        VgaFunc08FillWithZeroFor64000AtES_334B_0118_0335C8(0);
        return NearRet();
    }

    public Action ClearUnknownValuesAndAX_1000_98F5_0198F5(int gotoAddress) {
        // Called after screen change (video, room, dialogue, map ...).
        // When set to 255, cannot enter orni and enter palace instead
        _loggerService.Debug("Before: 1C06:{@1C06}, 1BF8:{@1BF8}, 1BEA:{@1BEA}", globalsOnDs.Get1138_1C06_Word16(), globalsOnDs.Get1138_1BF8_Word16(), globalsOnDs.Get1138_1BEA_Word16());
        globalsOnDs.Set1138_1C06_Word16(0);

        // 128 after end of dialogue if character is in the room
        globalsOnDs.Set1138_1BF8_Word16(0);

        // 128 after end of dialogue
        globalsOnDs.Set1138_1BEA_Word16(0);

        // If not done, book videos will show a character on screen instead
        AX = 0;
        return NearRet();
    }

    // sets the gfx offset to 0
    public Action ClearGlobalVgaOffset_1000_0579_010579(int gotoAddress) {
        _loggerService.Debug("Clearing VGA offset");
        CheckVtableContainsExpected((int)SegmentRegisterIndex.DsIndex, 0x3939, cs2, 0x163);
        AX = 0;
        VgaFunc33UpdateVgaOffset01A3FromLineNumberAsAx_334B_0163_033613(0);
        return NearRet();
    }

    public Action GetCharacterCoordsXY_1000_D05F_01D05F(int gotoAddress) {
        ushort x = globalsOnDs.Get1138_D82C_Word16_CharacterXCoord();
        ushort y = globalsOnDs.Get1138_D82E_Word16_CharacterYCoord();
        DX = x;
        BX = y;
        _loggerService.Debug("getCharacterCoordsXY x:{@X} y:{@Y}", DX, BX);
        return NearRet();
    }

    public Action PopAll_1000_E283_01E283(int gotoAddress) {
        _loggerService.Debug("popAll");

        // Called in most changes related to display like scene change, displaying map, clicking on map, clicking on
        // characters ...
        // XCHG AX <-> Stack[0x0C] (or 0x0E if done before the pop)
        ushort ax = Stack.Pop16();
        ushort stackPeek = Stack.Peek16(0x0C);
        Stack.Poke16(0x0C, ax);
        AX = stackPeek;

        // Regular pops
        BP = Stack.Pop16();
        DI = Stack.Pop16();
        SI = Stack.Pop16();
        DX = Stack.Pop16();
        CX = Stack.Pop16();
        BX = Stack.Pop16();
        return NearRet();
    }

    public Action PushAll_1000_E270_01E270(int gotoAddress) {
        _loggerService.Debug("pushAll");
        Stack.Push16(BX);
        Stack.Push16(CX);
        Stack.Push16(DX);
        Stack.Push16(SI);
        Stack.Push16(DI);
        Stack.Push16(BP);
        ushort stackTop = Stack.Peek16(0);

        // XCHG AX <-> Stack[0x0C]
        ushort stackPeek = Stack.Peek16(0x0C);
        Stack.Poke16(0x0C, AX);

        // In the original assembly code, AX seems modified but it's not the case as it's restored to its original value
        // later.
        Stack.Push16(stackPeek);
        BP = stackTop;
        return NearRet();
    }

    public Action Set479ETo0_1000_9901_019901(int gotoAddress) {
        // Called in intro when skipping scenes and in the book when clicking subjects or quitting.
        // Screen in intro becomes garbled when setting something else than 0.
        globalsOnDs.Set1138_479E_Word16(0);
        return NearRet();
    }

    public Action SetBackBufferAsActiveFrameBuffer_1000_C085_01C085(int gotoAddress) {
        ushort value = globalsOnDs.Get1138_DC32_Word16_framebufferBack();
        return SetVideoBuffer(value, "setDialogueVideoBufferSegmentDC32");
    }

    // book fonts related
    public Action SetFontToBook_1ED_D082_EF52(int gotoAddress) {
        globalsOnDs.Set1138_2518_Word16_FontRelated(0xD0FF);
        globalsOnDs.Set1138_47A0_Word16_FontRelated(0xCEEC);
        return NearRet();
    }

    // intro and map fonts
    public Action SetFontToIntro_1000_D068_01D068(int gotoAddress) {
        globalsOnDs.Set1138_2518_Word16_FontRelated(0xD096);
        globalsOnDs.Set1138_47A0_Word16_FontRelated(0xCEEC);
        return NearRet();
    }

    // menu fonts related
    public Action SetFontToMenu_1000_D075_01D075(int gotoAddress) {
        globalsOnDs.Set1138_2518_Word16_FontRelated(0xD12F);
        globalsOnDs.Set1138_47A0_Word16_FontRelated(0xCF6C);
        return NearRet();
    }

    public Action SetTextBufferAsActiveFrameBuffer_1000_C08E_01C08E(int gotoAddress) {
        ushort value = globalsOnDs.Get1138_DBD8_Word16_screenBuffer();
        return SetVideoBuffer(value, "setTextVideoBufferSegmentDBD8");
    }

    public Action SetFrontBufferAsActiveFrameBuffer_1000_C07C_01C07C(int gotoAddress) {
        ushort value = globalsOnDs.Get1138_DBD6_Word16_framebufferFront();
        return SetVideoBuffer(value, "setVideoBufferSegmentDBD6");
    }

    private Action SetVideoBuffer(ushort value, string functionName) {
        ushort oldValue = globalsOnDs.Get1138_DBDA_Word16_framebufferActive();
        if (value != oldValue) {
            globalsOnDs.Set1138_DBDA_Word16_framebufferActive(value);
            _loggerService.Debug("{@FunctionName} value:{@Value}, oldValue:{@OldValue}", functionName, value, oldValue);
        }

        return NearRet();
    }
}