namespace Cryogenic.Overrides;

/// <summary>
/// Partial class containing thin wrappers around indirect far/near dispatches
/// (the §A "call far [m]" cluster — see Plans/09 §A).
/// </summary>
/// <remarks>
/// <para>
/// Each routine sets up a few registers/globals then transfers through an indirect
/// pointer. The faithful Spice86 realization of <c>call far [m]</c> is: read the
/// 4-byte far pointer, push CS (cs1) and the continuation IP (the instruction right
/// after the <c>call far</c>, which stays raw asm), then <see cref="FarJump"/> to the
/// resolved seg:off. When the callee <c>retf</c>s it pops that synthetic CS:IP and the
/// raw asm at the continuation byte runs (typically <c>ret</c> / a tiny epilogue).
/// </para>
/// <para>
/// <c>jmp [m]</c> / jump-table forms are pure tail transfers — resolve the target and
/// <see cref="NearJump"/>/<see cref="FarJump"/> with no continuation push.
/// </para>
/// </remarks>
public partial class Overrides {
    /// <summary>Registers the indirect-far thin-wrapper overrides with Spice86.</summary>
    public void DefineIndirectFarCodeOverrides() {
        DefineFunction(cs1, 0xAC30, FarCall3999_1000_AC30_01AC30);
        DefineFunction(cs1, 0xEF22, CallXmsDriverFunc_1000_EF22_01EF22);
        DefineFunction(cs1, 0xA334, JumpTableA376_1000_A334_01A334);
        DefineFunction(cs1, 0xB9AE, FarCall3915WithEs_1000_B9AE_01B9AE);
        DefineFunction(cs1, 0xC432, FarCall38D9_1000_C432_01C432);
        DefineFunction(cs1, 0x1834, FarCall3919_1000_1834_011834);
        DefineFunction(cs1, 0xAEB7, FarCall3975StoreAl_1000_AEB7_01AEB7);
        DefineFunction(cs1, 0x7B1B, FarCall38DDThenJmp_1000_7B1B_017B1B);
        DefineFunction(cs1, 0xC412, FarCallSs38E1_1000_C412_01C412);
        DefineFunction(cs1, 0xC49A, GfxCopyFramebufferToScreen_1000_C49A_01C49A);
        DefineFunction(cs1, 0xC4CD, GfxCopyFramebufToScreen_1000_C4CD_01C4CD);
        DefineFunction(cs1, 0xC0D5, FarCallSs392D_1000_C0D5_01C0D5);
        DefineFunction(cs1, 0xEC46, CallMemoryFunc2_1000_EC46_01EC46);
        DefineFunction(cs1, 0xEC59, CallMemoryFunc1_1000_EC59_01EC59);
        DefineFunction(cs1, 0xDBB2, CallRestoreCursor_1000_DBB2_01DBB2);
        DefineFunction(cs1, 0x4B16, FarCall38FDIfNeBuffers_1000_4B16_014B16);
        DefineFunction(cs1, 0x0D0D, FarCall3951OrJmp0D23_1000_0D0D_010D0D);
        DefineFunction(cs1, 0xC477, GfxCopyRectAtSi_1000_C477_01C477);
        DefineFunction(cs1, 0xC4AA, GfxCopyRectToScreen_1000_C4AA_01C4AA);
        DefineFunction(cs1, 0xADED, FarCall397DClampBl_1000_ADED_01ADED);
        DefineFunction(cs1, 0xADE0, FarCall397DAlt_1000_ADE0_01ADE0);
        DefineFunction(cs1, 0xA637, FarCall39A5_1000_A637_01A637);
        DefineFunction(cs1, 0xA650, FarCall3985_1000_A650_01A650);
        DefineFunction(cs1, 0xD42F, DispatchCx4_1000_D42F_01D42F);
        DefineFunction(cs1, 0xD434, DispatchCx3_1000_D434_01D434);
        DefineFunction(cs1, 0xD439, DispatchCx2_1000_D439_01D439);
        DefineFunction(cs1, 0xD43E, DispatchCx1_1000_D43E_01D43E);
        DefineFunction(cs1, 0xC43E, BlitSi1470_1000_C43E_01C43E);
        DefineFunction(cs1, 0xC443, BlitSiD834_1000_C443_01C443);
        DefineFunction(cs1, 0xC474, RectCopySi1470_1000_C474_01C474);
        DefineFunction(cs1, 0xC53E, FarCall3901_1000_C53E_01C53E);
        DefineFunction(cs1, 0xD741, FarCall38DDIfBelow3_1000_D741_01D741);
        DefineFunction(cs1, 0xEFBA, FarCall3981Guarded_1000_EFBA_01EFBA);
    }

