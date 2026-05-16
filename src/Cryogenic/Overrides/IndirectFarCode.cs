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
    }

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
