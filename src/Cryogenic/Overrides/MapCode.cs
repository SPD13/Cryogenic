namespace Cryogenic.Overrides;


using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.ReverseEngineer;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Utils;

using System;

/// <summary>
/// Partial class containing map view and cursor handling overrides.
/// </summary>
/// <remarks>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly. This file handles different
/// map views (flat map, globe, ornithopter map) and their associated click handlers.
/// </remarks>
public partial class Overrides {
    /// <summary>Address of the click handler for flat strategic map view.</summary>
    public const ushort CLICK_HANDLER_FLAT_MAP = 0x1A9E;

    /// <summary>Address of the click handler for globe/planetary view.</summary>
    public const ushort CLICK_HANDLER_GLOBE_MAP = 0x2562;

    /// <summary>Address of the click handler for in-game exploration view.</summary>
    public const ushort CLICK_HANDLER_INGAME = 0x2572;

    /// <summary>Address of the click handler for troop movement map.</summary>
    public const ushort CLICK_HANDLER_MOVE_TROOP_MAP = 0x1AAC;

    /// <summary>Address of the click handler for ornithopter flight map.</summary>
    public const ushort CLICK_HANDLER_ORNI_MAP = 0x1AC8;

    /// <summary>
    /// Registers map view and cursor handling function overrides with Spice86.
    /// </summary>
    public void DefineMapCodeOverrides() {
        DefineFunction(cs1, 0xD95B, SetMapClickHandlerAddressToInGame_1000_D95B_01D95B);
        DefineFunction(cs1, 0xD95E, SetMapClickHandlerAddressFromAx_1000_D95E_01D95E);
        DefineFunction(cs1, 0xDAA3, InitMapCursorTypeDC58To0_1000_DAA3_01DAA3);
        DefineFunction(cs1, 0xDAAA, SetSiToMapCursorTypeDC58_1000_DAAA_01DAAA);
        DefineFunction(cs1, 0x5B96, UnknownMemcopy_1000_5B96_015B96);
        DefineFunction(cs1, 0x5E4F, CalcSalIndex_1000_5E4F_015E4F);
        DefineFunction(cs1, 0xB5A0, SineCosineLookupBx_1000_B5A0_01B5A0);
        DefineFunction(cs1, 0xB58B, ComputePolarOffset_1000_B58B_01B58B);
        DefineFunction(cs1, 0xB5C5, DivideByBp_1000_B5C5_01B5C5);
        DefineFunction(cs1, 0x55DD, ScanSpriteSheetClearBit4_1000_55DD_0155DD);
        DefineFunction(cs1, 0xBFE3, ComputeFremenSpicePercentages_1000_BFE3_01BFE3);
        DefineFunction(cs1, 0xB977, GlobeFarBlit_1000_B977_01B977);
        DefineFunction(cs1, 0xB6C3, GlobeRotationDispatch_1000_B6C3_01B6C3);
    }

