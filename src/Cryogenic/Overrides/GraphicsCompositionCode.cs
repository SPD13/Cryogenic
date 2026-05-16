namespace Cryogenic.Overrides;

/// <summary>
/// Partial class for Phase 37 — graphics composition layer (sprite draw dispatch,
/// SAL rasteriser, framebuffer/rect/clip). See Plans/09 Phase 37 and
/// Rebuild/DOCUMENTATION/Tech/07 (SAL), 10 (visual shapes), 13 (sprite RLE asm).
/// </summary>
/// <remarks>
/// The actual pixel decoders live in the DNPCS2 driver segment (0xD000) and are
/// reached via the indirect far vtable slots (<c>[ss:0x38C9]</c> = slot 5 RLE,
/// <c>[ss:0x38CD]</c> = slot 6 raw bitmap). These overrides port the cs1-side
/// descriptor-resolution / dispatch logic faithfully and transfer into the driver
/// via the §A FarJump-continuation pattern; the driver blit itself remains the
/// far-called routine (Phase 40 territory).
/// </remarks>
public partial class Overrides {
    /// <summary>Registers Phase 37 graphics-composition overrides with Spice86.</summary>
    public void DefineGraphicsCompositionCodeOverrides() {
        DefineFunction(cs1, 0xC22F, DrawSprite_1000_C22F_01C22F);
    }

    /// <summary>
    /// Override for cs1:0xC22F — <c>draw_sprite_ida</c>, the canonical sprite-draw
    /// entry. Resolves the sub-sprite descriptor from the sheet at <c>[ds:0xDBB0]</c>
    /// (DS reloaded via <c>lds</c>), merges caller flag bits / the
    /// <c>[cs:0xC21A]</c> palette-base override into word[0]/word[1], then dispatches:
    /// caller flags bits 10–12 clear → canonical blit via <c>ss: call far
    /// [ss:0x38C9]</c> (DNPCS2 slot 5, continuation = raw asm <c>push ss; pop ds;
    /// ret</c> @cs1:0xC268); bits set → the scaling/projection path at the still-asm
    /// <c>cs1:0xC26B</c> (div + scratch decoder cs1:0xC2A1 + <c>ss: call far
    /// [ss:0x3941]</c>), entered via <see cref="NearJump"/>.
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xC22F..0xC26A; scaling tail 0xC26B..0xC2A0 left as raw asm):
    /// <code>
    /// C22F: 8E 06 DA DB    mov es,[0xDBDA]
    /// C233: C5 36 B0 DB    lds si,[0xDBB0]            ; DS:SI = sheet base
    /// C237: 8B E8          mov bp,ax
    /// C239: 81 E5 FF 01    and bp,0x01FF
    /// C23D: D1 E5          shl bp,1
    /// C23F: 3E 03 32       ds: add si,[bp+si]
    /// C242: 8B C8          mov cx,ax
    /// C244: 50             push ax
    /// C245: AD             lodsw                      ; word[0]
    /// C246: 80 E5 60       and ch,0x60
    /// C249: 0A E5          or ah,ch
    /// C24B: 8B F8          mov di,ax
    /// C24D: AD             lodsw                      ; word[1]
    /// C24E: 8B C8          mov cx,ax
    /// C250: 2E 80 3E 1A C2 00  cmp byte [cs:0xC21A],0
    /// C256: 74 05          jz C25D
    /// C258: 2E 8A 2E 1A C2 mov ch,[cs:0xC21A]
    /// C25D: 58             pop ax
    /// C25E: 25 00 1C       and ax,0x1C00
    /// C261: 75 08          jnz C26B                   ; scaling/projection path
    /// C263: 36 FF 1E C9 38 ss: call far [ss:0x38C9]   ; canonical blit (slot 5)
    /// C268: 16 1F C3       push ss; pop ds; ret
    /// </code>
    /// </remarks>
    public System.Action DrawSprite_1000_C22F_01C22F(int gotoAddress) {
        ES = UInt16[DS, 0xDBDA];
        ushort curDs = DS;
        ushort si = UInt16[curDs, 0xDBB0];                 // lds si,[0xDBB0]
        ushort newDs = UInt16[curDs, 0xDBB2];
        DS = newDs;
        ushort spriteIndex = AX;
        ushort bp = (ushort)((AX & 0x01FF) << 1);
        BP = bp;
        si = (ushort)(si + UInt16[newDs, (ushort)(bp + si)]);   // ds: add si,[bp+si]
        SI = si;
        ushort cx = spriteIndex;                            // mov cx,ax
        SP = (ushort)(SP - 2); UInt16[SS, SP] = AX;        // push ax
        ushort w0 = UInt16[newDs, si]; si = (ushort)(si + 2);   // lodsw word[0]
        byte ch = (byte)((cx >> 8) & 0x60);                 // and ch,0x60
        w0 = (ushort)(w0 | (ch << 8));                      // or ah,ch
        ushort di = w0;                                      // mov di,ax
        DI = di;
        ushort w1 = UInt16[newDs, si]; si = (ushort)(si + 2);   // lodsw word[1]
        SI = si;
        cx = w1;                                             // mov cx,ax
        byte c21a = UInt8[cs1, 0xC21A];                      // cmp byte [cs:0xC21A],0
        if (c21a != 0) {
            cx = (ushort)((c21a << 8) | (cx & 0x00FF));      // mov ch,[cs:0xC21A]
        }
        CX = cx;
        AX = UInt16[SS, SP]; SP = (ushort)(SP + 2);         // pop ax
        ushort masked = (ushort)(AX & 0x1C00);               // and ax,0x1C00
        AX = masked;
        if (masked != 0) {                                   // jnz C26B (scaling path)
            return NearJump(0xC26B);
        }
        // C263: ss: call far [ss:0x38C9] — canonical blit (DNPCS2 slot 5)
        ushort off = UInt16[SS, 0x38C9];
        ushort seg = UInt16[SS, 0x38CB];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xC268;
        return FarJump(seg, off);
    }
}