    /// <summary>cs1:0xC53E — <c>si=0x276A; bp=[0x2772]; al=[0xDBE4];
    /// es=[0xDBDA]; call far [0x3901]; ret</c>. Continuation = raw asm
    /// <c>C3</c> @cs1:0xC550.</summary>
    public System.Action FarCall3901_1000_C53E_01C53E(int gotoAddress) {
        SI = 0x276A;
        BP = UInt16[DS, 0x2772];
        AL = UInt8[DS, 0xDBE4];
        ES = UInt16[DS, 0xDBDA];
        ushort off = UInt16[DS, 0x3901];
        ushort seg = UInt16[DS, 0x3903];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xC550;
        return FarJump(seg, off);
    }

    /// <summary>cs1:0xD741 — <c>ax=[0x1B0C]-3; if ax&gt;=3 (unsigned) ret; else
    /// si=0x2458; es=[0xDBD8]; al=0xF0; call far [0x38DD]; ret</c>. Continuation =
    /// raw asm <c>C3</c> @cs1:0xD759.</summary>
    public System.Action FarCall38DDIfBelow3_1000_D741_01D741(int gotoAddress) {
        ushort ax = (ushort)(UInt16[DS, 0x1B0C] - 3);
        AX = ax;
        if (ax >= 0x0003) {                               // cmp ax,3 ; jnc D759
            CarryFlag = false;
            return NearRet();
        }
        SI = 0x2458;
        ES = UInt16[DS, 0xDBD8];
        AL = 0xF0;
        ushort off = UInt16[DS, 0x38DD];
        ushort seg = UInt16[DS, 0x38DF];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xD759;
        return FarJump(seg, off);
    }

    /// <summary>
    /// cs1:0xEFBA — <c>push bx; if (byte[0x2943]&amp;0x10) {pop bx; ret}; push cx;
    /// call far [0x3981]</c>. Continuation = raw asm
    /// <c>mov [0xDBCD],al; mov [0xDBCE],bx; mov [0xDBD0],cx; pop cx; pop bx; ret</c>
    /// @cs1:0xEFC7 (pops the pushed cx then bx).
    /// </summary>
    public System.Action FarCall3981Guarded_1000_EFBA_01EFBA(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BX;       // push bx
        if ((UInt8[DS, 0x2943] & 0x10) != 0) {            // test ; jnz EFD3
            BX = UInt16[SS, SP];                          // pop bx
            SP = (ushort)(SP + 2);
            return NearRet();
        }
        SP = (ushort)(SP - 2); UInt16[SS, SP] = CX;       // push cx
        ushort off = UInt16[DS, 0x3981];
        ushort seg = UInt16[DS, 0x3983];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xEFC7;
        return FarJump(seg, off);
    }

    // 0xD42F/34/39/3E — parameterized entries that set CX then jump into the
    // shared dispatcher body at cs1:0xD445 (call C# 0xD454; or bx,bx; test ah,0x40;
    // jmp bx; ret — same body as DispatcherJumpsToBX 0xD443 minus the xor cx,cx).
    /// <summary>cs1:0xD42F — <c>mov cx,4; jmp 0xD445</c>.</summary>
    public System.Action DispatchCx4_1000_D42F_01D42F(int gotoAddress) {
        CX = 4;
        return NearJump(0xD445);
    }

    /// <summary>cs1:0xD434 — <c>mov cx,3; jmp 0xD445</c>.</summary>
    public System.Action DispatchCx3_1000_D434_01D434(int gotoAddress) {
        CX = 3;
        return NearJump(0xD445);
    }