    /// <summary>
    /// Override for cs1:0xB6C3 — Phase 33 globe-rotation indirect-far dispatch.
    /// </summary>
    /// <remarks>
    /// Asm (first 80 bytes, B6C3..B713):
    /// <code>
    /// B6C3: F6 06 EB 46 80         test byte [0x46EB], 0x80
    /// B6C8: 74 4A                    jz  B714                  ; alternative-path tail
    /// B6CA: 1E 07                    push ds; pop es           ; ES = DS
    /// B6CC: C7 06 F6 DC A0 00        mov word [0xDCF6], 0x00A0
    /// B6D2: C7 06 F8 DC 4C 00        mov word [0xDCF8], 0x004C
    /// B6D8: B9 12 00                  mov cx, 0x12
    /// B6DB: BB 4B 00                  mov bx, 0x4B
    /// B6DE: A1 7E 19                  mov ax, [0x197E]
    /// B6E1: 0B C0                    or  ax, ax
    /// B6E3: 8B D0                    mov dx, ax
    /// B6E5: 79 02                    jns B6E9                  ; abs value
    /// B6E7: F7 D8                    neg ax
    /// B6E9: 3B C3                    cmp ax, bx
    /// B6EB: 72 0B                    jc  B6F8                  ; within range
    /// B6ED: 8B C3                    mov ax, bx                ; clamp
    /// B6EF: 0B D2                    or  dx, dx
    /// B6F1: 79 02                    jns B6F5
    /// B6F3: F7 D8                    neg ax                    ; restore sign
    /// B6F5: A3 7E 19                  mov [0x197E], ax
    /// B6F8: BD 48 49                  mov bp, 0x4948            ; sin/cos table base
    /// B6FB: 8B 16 7C 19               mov dx, [0x197C]
    /// B6FF: A1 7E 19                  mov ax, [0x197E]
    /// B702: 2B C1                    sub ax, cx
    /// B704: C4 3E FE DC               les di, [0xDCFE]
    /// B708: BE 60 4C                  mov si, 0x4C60
    /// B70B: 8B 1E DA DB               mov bx, [0xDBDA]
    /// B70F: FF 1E 29 39               call far [0x3929]
    /// B713: C3                       ret
    /// B714: ...                      (alternative-path body, still asm)
    /// </code>
    /// Two paths:
    /// <list type="bullet">
    /// <item><description>Bit 7 of <c>[0x46EB]</c> set: clamps the globe-rotation angle at <c>[0x197E]</c> to [-0x4B, +0x4B], then indirect-far-calls through <c>[0x3929]</c> with the globe-render state pre-staged.</description></item>
    /// <item><description>Bit 7 clear: tail-jumps to <c>cs1:0xB714</c> (asm-side alternative-path body, ~32+ bytes; not yet ported).</description></item>
    /// </list>
    /// </remarks>
    public Action GlobeRotationDispatch_1000_B6C3_01B6C3(int gotoAddress) {
        byte flag = UInt8[DS, 0x46EB];
        if ((flag & 0x80) == 0) {
            // jz B714 — alternative path, asm
            return NearJump(0xB714);
        }
        // push ds; pop es — ES = DS via the stack write the asm performs
        ushort savedDs = DS;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = savedDs;
        ES = UInt16[SS, SP];
        SP = (ushort)(SP + 2);

        UInt16[DS, 0xDCF6] = 0x00A0;
        UInt16[DS, 0xDCF8] = 0x004C;

        ushort cx = 0x12;
        CX = cx;
        ushort bx = 0x4B;
        BX = bx;

        ushort ax = UInt16[DS, 0x197E];
        AX = ax;
        DX = ax;
        // jns +2: if ax >= 0 (signed bit clear), skip neg
        if ((ax & 0x8000) != 0) {
            ax = (ushort)(-(short)ax);
            AX = ax;
        }
        // cmp ax, bx; jc B6F8
        if (ax >= bx) {
            // clamp at 0x4B (preserving sign)
            ushort clamped = bx;
            // or dx, dx; jns +2 — if dx was negative, neg the clamped value
            if ((DX & 0x8000) != 0) {
                clamped = (ushort)(-(short)clamped);
            }
            AX = clamped;
            UInt16[DS, 0x197E] = clamped;
        }

        // B6F8: bp = 0x4948; dx = [0x197C]; ax = [0x197E]; ax -= cx
        BP = 0x4948;
        DX = UInt16[DS, 0x197C];
        AX = UInt16[DS, 0x197E];
        AX = (ushort)(AX - CX);

        // les di, [0xDCFE]
        SegmentedAddress p = globalsOnDs.GetPtr1138_DCFE_Dword32();
        ES = p.Segment;
        DI = p.Offset;

        SI = 0x4C60;
        BX = globalsOnDs.Get1138_DBDA_Word16_framebufferActive();

        // call far [0x3929]
        ushort targetOff = UInt16[DS, 0x3929];
        ushort targetSeg = UInt16[DS, 0x392B];
        // Far-call: push CS, push IP (0xB713 — the bare-ret byte at the function tail)
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xB713;
        return FarJump(targetSeg, targetOff);
    }

