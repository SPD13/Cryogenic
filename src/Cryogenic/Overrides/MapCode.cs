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
        DefineFunction(cs1, 0xB714, GlobeRotationAltPath_1000_B714_01B714);
        DefineFunction(cs1, 0xB7D2, GlobeBandScanlineCopy_1000_B7D2_01B7D2);
        DefineFunction(cs1, 0xB427, SavegameMapOverlayExpand_1000_B427_01B427);
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
    /// <item><description>Bit 7 clear: delegates to the bit-7-clear alternative path at
    /// <c>cs1:0xB714</c> — now fully ported (see <see cref="GlobeRotationAltPath_1000_B714_01B714"/>).</description></item>
    /// </list>
    /// </remarks>
    public Action GlobeRotationDispatch_1000_B6C3_01B6C3(int gotoAddress) {
        byte flag = UInt8[DS, 0x46EB];
        if ((flag & 0x80) == 0) {
            // jz B714 — bit-7-clear alternative path (now ported to C#)
            return GlobeRotationAltPath_1000_B714_01B714(gotoAddress);
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

    /// <summary>
    /// Override for cs1:0xB714 — the bit-7-clear branch of <see cref="GlobeRotationDispatch_1000_B6C3_01B6C3"/>
    /// (Phase 33 globe rotation). Completes the previously-partial dispatch: computes the
    /// viewport-derived globe-render parameters into <c>[0xDCF2..0xDCF8]</c>, clamps the
    /// rotation angle at <c>[0x197E]</c>, walks the TABLAT band table at <c>0x4948</c> (either
    /// downward for a negative band or upward for a positive band, with the engine's
    /// zero-<c>mapRowStart</c> direction-flip), assembling each band's pixel run into the
    /// globe scratch buffer via <see cref="GlobeBandScanlineCopyCore"/> (cs1:0xB7D2), then
    /// indirect-far-dispatches the assembled frame through <c>[0x390D]</c> (unless
    /// <c>[0x46EB] &amp; 0x40</c> is set, in which case the blit is skipped).
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xB714..0xB7D1):
    /// <code>
    /// B714: BF 60 4C            mov di,0x4C60
    /// B717: A1 E7 46            mov ax,[0x46E7]
    /// B71A: 2B 06 E3 46         sub ax,[0x46E3]
    /// B71E: 8B D0               mov dx,ax
    /// B720: D1 EA               shr dx,1
    /// B722: 03 16 E3 46         add dx,[0x46E3]
    /// B726: 89 16 F6 DC         mov [0xDCF6],dx
    /// B72A: A3 F2 DC            mov [0xDCF2],ax
    /// B72D: A1 E9 46            mov ax,[0x46E9]
    /// B730: 2B 06 E5 46         sub ax,[0x46E5]
    /// B734: 48                  dec ax
    /// B735: 8B D8               mov bx,ax
    /// B737: D1 EB               shr bx,1
    /// B739: 03 1E E5 46         add bx,[0x46E5]
    /// B73D: 89 1E F8 DC         mov [0xDCF8],bx
    /// B741: 40                  inc ax
    /// B742: A3 F4 DC            mov [0xDCF4],ax
    /// B745: 48                  dec ax
    /// B746: D1 E8               shr ax,1
    /// B748: 8B C8               mov cx,ax
    /// B74A: BB 56 00            mov bx,0x0056
    /// B74D: 2B D8               sub bx,ax
    /// B74F: A1 7E 19            mov ax,[0x197E]
    /// B752: 0B C0               or  ax,ax
    /// B754: 8B D0               mov dx,ax
    /// B756: 79 02               jns B75A
    /// B758: F7 D8               neg ax
    /// B75A: 3B C3               cmp ax,bx
    /// B75C: 72 0B               jc  B769
    /// B75E: 8B C3               mov ax,bx
    /// B760: 0B D2               or  dx,dx
    /// B762: 79 02               jns B766
    /// B764: F7 D8               neg ax
    /// B766: A3 7E 19            mov [0x197E],ax
    /// B769: BD 48 49            mov bp,0x4948
    /// B76C: 8B 16 7C 19         mov dx,[0x197C]
    /// B770: A1 7E 19            mov ax,[0x197E]
    /// B773: 2B C1               sub ax,cx
    /// B775: 50                  push ax
    /// B776: 8B 0E F4 DC         mov cx,[0xDCF4]
    /// B77A: D1 E0 D1 E0 D1 E0   shl ax,1 (x3)            ; ax <<= 3
    /// B780: 79 1A               jns B79C                 ; positive band
    /// B782: F7 D8               neg ax
    /// B784: 03 E8               add bp,ax
    /// B786: 51                  push cx                  ; --- negative loop
    /// B787: 8B 4E 00            mov cx,[bp+0]
    /// B78A: 8B 5E 02            mov bx,[bp+2]
    /// B78D: F7 D9               neg cx
    /// B78F: 74 14               jz  B7A5                 ; zero mapRowStart -> flip to fwd
    /// B791: E8 3E 00            call B7D2
    /// B794: 83 ED 08            sub bp,8
    /// B797: 59                  pop cx
    /// B798: E2 EC               loop B786
    /// B79A: EB 12               jmp B7AE
    /// B79C: 03 E8               add bp,ax                ; --- positive band
    /// B79E: 51                  push cx                  ; --- forward loop
    /// B79F: 8B 4E 00            mov cx,[bp+0]
    /// B7A2: 8B 5E 02            mov bx,[bp+2]
    /// B7A5: E8 2A 00            call B7D2
    /// B7A8: 83 C5 08            add bp,8
    /// B7AB: 59                  pop cx
    /// B7AC: E2 F0               loop B79E
    /// B7AE: 8E 06 DA DB         mov es,[0xDBDA]
    /// B7B2: 8B 3E F2 DC         mov di,[0xDCF2]
    /// B7B6: 8B 0E F4 DC         mov cx,[0xDCF4]
    /// B7BA: 8B 16 E3 46         mov dx,[0x46E3]
    /// B7BE: 8B 1E E5 46         mov bx,[0x46E5]
    /// B7C2: BE 60 4C            mov si,0x4C60
    /// B7C5: 58                  pop ax
    /// B7C6: F6 06 EB 46 40      test byte [0x46EB],0x40
    /// B7CB: 75 04               jnz B7D1
    /// B7CD: FF 1E 0D 39         call far [0x390D]
    /// B7D1: C3                  ret
    /// </code>
    /// The <c>loop</c>s decrement the <c>[0xDCF4]</c>-derived band count saved across the
    /// <c>call B7D2</c> by the surrounding <c>push cx</c>/<c>pop cx</c>. <c>jz B7A5</c> jumps
    /// from the negative-band loop into the forward loop's call site, so a zero
    /// <c>mapRowStart</c> permanently flips the remaining iterations to the forward
    /// (<c>+8</c>) walk — replicated here via <c>forward</c>. Ambient CLD assumed.
    /// </remarks>
    public Action GlobeRotationAltPath_1000_B714_01B714(int gotoAddress) {
        // B714..B748 — derive globe-render parameters from the viewport rectangle.
        ushort ax = (ushort)(UInt16[DS, 0x46E7] - UInt16[DS, 0x46E3]);
        ushort dx = (ushort)(ax >> 1);
        dx = (ushort)(dx + UInt16[DS, 0x46E3]);
        UInt16[DS, 0xDCF6] = dx;
        UInt16[DS, 0xDCF2] = ax;

        ax = (ushort)(UInt16[DS, 0x46E9] - UInt16[DS, 0x46E5]);
        ax = (ushort)(ax - 1);                              // dec ax
        ushort bx = (ushort)(ax >> 1);
        bx = (ushort)(bx + UInt16[DS, 0x46E5]);
        UInt16[DS, 0xDCF8] = bx;
        ax = (ushort)(ax + 1);                              // inc ax
        UInt16[DS, 0xDCF4] = ax;
        ax = (ushort)(ax - 1);                              // dec ax
        ax = (ushort)(ax >> 1);
        ushort cx = ax;                                     // mov cx,ax

        // B74A..B766 — clamp the rotation angle at [0x197E] to [-(0x56-cx), +(0x56-cx)].
        bx = (ushort)(0x0056 - ax);                         // mov bx,0x56 ; sub bx,ax
        ax = UInt16[DS, 0x197E];
        dx = ax;                                            // mov dx,ax (sign keeper)
        if ((short)ax < 0) {                                // jns +2 : abs
            ax = (ushort)(-(short)ax);                      // neg ax
        }
        if (ax >= bx) {                                     // jc B769 (CF set => ax<bx => skip)
            ax = bx;                                        // clamp
            if ((short)dx < 0) {                            // or dx,dx ; jns +2
                ax = (ushort)(-(short)ax);                  // neg ax (restore sign)
            }
            UInt16[DS, 0x197E] = ax;
        }

        // B769..B77E — TABLAT base, band selector.
        ushort bp = 0x4948;
        dx = UInt16[DS, 0x197C];
        ax = UInt16[DS, 0x197E];
        ax = (ushort)(ax - cx);                             // sub ax,cx
        ushort savedAx = ax;                                // push ax (restored at B7C5)
        ushort loopCx = UInt16[DS, 0xDCF4];                 // mov cx,[0xDCF4]
        ax = (ushort)(ax << 3);                             // shl ax,1 (x3)

        bool forward;
        if ((short)ax >= 0) {                               // jns B79C — positive band
            bp = (ushort)(bp + ax);                         // B79C: add bp,ax
            forward = true;
        } else {                                            // B782 — negative band
            ax = (ushort)(-(short)ax);                      // neg ax
            bp = (ushort)(bp + ax);                         // add bp,ax
            forward = false;
        }

        // B786 / B79E — band-table walk. dx is preserved across B7D2 (the helper
        // restores it); di accumulates +0xC8 per band; the loop counter lives in loopCx.
        ushort di = 0x4C60;                                 // B714: mov di,0x4C60
        while (loopCx != 0) {
            ushort bandStart = UInt16[SS, (ushort)(bp + 0)]; // mov cx,[bp+0]
            ushort bandLen = UInt16[SS, (ushort)(bp + 2)];   // mov bx,[bp+2]
            if (!forward) {
                bandStart = (ushort)(-(short)bandStart);     // neg cx
                if (bandStart == 0) {                        // jz B7A5 — flip to forward
                    di = GlobeBandScanlineCopyCore(bandStart, bandLen, dx, bp, di);
                    bp = (ushort)(bp + 8);                   // B7A8: add bp,8
                    loopCx = (ushort)(loopCx - 1);           // pop cx ; loop B79E
                    forward = true;
                    continue;
                }
                di = GlobeBandScanlineCopyCore(bandStart, bandLen, dx, bp, di);
                bp = (ushort)(bp - 8);                       // B794: sub bp,8
                loopCx = (ushort)(loopCx - 1);               // pop cx ; loop B786
            } else {
                di = GlobeBandScanlineCopyCore(bandStart, bandLen, dx, bp, di);
                bp = (ushort)(bp + 8);                       // B7A8: add bp,8
                loopCx = (ushort)(loopCx - 1);               // pop cx ; loop B79E
            }
        }

        // B7AE..B7D1 — stage blit registers and dispatch.
        ES = UInt16[DS, 0xDBDA];
        DI = UInt16[DS, 0xDCF2];
        CX = UInt16[DS, 0xDCF4];
        DX = UInt16[DS, 0x46E3];
        BX = UInt16[DS, 0x46E5];
        SI = 0x4C60;
        AX = savedAx;                                       // pop ax
        BP = bp;
        if ((UInt8[DS, 0x46EB] & 0x40) != 0) {              // jnz B7D1 — skip blit
            return NearRet();
        }
        // B7CD: call far [0x390D] (DS-relative); continuation = the raw-asm ret @0xB7D1.
        ushort targetOff = UInt16[DS, 0x390D];
        ushort targetSeg = UInt16[DS, 0x390F];
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xB7D1;
        return FarJump(targetSeg, targetOff);
    }

    /// <summary>
    /// Override for cs1:0xB7D2 — per-band globe scanline assembler. Reloads DS:SI from the
    /// MAP.HSQ far pointer at <c>[0xDCFE]</c>, stores the rotation product's high word into
    /// the band record's <c>fp</c> field (<c>[bp+6]</c>), then copies the band's centred
    /// pixel run from the map buffer into the globe scratch buffer (<c>ES=SS</c>) via two
    /// <c>rep movsb</c> phases. DS/DI/DX are restored on exit; DI is advanced by 200
    /// (one globe raster row). Pure compute — no driver/disk/INT/indirect dependencies.
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xB7D2..0xB826):
    /// <code>
    /// B7D2: 52 57 1E            push dx; push di; push ds
    /// B7D5: C5 36 FE DC         lds si,[0xDCFE]
    /// B7D9: 16 07               push ss; pop es          ; es = ss
    /// B7DB: 03 F1               add si,cx
    /// B7DD: 03 DB               add bx,bx
    /// B7DF: 8B C2 F7 E3         mov ax,dx ; mul bx        ; dx:ax = dx*bx
    /// B7E3: 89 56 06            mov [bp+6],dx
    /// B7E6: 8B C2               mov ax,dx
    /// B7E8: 36 8B 16 F2 DC      ss: mov dx,[0xDCF2]
    /// B7ED: 3B DA               cmp bx,dx
    /// B7EF: 73 0A               jnc B7FB
    /// B7F1: 8B CA 2B CB         mov cx,dx ; sub cx,bx
    /// B7F5: D1 E9               shr cx,1
    /// B7F7: 03 F9               add di,cx
    /// B7F9: 8B D3               mov dx,bx
    /// B7FB: 8B CA               mov cx,dx
    /// B7FD: D1 E9               shr cx,1
    /// B7FF: 2B C1               sub ax,cx
    /// B801: 79 02               jns B805
    /// B803: 03 C3               add ax,bx
    /// B805: 8B CA               mov cx,dx
    /// B807: 2B D8               sub bx,ax
    /// B809: 2B CB               sub cx,bx
    /// B80B: 79 06               jns B813
    /// B80D: 03 CB 03 F0 EB 0A   add cx,bx ; add si,ax ; jmp B81D
    /// B813: 87 D9               xchg bx,cx
    /// B815: 56 03 F0            push si ; add si,ax
    /// B818: F3 A4               rep movsb
    /// B81A: 5E 87 D9            pop si ; xchg bx,cx
    /// B81D: F3 A4               rep movsb
    /// B81F: 1F 5F 5A            pop ds ; pop di ; pop dx
    /// B822: 81 C7 C8 00         add di,0x00C8
    /// B826: C3                  ret
    /// </code>
    /// The post-<c>cmp bx,dx</c> <c>jnc</c> tests CF (bx&lt;dx unsigned). The two
    /// <c>jns</c> branches test SF (signed non-negative). The <c>xchg bx,cx</c> pair around
    /// the first <c>rep movsb</c> swaps the run lengths so the two phases copy
    /// <c>bx'</c> bytes from <c>si+ax</c> then <c>cx'</c> bytes from the un-offset <c>si</c>.
    /// <c>rep</c> leaves CX=0. Ambient CLD assumed. The final DI is always
    /// <c>entryDI + 200</c> (the working DI used for the writes is discarded by <c>pop di</c>).
    /// </remarks>
    public Action GlobeBandScanlineCopy_1000_B7D2_01B7D2(int gotoAddress) {
        ushort entryDs = DS;
        ushort entryDx = DX;
        ushort entryDi = DI;
        ushort newDi = GlobeBandScanlineCopyCore(CX, BX, DX, BP, DI);
        // pop ds ; pop di ; pop dx  (restore), then add di,0xC8 (folded into newDi).
        DS = entryDs;
        DX = entryDx;
        _ = entryDi;
        DI = newDi;
        CX = 0;                                             // rep exhausts cx
        ES = SS;                                            // push ss; pop es
        AX = _b7d2Ax;
        BX = _b7d2Bx;
        SI = _b7d2Si;
        return NearRet();
    }

    private ushort _b7d2Ax;
    private ushort _b7d2Bx;
    private ushort _b7d2Si;

    /// <summary>
    /// Byte-faithful core of cs1:0xB7D2 (see <see cref="GlobeBandScanlineCopy_1000_B7D2_01B7D2"/>).
    /// Performs the band record write and the two <c>rep movsb</c> phases against the live
    /// emulated memory using the supplied register snapshot, and returns the function's net
    /// DI (<c>entryDI + 200</c>). The working AX/BX/SI the asm leaves in registers are
    /// stashed in <see cref="_b7d2Ax"/>/<see cref="_b7d2Bx"/>/<see cref="_b7d2Si"/> for the
    /// registered-entry override; the band-walk caller ignores them (it re-derives CX/BX
    /// from the next TABLAT record).
    /// </summary>
    private ushort GlobeBandScanlineCopyCore(ushort cxIn, ushort bxIn, ushort dxIn, ushort bp, ushort diIn) {
        SegmentedAddress p = globalsOnDs.GetPtr1138_DCFE_Dword32();  // lds si,[0xDCFE]
        ushort mapSeg = p.Segment;
        ushort si = (ushort)(p.Offset + cxIn);              // add si,cx
        ushort es = SS;                                     // push ss; pop es
        ushort bx = (ushort)(bxIn + bxIn);                  // add bx,bx
        uint product = (uint)dxIn * (uint)bx;               // mov ax,dx ; mul bx
        ushort dx = (ushort)(product >> 16);                // dx = high word
        UInt16[SS, (ushort)(bp + 6)] = dx;                  // mov [bp+6],dx (SS default seg)
        ushort ax = dx;                                     // mov ax,dx
        dx = UInt16[SS, 0xDCF2];                            // ss: mov dx,[0xDCF2]
        ushort di = diIn;
        if (bx < dx) {                                      // cmp bx,dx ; jnc B7FB (CF => bx<dx)
            ushort c = (ushort)(dx - bx);                   // mov cx,dx ; sub cx,bx
            c = (ushort)(c >> 1);                           // shr cx,1
            di = (ushort)(di + c);                          // add di,cx
            dx = bx;                                        // mov dx,bx
        }
        ushort cx = (ushort)(dx >> 1);                      // mov cx,dx ; shr cx,1
        ax = (ushort)(ax - cx);                             // sub ax,cx
        if ((short)ax < 0) {                                // jns B805
            ax = (ushort)(ax + bx);                         // add ax,bx
        }
        cx = dx;                                            // mov cx,dx
        bx = (ushort)(bx - ax);                             // sub bx,ax
        cx = (ushort)(cx - bx);                             // sub cx,bx
        if ((short)cx < 0) {                                // jns B813 (SF set => B80D)
            cx = (ushort)(cx + bx);                         // add cx,bx
            si = (ushort)(si + ax);                         // add si,ax
            for (int k = 0; k < cx; k++) {                  // rep movsb @B81D
                UInt8[es, di] = UInt8[mapSeg, si];
                si = (ushort)(si + 1);
                di = (ushort)(di + 1);
            }
            cx = 0;
        } else {                                            // B813
            ushort phase1 = cx;                             // xchg bx,cx -> rep count = old BX
            (bx, cx) = (cx, bx);                            // xchg bx,cx
            ushort siSave = si;                             // push si
            si = (ushort)(si + ax);                         // add si,ax
            for (int k = 0; k < cx; k++) {                  // rep movsb @B818 (count = oldBX)
                UInt8[es, di] = UInt8[mapSeg, si];
                si = (ushort)(si + 1);
                di = (ushort)(di + 1);
            }
            cx = 0;                                         // rep exhausts cx
            si = siSave;                                    // pop si
            (bx, cx) = (cx, bx);                            // xchg bx,cx -> cx = old CX
            for (int k = 0; k < cx; k++) {                  // rep movsb @B81D (count = oldCX)
                UInt8[es, di] = UInt8[mapSeg, si];
                si = (ushort)(si + 1);
                di = (ushort)(di + 1);
            }
            cx = 0;
            _ = phase1;
        }
        _b7d2Ax = ax;
        _b7d2Bx = bx;
        _b7d2Si = si;
        // pop di discards the working di; net DI = entryDI + 0xC8.
        return (ushort)(diIn + 0x00C8);
    }

    /// <summary>
    /// Override for cs1:0xB427 — <c>map_func_ida</c> / <c>sub_D2F7</c> (DNCDPRG.ASM:26736),
    /// the save-game map overlay <b>expander</b> (called from <c>subSaveSavegame</c>).
    /// Allocates a scratch block via <see cref="AllocCxPagesToDi_1000_F11C_01F11C"/>
    /// (cs1:0xF11C), then unpacks the 2-bit-packed MAP.HSQ cell stream (far ptr at
    /// <c>[0xDCFE]</c>) into one byte per cell at <c>ES:0x100</c> using the
    /// <c>AH=3</c>-seeded MSB shift-register decoder, and appends three fixed data
    /// blocks (from <c>cs1:0xAA</c>, the entry-DS region <c>0xAA76</c>, and entry-DS
    /// <c>0</c>). Returns <c>CX=0x567A</c>, <c>DI=0x100</c>, <c>ES</c>=scratch segment.
    /// Pure compute (the only call, cs1:0xF11C, is now C#) — exact static port.
    /// </summary>
    /// <remarks>
    /// Authoritative source: <c>DNCDPRG.ASM</c> <c>sub_D2F7</c> (lines 26736–26779),
    /// cross-verified against the cs1 dump bytes 0xB427..0xB473:
    /// <code>
    /// B427: B9 78 05      mov cx,0x578
    /// B42A: E8 EF 3C      call 0xF11C            ; alloc_cx_pages_to_di -> ES:DI
    /// B42D: BF 00 01      mov di,0x100
    /// B430: 57 06 1E      push di; push es; push ds
    /// B433: C5 36 FE DC   lds si,[0xDCFE]        ; DS=[0xDD00], SI=[0xDCFE]
    /// B437: 33 F6         xor si,si
    /// B439: B9 FC C5      mov cx,0xC5FC
    /// B43C: D1 E9 D1 E9   shr cx,1 ; shr cx,1    ; cx = 0x317F (cell count)
    /// loc_D310 0xB440: B4 03            mov ah,3            ; (loop target — re-seeds AH)
    /// loc_D312 0xB442: AC               lodsb
    ///          0xB443: D0 E0 D0 E0      shl al,1 ; shl al,1
    ///          0xB447: D1 E0 D1 E0      shl ax,1 ; shl ax,1
    ///          0xB44B: 73 F5            jnb loc_D312        ; while CF==0 keep reading
    ///          0xB44D: 8A C4 AA         mov al,ah ; stosb   ; emit accumulated AH
    ///          0xB450: E2 EE            loop loc_D310       ; (-> 0xB440)
    /// B452: 0E 1F         push cs; pop ds
    /// B454: BE AA 00 / B9 A2 00 / F3 A4 mov si,0xAA ; mov cx,0xA2  ; rep movsb (cs1:0xAA)
    /// B45D: 1F            pop ds                 ; DS = entry DS (pushed @B432)
    /// B45E: BE 76 AA / B9 F8 11 / F3 A4 mov si,0xAA76 ; mov cx,0x11F8 ; rep movsb
    /// B466: BE 00 00 / B9 61 12 / F3 A4 mov si,0 ; mov cx,0x1261 ; rep movsb
    /// B46E: 07 5F         pop es; pop di         ; ES=scratch, DI=0x100
    /// B470: B9 7A 56      mov cx,0x567A
    /// B473: C3            ret
    /// </code>
    /// The first-pass raw decode mis-read <c>E2 EE</c> as <c>loop 0xB442</c>; the
    /// authoritative ASM confirms <c>loop loc_D310</c> (0xB440), i.e. <c>AH</c> is
    /// re-seeded to 3 for <b>every</b> output byte and acts as a 6-shift terminator
    /// counter. The two <c>shl al,1</c> discard the input's top 2 bits; the two
    /// <c>shl ax,1</c> feed the remaining bits MSB-first through the <c>AH</c> shift
    /// register, emitting a byte when the seed bit shifts out (CF=1).
    /// </remarks>
    public Action SavegameMapOverlayExpand_1000_B427_01B427(int gotoAddress) {
        ushort entryDs = DS;

        // B427..B42A — inline cs1:0xF11C (alloc_cx_pages_to_di) to obtain ES.
        const ushort cxPages = 0x0578;
        ushort scratchSeg;
        while (true) {
            ushort e = UInt16[entryDs, 0x39B9];                 // les di,[0x39B7] -> ES=[0x39B9]
            ushort axp = (ushort)(e + cxPages);                 // mov ax,es ; add ax,cx
            if (axp < UInt16[entryDs, 0xCE68]) {                 // cmp ax,[0xCE68] ; jnc
                scratchSeg = e;
                break;
            }
            if (AllocatorFreeSpaceCore()) {                      // call 0xF13F ; (bail -> 0xF130)
                return NearJump(0xF130);
            }
        }

        // B42D..B43E — scratch dst at ES:0x100; MAP.HSQ src via lds [0xDCFE].
        ushort di = 0x0100;                                      // mov di,0x100 (di pushed/popped -> 0x100)
        ushort mapSeg = UInt16[entryDs, 0xDD00];                 // lds: DS = [0xDCFE+2]
        ushort si = 0;                                            // xor si,si
        int cellCount = 0xC5FC >> 2;                              // 0x317F

        // loc_D310/loc_D312 — AH=3-seeded MSB shift-register cell expander.
        ushort lastAh = 0x03;
        for (int n = 0; n < cellCount; n++) {                    // loop loc_D310 (cx=0x317F)
            ushort ax = 0x0300;                                  // mov ah,3 (AL set by lodsb)
            while (true) {                                        // loc_D312
                byte b = UInt8[mapSeg, si]; si = (ushort)(si + 1); // lodsb
                int al2 = (b << 2) & 0xFF;                        // shl al,1 ; shl al,1
                ax = (ushort)((ax & 0xFF00) | al2);              // AX = AH:AL
                ax = (ushort)((ax << 1) & 0xFFFF);               // shl ax,1 (1st)
                int t2 = ax << 1;                                 // shl ax,1 (2nd)
                bool cf = (t2 & 0x10000) != 0;
                ax = (ushort)(t2 & 0xFFFF);
                if (cf) {                                         // jnb loc_D312 (loop while CF==0)
                    break;
                }
            }
            lastAh = (ushort)(ax >> 8);                           // mov al,ah
            UInt8[scratchSeg, di] = (byte)lastAh;                 // stosb
            di = (ushort)(di + 1);
        }

        // B452..B46C — append three fixed data blocks after the expanded cells.
        for (int k = 0; k < 0xA2; k++) {                          // cs1:0xAA, cx=0xA2
            UInt8[scratchSeg, di] = UInt8[cs1, (ushort)(0x00AA + k)];
            di = (ushort)(di + 1);
        }
        for (int k = 0; k < 0x11F8; k++) {                        // entryDS:0xAA76, cx=0x11F8
            UInt8[scratchSeg, di] = UInt8[entryDs, (ushort)(0xAA76 + k)];
            di = (ushort)(di + 1);
        }
        for (int k = 0; k < 0x1261; k++) {                        // entryDS:0, cx=0x1261
            UInt8[scratchSeg, di] = UInt8[entryDs, (ushort)k];
            di = (ushort)(di + 1);
        }

        // B46E..B473 — pop es/di (ES=scratch, DI=0x100), cx=0x567A, ret.
        ES = scratchSeg;
        DI = 0x0100;
        DS = entryDs;                                             // pop ds @B45D restored entry DS
        SI = 0x1261;                                              // SI left after the last rep movsb
        AX = (ushort)((lastAh << 8) | lastAh);                    // mov al,ah (AH preserved by movsb)
        CX = 0x567A;
        return NearRet();
    }
}