    /// <summary>cs1:0xD439 — <c>mov cx,2; jmp 0xD445</c>.</summary>
    public System.Action DispatchCx2_1000_D439_01D439(int gotoAddress) {
        CX = 2;
        return NearJump(0xD445);
    }

    /// <summary>cs1:0xD43E — <c>mov cx,1; jmp 0xD445</c>.</summary>
    public System.Action DispatchCx1_1000_D43E_01D43E(int gotoAddress) {
        CX = 1;
        return NearJump(0xD445);
    }

    /// <summary>cs1:0xC43E — <c>mov si,0x1470; jmp 0xC446</c> (the C#
    /// <see cref="FarBlitTo38ED_1000_C446_01C446"/>).</summary>
    public System.Action BlitSi1470_1000_C43E_01C43E(int gotoAddress) {
        SI = 0x1470;
        return NearJump(0xC446);
    }

    /// <summary>cs1:0xC443 — <c>mov si,0xD834; jmp 0xC446</c> (the C#
    /// <see cref="FarBlitTo38ED_1000_C446_01C446"/>).</summary>
    public System.Action BlitSiD834_1000_C443_01C443(int gotoAddress) {
        SI = 0xD834;
        return NearJump(0xC446);
    }

    /// <summary>cs1:0xC474 — <c>mov si,0x1470; jmp 0xC477</c> (the C#
    /// <see cref="GfxCopyRectAtSi_1000_C477_01C477"/>).</summary>
    public System.Action RectCopySi1470_1000_C474_01C474(int gotoAddress) {
        SI = 0x1470;
        return NearJump(0xC477);
    }

    /// <summary>cs1:0xADED — <c>ax=0x0190; bl=[0x2896]; bh=[0x28AE];
    /// if bl&lt;4 bl=4; call far [0x397D]; ret</c>. Continuation = raw asm
    /// <c>C3</c> @cs1:0xAE03.</summary>
    public System.Action FarCall397DClampBl_1000_ADED_01ADED(int gotoAddress) {
        AX = 0x0190;
        byte bl = UInt8[DS, 0x2896];
        byte bh = UInt8[DS, 0x28AE];
        if (bl < 0x04) {                                  // cmp bl,4 ; jnc ADFF
            bl = 0x04;
        }
        BX = (ushort)((bh << 8) | bl);
        ushort off = UInt16[DS, 0x397D];
        ushort seg = UInt16[DS, 0x397F];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xAE03;
        return FarJump(seg, off);
    }

    /// <summary>cs1:0xADE0 — <c>ax=0x0064; bl=[0x289E]; bh=[0x28B6];
    /// jmp 0xADF8</c> (shares 0xADED's clamp+call-far tail, run as raw asm).</summary>
    public System.Action FarCall397DAlt_1000_ADE0_01ADE0(int gotoAddress) {
        AX = 0x0064;
        byte bl = UInt8[DS, 0x289E];
        byte bh = UInt8[DS, 0x28B6];
        BX = (ushort)((bh << 8) | bl);
        return NearJump(0xADF8);
    }

    /// <summary>cs1:0xA637 — if <c>(word[0xDBC8] &amp; 4)==0</c> set
    /// <c>[0x288E]=0xFF</c>; then <c>al=[0x288E]; ah=[0x28A6]; call far [0x39A5];
    /// ret</c>. Continuation = raw asm <c>C3</c> @cs1:0xA64F.</summary>
    public System.Action FarCall39A5_1000_A637_01A637(int gotoAddress) {
        if ((UInt16[DS, 0xDBC8] & 0x0004) == 0) {         // test ; jnz A644
            UInt8[DS, 0x288E] = 0xFF;
        }
        byte al = UInt8[DS, 0x288E];
        byte ah = UInt8[DS, 0x28A6];
        AX = (ushort)((ah << 8) | al);
        ushort off = UInt16[DS, 0x39A5];
        ushort seg = UInt16[DS, 0x39A7];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xA64F;
        return FarJump(seg, off);
    }