    /// <summary>
    /// Override for cs1:0xB977 — Phase 33 globe-render indirect-far blit dispatch.
    /// </summary>
    /// <remarks>
    /// Asm (20 bytes):
    /// <code>
    /// B977: BD 48 49           mov bp, 0x4948
    /// B97A: 8E 06 D6 DB        mov es, [0xDBD6]
    /// B97E: A0 02 DD           mov al, [0xDD02]
    /// B981: C5 36 FE DC        lds si, [0xDCFE]
    /// B985: 36 FF 1E 11 39     call far [ss:0x3911]
    /// B98A: C3                 ret
    /// </code>
    /// Sets up BP=sin/cos-table base, ES=front-framebuffer-segment, AL=globe-state-byte,
    /// loads DS:SI from a far pointer at DS:0xDCFE, then indirect-far-calls through the
    /// function-pointer slot at SS:0x3911 (typically a globe-render routine in cs2).
    /// <para>
    /// DS is permanently modified by the LDS — callers must save/restore DS themselves
    /// or accept the new DS value (preserved here byte-faithfully).
    /// </para>
    /// </remarks>
    public Action GlobeFarBlit_1000_B977_01B977(int gotoAddress) {
        BP = 0x4948;
        ES = globalsOnDs.Get1138_DBD6_Word16_framebufferFront();
        AL = (byte)globalsOnDs.Get1138_DD02_Byte8();
        // lds si, [0xDCFE]
        SegmentedAddress p = globalsOnDs.GetPtr1138_DCFE_Dword32();
        SI = p.Offset;
        DS = p.Segment;
        // call far [ss:0x3911]
        ushort targetOff = UInt16[SS, 0x3911];
        ushort targetSeg = UInt16[SS, 0x3913];
        // Far call: push CS, push IP (0xB98A — the bare-ret byte)
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xB98A;
        return FarJump(targetSeg, targetOff);
    }

    public Action InitMapCursorTypeDC58To0_1000_DAA3_01DAA3(int gotoAddress) {
        // when 0 or any other value, map cursor is disabled for globe / orni.
        this.globalsOnDs.Set1138_DC58_Word16_MapCursorType(0);
        return NearRet();
    }

