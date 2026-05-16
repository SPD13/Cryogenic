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
        DefineFunction(cs1, 0xC2A1, ScratchSpriteDecoder_1000_C2A1_01C2A1);
    }

    /// <summary>
    /// Override for cs1:0xC2A1 — the scaled-sprite scratch RLE decoder (Tech/13
    /// §"bit-15-set scratch decoder"). Decodes the bit7=fill / else=literal RLE
    /// stream at <c>ds:si</c> into the scratch buffer at <c>ss:0x4C60</c>,
    /// <c>height</c> rows × <c>bytesPerRow = ((word[0]&amp;0x1FF)+3 &gt;&gt; 2) &lt;&lt; 1</c>
    /// bytes (1:1 unpacked, not nibble-packed). Returns with <c>ds=ss</c>,
    /// <c>si=0x4C60</c> (pointing at the decoded scratch); AX/BX/CX/DI/ES/BP restored.
    /// Pure compute — no driver/disk/indirect deps. Resolves the
    /// <see cref="DrawSprite_1000_C22F_01C22F"/> scaling path.
    /// </summary>
    /// <remarks>
    /// Asm (81 bytes, cs1:0xC2A1..0xC2F1):
    /// <code>
    /// C2A1: 50 53 51 57 06 55   push ax,bx,cx,di,es,bp
    /// C2A7: 16 07               push ss / pop es        ; es = ss
    /// C2A9: 8B EF               mov bp,di               ; bp = word[0]
    /// C2AB: BF 60 4C            mov di,0x4C60           ; dst = ss:0x4C60
    /// C2AE: 81 E5 FF 01         and bp,0x01FF
    /// C2B2: 83 C5 03 / D1 ED / D1 ED / D1 E5            ; bp = ((w+3)>>2)<<1 = bpr
    /// C2BB: 8B 4C FE            mov cx,[si-2]           ; word[1]
    /// C2BE: 32 ED               xor ch,ch               ; cx = height
    /// C2C0: 51                  push cx                 ; ROW loop
    /// C2C1: 8B DD               mov bx,bp               ; bx = bpr (row budget)
    /// C2C3: AC                  lodsb                   ; opcode
    /// C2C4: A8 80 / 75 0E       test al,0x80 ; jnz FILL
    /// C2C8: B1 01 / 02 C8 / 32 ED   cx = al+1 (literal count)
    /// C2CE: 2B D9               sub bx,cx
    /// C2D0: F3 A4               rep movsb               ; ds:si -> es:di
    /// C2D2: 75 EF               jnz C2C3                ; bx!=0 -> next opcode
    /// C2D4: EB 0D               jmp C2E3
    /// C2D6: B1 01 / 2A C8 / 32 ED   cx = 1-al (fill count)  [FILL]
    /// C2DC: 2B D9               sub bx,cx
    /// C2DE: AC / F3 AA          lodsb (fill) ; rep stosb
    /// C2E1: 75 E0               jnz C2C3
    /// C2E3: 59 / E2 DA          pop cx ; loop C2C0       ; rowCount--
    /// C2E6: BE 60 4C            mov si,0x4C60
    /// C2E9: 16 1F               push ss / pop ds        ; ds = ss
    /// C2EB: 5D 07 5F 59 5B 58 C3   pop bp,es,di,cx,bx,ax ; ret
    /// </code>
    /// The post-<c>rep</c> <c>jnz</c> tests ZF from the preceding <c>sub bx,cx</c>
    /// (string ops/REP don't touch flags), i.e. loop while the row budget ≠ 0.
    /// Forward DF assumed (ambient cld).
    /// </remarks>
    public System.Action ScratchSpriteDecoder_1000_C2A1_01C2A1(int gotoAddress) {
        ushort savedAx = AX, savedBx = BX, savedCx = CX, savedDi = DI, savedEs = ES, savedBp = BP;
        ES = SS;                                            // push ss ; pop es
        ushort bp = (ushort)(DI & 0x01FF);                  // mov bp,di ; and bp,0x1FF
        ushort di = 0x4C60;
        bp = (ushort)(bp + 3);
        bp = (ushort)(bp >> 1);
        bp = (ushort)(bp >> 1);
        bp = (ushort)(bp << 1);                             // bytesPerRow
        ushort si = SI;
        ushort rowCount = (ushort)(UInt16[DS, (ushort)(si - 2)] & 0x00FF);   // cx=[si-2]; xor ch,ch
        while (true) {
            ushort bx = bp;                                 // mov bx,bp
            while (true) {
                byte al = UInt8[DS, si]; si = (ushort)(si + 1);   // lodsb
                ushort count;
                if ((al & 0x80) == 0) {                     // literal
                    count = (ushort)((al + 1) & 0x00FF);    // cl=1 ; add cl,al ; xor ch,ch
                    bx = (ushort)(bx - count);              // sub bx,cx
                    for (int k = 0; k < count; k++) {       // rep movsb
                        UInt8[ES, di] = UInt8[DS, si];
                        si = (ushort)(si + 1);
                        di = (ushort)(di + 1);
                    }
                } else {                                     // fill
                    count = (ushort)((1 - al) & 0x00FF);    // cl=1 ; sub cl,al ; xor ch,ch
                    bx = (ushort)(bx - count);              // sub bx,cx
                    byte fill = UInt8[DS, si]; si = (ushort)(si + 1);   // lodsb
                    for (int k = 0; k < count; k++) {       // rep stosb
                        UInt8[ES, di] = fill;
                        di = (ushort)(di + 1);
                    }
                }
                if (bx == 0) {                              // jnz C2C3 (ZF from sub bx,cx)
                    break;
                }
            }
            rowCount = (ushort)(rowCount - 1);              // pop cx ; loop C2C0
            if (rowCount == 0) {
                break;
            }
        }
        SI = 0x4C60;                                        // mov si,0x4C60
        DS = SS;                                            // push ss ; pop ds (deliberate output)
        BP = savedBp;
        ES = savedEs;
        DI = savedDi;
        CX = savedCx;
        BX = savedBx;
        AX = savedAx;
        return NearRet();
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