    /// <summary>cs1:0xA650 — if <c>(word[0xDBC8] &amp; 0x400)==0</c> set
    /// <c>[0x2896]=[0x289E]=0xFF</c>; then <c>ah=[0x28AE]; al=[0x2896];
    /// if al&lt;4 al=4; call far [0x3985]; ret</c>. Continuation = raw asm
    /// <c>C3</c> @cs1:0xA671.</summary>
    public System.Action FarCall3985_1000_A650_01A650(int gotoAddress) {
        if ((UInt16[DS, 0xDBC8] & 0x0400) == 0) {         // test ; jnz A660
            UInt8[DS, 0x2896] = 0xFF;
            UInt8[DS, 0x289E] = 0xFF;
        }
        byte ah = UInt8[DS, 0x28AE];
        byte al = UInt8[DS, 0x2896];
        if (al < 0x04) {                                  // cmp al,4 ; jnc A66D
            al = 0x04;
        }
        AX = (ushort)((ah << 8) | al);
        ushort off = UInt16[DS, 0x3985];
        ushort seg = UInt16[DS, 0x3987];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xA671;
        return FarJump(seg, off);
    }

    // Shared shape (gfx rect copy): read 4-word rect from [si]; bounds-check
    // (sub bp,dx / sub ax,bx, jbe -> ret); push ds; es=[E]; ds=[B];
    // ss: call far [ss:P]; pop ds; ret.  contIp = the `pop ds; ret`.
    private System.Action GfxRectCopySsPtr(ushort gOffEs, ushort gOffDs, ushort ssPtr, ushort contIp) {
        ushort origDs = DS;
        ushort dx = UInt16[origDs, SI];
        DX = dx;
        ushort bx = UInt16[origDs, (ushort)(SI + 2)];
        BX = bx;
        ushort bp = UInt16[origDs, (ushort)(SI + 4)];
        ushort ax = UInt16[origDs, (ushort)(SI + 6)];
        ushort bpOrig = bp;
        bp = (ushort)(bp - dx);
        BP = bp;
        if (bpOrig <= dx) {                              // sub bp,dx ; jbe -> ret
            return NearRet();
        }
        ushort axOrig = ax;
        ax = (ushort)(ax - bx);
        AX = ax;
        if (axOrig <= bx) {                              // sub ax,bx ; jbe -> ret
            return NearRet();
        }
        ES = UInt16[origDs, gOffEs];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = origDs;  // push ds
        DS = UInt16[origDs, gOffDs];
        ushort off = UInt16[SS, ssPtr];
        ushort seg = UInt16[SS, (ushort)(ssPtr + 2)];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = contIp;
        return FarJump(seg, off);
    }

    /// <summary>cs1:0x4B16 — if <c>[0xDBD8] != [0xDBD6]</c>: <c>es = [0xDBD8]+0x1E0;
    /// call far [0x38FD]</c>; else returns. Continuation = raw asm <c>C3</c>
    /// @cs1:0x4B2A.</summary>
    public System.Action FarCall38FDIfNeBuffers_1000_4B16_014B16(int gotoAddress) {
        ushort ax = UInt16[DS, 0xDBD8];
        AX = ax;
        ushort si = UInt16[DS, 0xDBD6];
        SI = si;
        if (ax == si) {                                  // cmp ax,si ; jz 4B2A
            ZeroFlag = true;
            return NearRet();
        }
        ax = (ushort)(ax + 0x01E0);
        AX = ax;
        ES = ax;
        ushort off = UInt16[DS, 0x38FD];
        ushort seg = UInt16[DS, 0x38FF];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0x4B2A;
        return FarJump(seg, off);
    }