    public Action SetMapClickHandlerAddressFromAx_1000_D95E_01D95E(int gotoAddress) {
        globalsOnDs.Set1138_2570_Word16_MapClickHandlerAddress(AX);
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Debug)) {
            _loggerService.Debug("setMapClickHandlerAddressFromAx: DS:{@Ds}, AX:{@Ax}", ConvertUtils.ToHex16(DS), ConvertUtils.ToHex16(AX));
        }

        return NearRet();
    }

    public Action SetMapClickHandlerAddressToInGame_1000_D95B_01D95B(int gotoAddress) {
        // called when starting to fly the orni, exiting maps and when switching from intro to game
        // at load time
        // See setMapClickHandlerAddressFromAx_1ED_D95E_F82E
        AX = CLICK_HANDLER_INGAME;
        return SetMapClickHandlerAddressFromAx_1000_D95E_01D95E(0);
    }

    public Action SetSiToMapCursorTypeDC58_1000_DAAA_01DAAA(int gotoAddress) {
        // when taking an orni: 0x149C, when loading globe or results: 0x2448
        ushort value = SI;
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Debug)) {
            _loggerService.Debug("setSiToMapCursorTypeDC58: value:{@Value}", ConvertUtils.ToHex16(value));
        }

        this.globalsOnDs.Set1138_DC58_Word16_MapCursorType(value);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x5E4F — <c>calc_SAL_index_ida</c>. Increments AX once for each
    /// of the thresholds <c>0x20, 0x21, 0x28, 0x30</c> that <c>[si+0x08]</c> meets or exceeds.
    /// </summary>
    /// <remarks>
    /// Asm (28 bytes):
    /// <code>
    /// 5E4F: 8A 4C 08         mov cl, [si+0x08]
    /// 5E52: 80 F9 20         cmp cl, 0x20; 5E55: 72 13 jc 5E6A
    /// 5E57: 40               inc ax
    /// 5E58: 80 F9 21         cmp cl, 0x21; 5E5B: 72 0D jc 5E6A
    /// 5E5D: 40               inc ax
    /// 5E5E: 80 F9 28         cmp cl, 0x28; 5E61: 72 07 jc 5E6A
    /// 5E63: 40               inc ax
    /// 5E64: 80 F9 30         cmp cl, 0x30; 5E67: 72 01 jc 5E6A
    /// 5E69: 40               inc ax
    /// 5E6A: C3               ret
    /// </code>
    /// Pure leaf — counts how many of four thresholds the byte at <c>[si+0x08]</c> equals
    /// or exceeds, and adds that count to AX. Used by SAL polygon classification.
    /// </remarks>
    public Action CalcSalIndex_1000_5E4F_015E4F(int gotoAddress) {
        byte cl = UInt8[DS, (ushort)(SI + 0x08)];
        CL = cl;
        ushort ax = AX;
        if (cl >= 0x20) ax = (ushort)(ax + 1);
        if (cl >= 0x21) ax = (ushort)(ax + 1);
        if (cl >= 0x28) ax = (ushort)(ax + 1);
        if (cl >= 0x30) ax = (ushort)(ax + 1);
        AX = ax;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xB5A0 — sine/cosine table lookup with sign handling.
    /// </summary>
    /// <remarks>
    /// Asm (37 bytes, B5A0..B5C4):
    /// <code>
    /// B5A0: 53                push bx
    /// B5A1: D1 E3             shl bx, 1   ; bx *= 8
    /// B5A3: D1 E3             shl bx, 1
    /// B5A5: D1 E3             shl bx, 1
    /// B5A7: 79 10             jns B5B9                ; if (bx after shifts) ≥ 0 → positive path
    /// B5A9: F7 DB             neg bx                  ; (negative path)
    /// B5AB: 8B 87 48 49       mov ax, [bx+0x4948]
    /// B5AF: F7 D8             neg ax
    /// B5B1: 8B AF 4A 49       mov bp, [bx+0x494A]
    /// B5B5: D1 E5             shl bp, 1
    /// B5B7: 5B                pop bx
    /// B5B8: C3                ret
    /// B5B9: 8B 87 48 49       mov ax, [bx+0x4948]
    /// B5BD: 8B AF 4A 49       mov bp, [bx+0x494A]
    /// B5C1: D1 E5             shl bp, 1
    /// B5C3: 5B                pop bx
    /// B5C4: C3                ret
    /// </code>
    /// Pure leaf. Sin/cos table at <c>ds:0x4948</c> (sin) and <c>ds:0x494A</c> (cos),
    /// indexed by BX*8 with sign-mirror for the negative half-circle.
    /// </remarks>
    public Action SineCosineLookupBx_1000_B5A0_01B5A0(int gotoAddress) {
        // push bx
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = BX;

        ushort bxShifted = (ushort)(BX << 3);  // bx *= 8 (three shl by 1)
        BX = bxShifted;

        // jns B5B9 — jump if bit 15 of bxShifted clear (positive)
        if ((bxShifted & 0x8000) == 0) {
            // Positive path
            AX = UInt16[DS, (ushort)(bxShifted + 0x4948)];
            ushort bp = UInt16[DS, (ushort)(bxShifted + 0x494A)];
            BP = (ushort)(bp << 1);
        } else {
            // Negative path
            ushort negBx = (ushort)(-(short)bxShifted);
            BX = negBx;
            ushort ax = UInt16[DS, (ushort)(negBx + 0x4948)];
            AX = (ushort)(-(short)ax);
            ushort bp = UInt16[DS, (ushort)(negBx + 0x494A)];
            BP = (ushort)(bp << 1);
        }
        // pop bx
        BX = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xB58B — compute polar offset into the framebuffer pointer at
    /// <c>ds:[0xDCFE]</c>. Wraps <see cref="SineCosineLookupBx_1000_B5A0_01B5A0"/>.
    /// </summary>
    /// <remarks>
    /// Asm (20 bytes):
    /// <code>
    /// B58B: E8 12 00         call 0xB5A0
    /// B58E: C4 3E FE DC      les di, [0xDCFE]
    /// B592: 03 F8            add di, ax
    /// B594: 8B C5            mov ax, bp
    /// B596: F7 E2            mul dx
    /// B598: D1 E0            shl ax, 1
    /// B59A: 83 D2 00         adc dx, 0
    /// B59D: 03 FA            add di, dx
    /// B59F: C3                ret
    /// </code>
    /// <c>0xB5A0</c> is C#; called directly. Mirror the asm <c>call</c>'s 2-byte stack
    /// push of <c>0xB58E</c> for memdiff parity.
    /// </remarks>
    public Action ComputePolarOffset_1000_B58B_01B58B(int gotoAddress) {
        // call 0xB5A0 — direct C# invocation; mirror asm stack push of 0xB58E
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xB58E;
        SineCosineLookupBx_1000_B5A0_01B5A0(0);
        SP = (ushort)(SP + 2);

        // les di, [0xDCFE]
        SegmentedAddress p = globalsOnDs.GetPtr1138_DCFE_Dword32();
        ES = p.Segment;
        DI = p.Offset;
        // add di, ax
        DI = (ushort)(DI + AX);
        // mov ax, bp
        AX = BP;
        // mul dx — DX:AX = AX * DX
        uint product = (uint)AX * DX;
        ushort axMul = (ushort)(product & 0xFFFF);
        ushort dxMul = (ushort)(product >> 16);
        AX = axMul;
        DX = dxMul;
        // shl ax, 1
        AX = (ushort)(AX << 1);
        // adc dx, 0 — DX += carry from the shl
        bool carry = (axMul & 0x8000) != 0;  // top bit of pre-shift AX was the new CF
        if (carry) DX = (ushort)(DX + 1);
        // add di, dx
        DI = (ushort)(DI + DX);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xB5C5 — divides (zero-extended AX, DX) by BP and returns DX = quotient.
    /// Wraps <see cref="ComputePolarOffset_1000_B58B_01B58B"/>.
    /// </summary>
    /// <remarks>
    /// Asm (10 bytes):
    /// <code>
    /// B5C5: E8 C3 FF         call 0xB58B
    /// B5C8: 33 C0             xor ax, ax
    /// B5CA: F7 F5             div bp                 ; AX = (DX:AX) / BP, DX = remainder
    /// B5CC: 8B D0             mov dx, ax
    /// B5CE: C3                 ret
    /// </code>
    /// </remarks>
    public Action DivideByBp_1000_B5C5_01B5C5(int gotoAddress) {
        // call 0xB58B
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xB5C8;
        ComputePolarOffset_1000_B58B_01B58B(0);
        SP = (ushort)(SP + 2);
        // xor ax, ax
        AX = 0;
        // div bp — AX = (DX:AX) / BP, DX = remainder
        if (BP != 0) {
            uint dividend = ((uint)DX << 16) | AX;
            ushort quotient = (ushort)(dividend / BP);
            ushort remainder = (ushort)(dividend % BP);
            AX = quotient;
            DX = remainder;
        }
        // mov dx, ax
        DX = AX;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0x55DD — scans the sprite-sheet buffer at the far pointer in
    /// <c>ds:[0xDBB0]</c> for byte value <c>AL</c>; when a matching cell has its high
    /// nibble equal to <c>0x30</c>, clears bit 4 of the cell.
    /// </summary>
    /// <remarks>
    /// Asm (40 bytes, 55DD..5604):
    /// <code>
    /// 55DD: 1E                push ds
    /// 55DE: 0A C0             or  al, al
    /// 55E0: 74 21              jz  5603                  ; AL == 0 → exit
    /// 55E2: C4 3E B0 DB        les di, [0xDBB0]          ; ES:DI = sprite-sheet far ptr
    /// 55E6: 8E 1E 00 DD        mov ds, [0xDD00]
    /// 55EA: B9 F9 C5            mov cx, 0xC5F9
    /// 55ED: F2 AE                repne scasb              ; scan ES:DI for AL
    /// 55EF: 75 12                jnz 5603                  ; no match → exit
    /// 55F1: 8A 65 FF             mov ah, [di-0x01]
    /// 55F4: 80 E4 30             and ah, 0x30
    /// 55F7: 80 FC 30             cmp ah, 0x30
    /// 55FA: 75 04                jnz 5600
    /// 55FC: 80 65 FF EF          and byte [di-0x01], 0xEF
    /// 5600: 41                   inc cx
    /// 5601: E2 EA                loop 55ED
    /// 5603: 1F                   pop ds
    /// 5604: C3                   ret
    /// </code>
    /// Pure leaf. The <c>inc cx; loop</c> at 5600/5601 is the cx=0-safe loop-back idiom
    /// (avoids the standard <c>loop</c> 65536-iter wrap when cx underflows).
    /// </remarks>
    public Action ScanSpriteSheetClearBit4_1000_55DD_0155DD(int gotoAddress) {
        // push ds
        ushort savedDs = DS;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = savedDs;

        if (AL == 0) {
            // jz 5603 → pop ds; ret
            DS = UInt16[SS, SP];
            SP = (ushort)(SP + 2);
            return NearRet();
        }
        // les di, [0xDBB0]
        SegmentedAddress sprite = globalsOnDs.GetPtr1138_DBB0_Dword32_spriteSheetResourcePointer();
        ES = sprite.Segment;
        DI = sprite.Offset;
        // mov ds, [0xDD00]
        ushort newDs = (ushort)globalsOnDs.Get1138_DD00_Word16();
        DS = newDs;
        // mov cx, 0xC5F9
        ushort cx = 0xC5F9;
        CX = cx;
        byte al = AL;

        while (cx > 0) {
            // repne scasb: scan ES:DI for AL, max cx iterations
            bool found = false;
            while (cx > 0) {
                byte cell = UInt8[ES, DI];
                DI = (ushort)(DI + 1);
                cx = (ushort)(cx - 1);
                if (cell == al) {
                    found = true;
                    break;
                }
            }
            if (!found) {
                // jnz 5603 — exit
                break;
            }
            // mov ah, [di-1] — read with default DS (= new DS)
            byte ah = UInt8[DS, (ushort)(DI - 1)];
            byte ahMasked = (byte)(ah & 0x30);
            AH = ahMasked;
            if (ahMasked == 0x30) {
                // and byte [di-1], 0xEF
                UInt8[DS, (ushort)(DI - 1)] = (byte)(ah & 0xEF);
            }
            // inc cx; loop -0x16 — net effect: re-enter scasb with same cx
            // (asm-faithful: the inc+loop is the cx=0-safe form). The outer while
            // already checks cx > 0 at top, so we continue naturally.
        }
        CX = cx;
        // pop ds
        DS = UInt16[SS, SP];
        SP = (ushort)(SP + 2);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xBFE3 — scans the planet's sprite-sheet buffer at the segment
    /// in <c>ds:[0xDD00]</c> for cells with bits 4-5 set in the high nibble, then
    /// computes two percentages and publishes them at <c>ds[0x00A2]</c> / <c>ds[0x00A4]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (75 bytes, BFE3..C02D):
    /// <code>
    /// BFE3: 1E              push ds
    /// BFE4: 33 DB           xor bx, bx
    /// BFE6: 33 D2           xor dx, dx
    /// BFE8: B9 F9 C5        mov cx, 0xC5F9
    /// BFEB: 33 F6           xor si, si
    /// BFED: 8E 1E 00 DD     mov ds, [0xDD00]
    /// BFF1: AC              lodsb
    /// BFF2: 24 30           and al, 0x30
    /// BFF4: 74 06           jz BFFC
    /// BFF6: 42              inc dx
    /// BFF7: 3C 30           cmp al, 0x30
    /// BFF9: 74 01           jz BFFC
    /// BFFB: 43              inc bx
    /// BFFC: E2 F3           loop BFF1
    /// BFFE: 2B D3           sub dx, bx               ; dx_filled - bx_partial = both-bits-set count
    /// C000: 33 C0           xor ax, ax
    /// C002: 81 EE 88 01     sub si, 0x0188
    /// C006: 46              inc si                    ; si = denominator
    /// C007: F7 F6           div si                    ; ax = (dx &lt;&lt; 16) / si
    /// C009: BA 64 00        mov dx, 0x64
    /// C00C: F7 E2           mul dx                    ; dx:ax = ax * 100
    /// C00E: 03 C0           add ax, ax               ; dx:ax *= 2 (24-bit fits → faithful)
    /// C010: 83 D2 00        adc dx, 0
    /// C013: 87 D3           xchg dx, bx              ; bx = doubled-quotient-high; dx = bx_partial
    /// C015: 33 C0           xor ax, ax
    /// C017: F7 F6           div si
    /// C019: BA 64 00        mov dx, 0x64
    /// C01C: F7 E2           mul dx
    /// C01E: 03 C0           add ax, ax
    /// C020: 83 D2 00        adc dx, 0
    /// C023: 42              inc dx
    /// C024: 1F              pop ds
    /// C025: 89 16 A2 00     mov [0x00A2], dx
    /// C029: 89 1E A4 00     mov [0x00A4], bx
    /// C02D: C3              ret
    /// </code>
    /// Counts buffer cells where bits 4 OR 5 of the high nibble are set, separately
    /// counts where exactly one is set, then computes <c>(count * 0x10000 / denominator) *
    /// 100 * 2</c> for each — a 16.16 fixed-point percentage. Pure leaf.
    /// </remarks>
    public Action ComputeFremenSpicePercentages_1000_BFE3_01BFE3(int gotoAddress) {
        // push ds
        ushort savedDs = DS;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = savedDs;

        ushort bx = 0; BX = 0;
        ushort dx = 0; DX = 0;
        ushort cx = 0xC5F9; CX = cx;
        ushort si = 0; SI = 0;
        ushort scanSeg = (ushort)globalsOnDs.Get1138_DD00_Word16();
        DS = scanSeg;

        // Scan loop
        do {
            byte al = UInt8[DS, si];
            si = (ushort)(si + 1);
            byte masked = (byte)(al & 0x30);
            AL = masked;
            if (masked != 0) {
                dx = (ushort)(dx + 1);
                if (masked != 0x30) {
                    bx = (ushort)(bx + 1);
                }
            }
            cx = (ushort)(cx - 1);
        } while (cx != 0);
        CX = 0;
        SI = si;
        BX = bx;
        DX = dx;

        // sub dx, bx → dx = "both-bits-set" count
        dx = (ushort)(dx - bx);
        DX = dx;
        // xor ax, ax
        AX = 0;
        // sub si, 0x0188; inc si
        si = (ushort)(si - 0x0188);
        si = (ushort)(si + 1);
        SI = si;

        // Helper for the (dx_in, si_in) → ((dx_in << 16) / si * 100 * 2) doubled-product flow.
        // Returns the high-word of the doubled product.
        ushort ComputeScaledHigh(ushort dxIn) {
            if (si == 0) return 0;
            uint dividend = (uint)dxIn << 16;
            ushort q = (ushort)(dividend / si);
            // mul dx (= 100): dx_mul:ax_mul = q * 100
            uint prod = (uint)q * 100u;
            ushort axMul = (ushort)(prod & 0xFFFF);
            ushort dxMul = (ushort)(prod >> 16);
            // add ax, ax; adc dx, 0 → ax doubled; CF propagates into dx
            bool carry = (axMul & 0x8000) != 0;
            dxMul = (ushort)(dxMul + (carry ? 1 : 0));
            // The doubled AX itself isn't returned — only DX is stored later.
            // (The asm leaves the new AX in AX but doesn't store it.)
            return dxMul;
        }

        // First pass — dx = "both-bits-set" count
        ushort firstHigh = ComputeScaledHigh(dx);
        // xchg dx, bx — DX gets old BX (partial count), BX gets the doubled-quotient-high
        ushort tmp = bx;
        bx = firstHigh;
        dx = tmp;
        BX = bx;
        DX = dx;

        // Second pass — dx = "exactly one bit set" count
        ushort secondHigh = ComputeScaledHigh(dx);
        secondHigh = (ushort)(secondHigh + 1);  // inc dx at C023
        dx = secondHigh;
        DX = dx;

        // pop ds — restore caller's DS
        DS = UInt16[SS, SP];
        SP = (ushort)(SP + 2);

        // mov [0x00A2], dx
        UInt16[DS, 0x00A2] = dx;
        // mov [0x00A4], bx
        UInt16[DS, 0x00A4] = bx;
        return NearRet();
    }

    public Action UnknownMemcopy_1000_5B96_015B96(int gotoAddress) {
        // Called on map display / move, data to be copied never seems to change.
        uint sourceAddress = MemoryUtils.ToPhysicalAddress(DS, 0x46e3);
        uint destinationAddress = MemoryUtils.ToPhysicalAddress(DS, DI);
        Memory.MemCopy(sourceAddress, destinationAddress, 4 * 2);
        return NearRet();
    }
}