    /// <summary>
    /// cs1:0x0D0D — <c>al=bl; bx=0x0180; cx=0x0054; dl=0x37; jz 0x0D23</c> (on the
    /// caller's incoming ZF); else <c>dec dx; cmp al,0x0A; jz 0x0D23</c>; else
    /// <c>call far [0x3951]; ret</c>. 0x0D23 is the still-asm continuation body;
    /// the call-far continuation = raw asm <c>C3</c> @cs1:0x0D22.
    /// </summary>
    public System.Action FarCall3951OrJmp0D23_1000_0D0D_010D0D(int gotoAddress) {
        byte al = (byte)(BX & 0xFF);                     // mov al,bl (original BL)
        AL = al;
        BX = 0x0180;
        CX = 0x0054;
        DX = (ushort)((DX & 0xFF00) | 0x37);             // mov dl,0x37
        if (ZeroFlag) {                                  // jz 0x0D23 (incoming ZF)
            return NearJump(0x0D23);
        }
        DX = (ushort)(DX - 1);                            // dec dx
        if (al == 0x0A) {                                 // cmp al,0x0A ; jz 0x0D23
            return NearJump(0x0D23);
        }
        ushort off = UInt16[DS, 0x3951];
        ushort seg = UInt16[DS, 0x3953];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0x0D22;
        return FarJump(seg, off);
    }

    /// <summary>cs1:0xC477 — <c>gfx_copy_rect_at_si_ida</c>: rect bounds-check then
    /// <c>es=[0xDBDE]; ds=[0xDBD6]; ss: call far [ss:0x38E5]</c>. Continuation =
    /// raw asm <c>pop ds; ret</c> @cs1:0xC498.</summary>
    public System.Action GfxCopyRectAtSi_1000_C477_01C477(int gotoAddress)
        => GfxRectCopySsPtr(0xDBDE, 0xDBD6, 0x38E5, 0xC498);

    /// <summary>cs1:0xC4AA — <c>gfx_copy_rect_to_screen_ida</c>: rect bounds-check then
    /// <c>es=[0xDBD6]; ds=[0xDBD8]; ss: call far [ss:0x38F5]</c>. Continuation =
    /// raw asm <c>pop ds; ret</c> @cs1:0xC4CB.</summary>
    public System.Action GfxCopyRectToScreen_1000_C4AA_01C4AA(int gotoAddress)
        => GfxRectCopySsPtr(0xDBD6, 0xDBD8, 0x38F5, 0xC4CB);

    // Shared shape: push ds; es=[A]; ds=[B]; ss: call far [ss:P]; pop ds; ret.
    private System.Action FarCallPushDsEsBSsPtr(ushort gOffEs, ushort gOffDs, ushort ssPtr, ushort contIp) {
        ushort origDs = DS;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = origDs;        // push ds
        ES = UInt16[origDs, gOffEs];
        DS = UInt16[origDs, gOffDs];
        ushort off = UInt16[SS, ssPtr];
        ushort seg = UInt16[SS, (ushort)(ssPtr + 2)];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = contIp;
        return FarJump(seg, off);
    }

    /// <summary>cs1:0xC412 — <c>push ds; es=[0xDBDE]; ds=[0xDBDA]; ss: call far
    /// [ss:0x38E1]; pop ds; ret</c>. Continuation = raw asm <c>pop ds; ret</c>
    /// @cs1:0xC421.</summary>
    public System.Action FarCallSs38E1_1000_C412_01C412(int gotoAddress)
        => FarCallPushDsEsBSsPtr(0xDBDE, 0xDBDA, 0x38E1, 0xC421);

    /// <summary>cs1:0xC49A — <c>gfx_copy_framebuffer_to_screen_ida</c>:
    /// <c>push ds; es=[0xDBD6]; ds=[0xDBD8]; ss: call far [ss:0x38F1]; pop ds;
    /// ret</c>. Continuation @cs1:0xC4A9.</summary>
    public System.Action GfxCopyFramebufferToScreen_1000_C49A_01C49A(int gotoAddress)
        => FarCallPushDsEsBSsPtr(0xDBD6, 0xDBD8, 0x38F1, 0xC4A9);

    /// <summary>cs1:0xC4CD — <c>gfx_copy_framebuf_to_screen_ida</c>:
    /// <c>push ds; es=[0xDBD8]; ds=[0xDBD6]; ss: call far [ss:0x38F1]; pop ds;
    /// ret</c>. Continuation @cs1:0xC4DC.</summary>
    public System.Action GfxCopyFramebufToScreen_1000_C4CD_01C4CD(int gotoAddress)
        => FarCallPushDsEsBSsPtr(0xDBD8, 0xDBD6, 0x38F1, 0xC4DC);

    /// <summary>cs1:0xC0D5 — <c>push ds; es=[0xDBD8]; ds=[0xDBD6]; bp=0xCE7A;
    /// ss: call far [ss:0x392D]; pop ds; ret</c>. Continuation @cs1:0xC0E7.</summary>
    public System.Action FarCallSs392D_1000_C0D5_01C0D5(int gotoAddress) {
        ushort origDs = DS;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = origDs;
        ES = UInt16[origDs, 0xDBD8];
        DS = UInt16[origDs, 0xDBD6];
        BP = 0xCE7A;
        ushort off = UInt16[SS, 0x392D];
        ushort seg = UInt16[SS, 0x392F];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xC0E7;
        return FarJump(seg, off);
    }

    /// <summary>
    /// cs1:0xEC46 — <c>call_memory_func_2_ida</c>: pushes cx/di/ds/es, <c>dec bx</c>,
    /// near-indirect <c>call cs:[0xEA79]</c>; continuation = raw asm
    /// <c>pop es; pop ds; pop di; pop cx; pushf; add di,cx; popf; ret</c> @cs1:0xEC50.
    /// </summary>
    public System.Action CallMemoryFunc2_1000_EC46_01EC46(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = CX;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DI;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DS;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = ES;
        BX = (ushort)(BX - 1);
        ushort target = UInt16[cs1, 0xEA79];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xEC50;   // near return IP
        return NearJump(target);
    }

    /// <summary>
    /// cs1:0xEC59 — <c>call_memory_func_1_ida</c>: pushes si/ds/es/cx, <c>dec bx</c>,
    /// near-indirect <c>call cs:[0xEA77]</c>; continuation = raw asm
    /// <c>pop ax; pop es; pop ds; pop si; pushf; add si,ax; popf; ret</c> @cs1:0xEC63.
    /// </summary>
    public System.Action CallMemoryFunc1_1000_EC59_01EC59(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DS;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = ES;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = CX;
        BX = (ushort)(BX - 1);
        ushort target = UInt16[cs1, 0xEA77];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xEC63;
        return NearJump(target);
    }

    /// <summary>
    /// cs1:0xDBB2 — <c>call_restore_cursor_ida</c>: <c>push ax; al=[0xDC46];
    /// dec [0xDC46] (re-inc unless it went negative); or al,al; js 0xDBC8;
    /// call far [0x38C5]</c>. Continuation = raw asm <c>pop ax; ret</c> @cs1:0xDBC8.
    /// </summary>
    public System.Action CallRestoreCursor_1000_DBB2_01DBB2(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = AX;       // push ax
        byte al = UInt8[DS, 0xDC46];
        byte decd = (byte)(UInt8[DS, 0xDC46] - 1);
        UInt8[DS, 0xDC46] = decd;
        if ((sbyte)decd >= 0) {
            UInt8[DS, 0xDC46] = (byte)(decd + 1);
        }
        if ((sbyte)al < 0) {
            return NearJump(0xDBC8);                       // raw asm: pop ax; ret
        }
        ushort off = UInt16[DS, 0x38C5];
        ushort seg = UInt16[DS, 0x38C7];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xDBC8;
        return FarJump(seg, off);
    }

    /// <summary>cs1:0xAC30 — <c>call far [0x3999]; ret</c>. Continuation = raw asm
    /// <c>C3</c> at cs1:0xAC34.</summary>
    public System.Action FarCall3999_1000_AC30_01AC30(int gotoAddress) {
        ushort off = UInt16[DS, 0x3999];
        ushort seg = UInt16[DS, 0x399B];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xAC34;
        return FarJump(seg, off);
    }

    /// <summary>
    /// cs1:0xEF22 — <c>call_xms_driver_func_ida</c>: <c>call far cs:[0xEE8C];
    /// cmp ax,1; ret</c>. The far pointer lives in the code segment (CS override).
    /// Continuation = raw asm <c>cmp ax,1; ret</c> at cs1:0xEF27.
    /// </summary>
    public System.Action CallXmsDriverFunc_1000_EF22_01EF22(int gotoAddress) {
        ushort off = UInt16[cs1, 0xEE8C];
        ushort seg = UInt16[cs1, 0xEE8E];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xEF27;
        return FarJump(seg, off);
    }

    /// <summary>
    /// cs1:0xA334 — <c>and bx,0x1F; jmp word cs:[bx+0xA376]</c>. Pure jump-table
    /// dispatcher (near indirect tail-jump); no continuation.
    /// </summary>
    public System.Action JumpTableA376_1000_A334_01A334(int gotoAddress) {
        BX = (ushort)(BX & 0x1F);
        ushort target = UInt16[cs1, (ushort)(BX + 0xA376)];
        return NearJump(target);
    }

    /// <summary>cs1:0xB9AE — <c>mov es,[0xDBD6]; call far [0x3915]; jc 0xB98E;
    /// ret</c>. Continuation = raw asm <c>jc; ret</c> at cs1:0xB9B6.</summary>
    public System.Action FarCall3915WithEs_1000_B9AE_01B9AE(int gotoAddress) {
        ES = UInt16[DS, 0xDBD6];
        ushort off = UInt16[DS, 0x3915];
        ushort seg = UInt16[DS, 0x3917];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xB9B6;
        return FarJump(seg, off);
    }

    /// <summary>cs1:0xC432 — <c>mov si,0x1470; mov es,[0xDBDA]; call far [0x38D9];
    /// ret</c>. Continuation = raw asm <c>C3</c> at cs1:0xC43D.</summary>
    public System.Action FarCall38D9_1000_C432_01C432(int gotoAddress) {
        SI = 0x1470;
        ES = UInt16[DS, 0xDBDA];
        ushort off = UInt16[DS, 0x38D9];
        ushort seg = UInt16[DS, 0x38DB];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xC43D;
        return FarJump(seg, off);
    }

    /// <summary>cs1:0x1834 — <c>mov si,0xCD9E; mov bp,0x1E76; mov es,[0xDBD6];
    /// call far [0x3919]; ret</c>. Continuation = raw asm <c>C3</c> at cs1:0x1843.</summary>
    public System.Action FarCall3919_1000_1834_011834(int gotoAddress) {
        SI = 0xCD9E;
        BP = 0x1E76;
        ES = UInt16[DS, 0xDBD6];
        ushort off = UInt16[DS, 0x3919];
        ushort seg = UInt16[DS, 0x391B];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0x1843;
        return FarJump(seg, off);
    }

    /// <summary>
    /// cs1:0xAEB7 — <c>push ax; mov byte [0xDBCB],0; call far [0x3975];
    /// mov [0xDBCD],al; pop ax; ret</c>. Continuation = raw asm
    /// <c>mov [0xDBCD],al; pop ax; ret</c> at cs1:0xAEC1 (pops the pushed AX).
    /// </summary>
    public System.Action FarCall3975StoreAl_1000_AEB7_01AEB7(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = AX;   // push ax
        UInt8[DS, 0xDBCB] = 0;
        ushort off = UInt16[DS, 0x3975];
        ushort seg = UInt16[DS, 0x3977];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xAEC1;
        return FarJump(seg, off);
    }

    /// <summary>
    /// cs1:0x7B1B — <c>mov es,[0xDBDA]; mov al,[si+9]; push si; call far [0x38DD];
    /// pop si; jmp …</c>. Continuation = raw asm <c>pop si; jmp 0xC552</c> at
    /// cs1:0x7B26 (pops the pushed SI).
    /// </summary>
    public System.Action FarCall38DDThenJmp_1000_7B1B_017B1B(int gotoAddress) {
        ES = UInt16[DS, 0xDBDA];
        AL = UInt8[DS, (ushort)(SI + 9)];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;   // push si
        ushort off = UInt16[DS, 0x38DD];
        ushort seg = UInt16[DS, 0x38DF];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0x7B26;
        return FarJump(seg, off);
    }
}
