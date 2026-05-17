namespace Cryogenic.Overrides;

/// <summary>
/// Phase 38 — save/load. The savegame (<c>DUNE37S&lt;slot&gt;.SAV</c>) is the
/// expanded game-state buffer (produced by <c>cs1:0xB427</c>, already C#)
/// run through a small escape-byte RLE codec. This file ports the two
/// pure-compute codec halves; the INT&#160;21 file create/write/open
/// (savegame disk I/O) is the §B boundary handled separately (see Tech/54).
/// </summary>
/// <remarks>
/// Codec format (verified against the shipped <c>DUNE37S0.SAV</c>):
/// a stream of literal bytes interspersed with <c>ESC count value</c> runs,
/// where <c>ESC = 0xF7</c>. Runs of ≥3 equal bytes (or any occurrence of the
/// escape byte itself) are encoded as the 3-byte token; runs of 1–2 are
/// stored as literal bytes. The encoder writes a 4-byte header in front of
/// the stream: word0 = <c>dx</c> (<c>dl=ESC</c>, <c>dh=</c>last run length),
/// word1 = compressed length. The decoder is purely length-driven off that
/// length word — there is no in-stream end sentinel.
/// </remarks>
public partial class Overrides {
    /// <summary>Registers the Phase 38 save/load codec overrides.</summary>
    public void DefineSaveLoadCodeOverrides() {
        DefineFunction(cs1, 0xB473, ApplySavegameMapOverlay_1000_B473_01B473);
        DefineFunction(cs1, 0xB4BB, SaveRleDecode_1000_B4BB_01B4BB);
        DefineFunction(cs1, 0xB4EA, SaveRleEncode_1000_B4EA_01B4EA);
    }

    /// <summary>
    /// Override for cs1:0xB473 — the load-side savegame map-flag overlay
    /// applier (<c>DNCDPRG_RECENT.ASM sub_1B473</c>; Tech/30 §"Save-game flag
    /// overlay"). Counterpart of the save-side expander <c>cs1:0xB427</c>:
    /// reads the decompressed <c>.SAV</c> stream at <c>DS:SI</c> and overwrites
    /// bits 4–5 of every MAP cell with 2 bits per cell from the stream, then
    /// restores the three fixed state blocks. Pure compute.
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xB473..0xB4BA), cross-verified against the cs1 dump:
    /// <code>
    /// ss: mov es,[0xDD00]            ; ES = MAP resource segment
    /// xor di,di ; mov bx,0xC5FC ; shr bx,1 ; shr bx,1   ; bx = 0x317F groups
    /// loc_1B481: lodsb ; mov cx,4 ; mov ah,al ; ror ah,1 ; ror ah,1
    /// loc_1B48B: mov al,es:[di] ; xor al,ah ; and al,0xCF ; xor al,ah
    ///            stosb ; rol ah,1 ; rol ah,1 ; loop loc_1B48B
    ///            dec bx ; jnz loc_1B481
    /// push cs ; pop es ; mov di,0x00AA ; mov cx,0x00A2 ; rep movsb   ; -> cs1:0xAA
    /// push ss ; pop es ; mov di,0xAA76 ; mov cx,0x11F8 ; rep movsb   ; -> ss:0xAA76
    /// mov di,0 ; mov cx,0x1261 ; rep movsb                           ; -> ss:0
    /// retn
    /// </code>
    /// <c>((x^ah)&amp;0xCF)^ah</c> leaves the <c>0xCF</c> bits as <c>x</c>
    /// (the <c>ah</c> cancels) and forces bits 4–5 to <c>ah</c>'s — i.e. it
    /// patches only the 2 terrain-flag bits per cell, cycling the source
    /// byte's bit-pairs across 4 cells via the <c>ror2</c>/<c>rol2</c>. The
    /// three trailing copies mirror the <c>0xB427</c> expander's three blocks
    /// (cs1:0xAA ×0xA2, ss:0xAA76 ×0x11F8, ss:0 ×0x1261). Ambient CLD.
    /// </remarks>
    public System.Action ApplySavegameMapOverlay_1000_B473_01B473(int gotoAddress) {
        ushort ds = DS;
        ushort ss = SS;
        ushort mapSeg = UInt16[ss, 0xDD00];               // ss: mov es,[0xDD00]
        ushort si = SI;
        ushort di = 0;                                     // xor di,di
        ushort bx = 0xC5FC >> 2;                           // mov bx,0xC5FC ; shr bx,1 (x2) = 0x317F
        byte al = 0, ah = 0;
        while (bx != 0) {                                  // loc_1B481 ; dec bx ; jnz
            al = UInt8[ds, si]; si = (ushort)(si + 1);     // lodsb
            ah = al;                                        // mov ah,al
            ah = (byte)(((ah & 0x03) << 6) | (ah >> 2));   // ror ah,1 ; ror ah,1
            for (int c = 0; c < 4; c++) {                  // mov cx,4 ; loop loc_1B48B
                byte x = UInt8[mapSeg, di];                 // mov al,es:[di]
                al = (byte)(((x ^ ah) & 0xCF) ^ ah);       // xor al,ah ; and al,0xCF ; xor al,ah
                UInt8[mapSeg, di] = al; di = (ushort)(di + 1);  // stosb
                ah = (byte)(((ah << 2) | (ah >> 6)) & 0xFF);    // rol ah,1 ; rol ah,1
            }
            bx = (ushort)(bx - 1);
        }
        // push cs ; pop es ; mov di,0xAA ; mov cx,0xA2 ; rep movsb
        di = 0x00AA;
        for (int k = 0; k < 0xA2; k++) {
            UInt8[cs1, di] = UInt8[ds, si]; di = (ushort)(di + 1); si = (ushort)(si + 1);
        }
        // push ss ; pop es ; mov di,0xAA76 ; mov cx,0x11F8 ; rep movsb
        di = 0xAA76;
        for (int k = 0; k < 0x11F8; k++) {
            UInt8[ss, di] = UInt8[ds, si]; di = (ushort)(di + 1); si = (ushort)(si + 1);
        }
        // mov di,0 ; mov cx,0x1261 ; rep movsb
        di = 0;
        for (int k = 0; k < 0x1261; k++) {
            UInt8[ss, di] = UInt8[ds, si]; di = (ushort)(di + 1); si = (ushort)(si + 1);
        }
        ES = ss;                                           // last: push ss ; pop es
        DI = 0x1261;                                       // di after the final rep movsb
        SI = si;
        BX = 0;
        CX = 0;
        AX = (ushort)((ah << 8) | al);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xB4BB — the savegame RLE <b>decoder</b>
    /// (<c>DNCDPRG_RECENT.ASM sub_1B4BB</c>). Reads the escape byte and the
    /// compressed length from the 2-word header at <c>DS:SI</c>, expands the
    /// stream into <c>ES:DI</c>, and returns the decompressed length in
    /// <c>CX</c> with <c>DI</c> reset to the buffer start. Pure compute.
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xB4BB..0xB4E9), cross-verified against the cs1 dump:
    /// <code>
    /// lodsw ; mov bl,al            ; BL = escape byte (=0xF7)
    /// lodsw ; mov cx,ax ; sub cx,4 ; CX = (length word) - 4   (input budget)
    /// push di
    /// loc_1B4C5: lodsb ; cmp al,bl ; jz loc_1B4D3
    ///            stosb ; loop loc_1B4C5
    /// loc_1B4CD: pop cx ; sub di,cx ; xchg cx,di ; retn
    /// loc_1B4D3: lodsb ; mov dx,cx ; mov cl,al ; xor ch,ch
    ///            lodsb ; rep stosb
    ///            mov cx,dx ; sub cx,2
    ///            jcxz loc_1B4CD ; jb loc_1B4CD ; loop loc_1B4C5 ; jmp loc_1B4CD
    /// </code>
    /// The input budget <c>CX</c> is debited 1 per literal (the <c>loop</c>)
    /// and 3 per run (the escape <c>lodsb</c> is free, then <c>sub cx,2</c>
    /// for count+value and <c>loop</c> for −1). <c>jcxz</c>/<c>jb</c> after
    /// <c>sub cx,2</c> terminate when the budget is exhausted or underflows.
    /// Ambient CLD (forward string ops).
    /// </remarks>
    public System.Action SaveRleDecode_1000_B4BB_01B4BB(int gotoAddress) {
        ushort ds = DS;
        ushort es = ES;
        ushort si = SI;
        ushort di = DI;

        byte escByte = UInt8[ds, si];                 // lodsw ; mov bl,al
        si = (ushort)(si + 2);
        ushort lenWord = UInt16[ds, si];              // lodsw
        si = (ushort)(si + 2);
        ushort cx = (ushort)(lenWord - 4);            // sub cx,4
        ushort savedDi = di;                          // push di

        bool done = false;
        while (!done) {
            byte b;
            bool atRun = false;
            while (true) {                             // loc_1B4C5
                b = UInt8[ds, si]; si = (ushort)(si + 1);  // lodsb
                if (b == escByte) {                    // cmp al,bl ; jz loc_1B4D3
                    atRun = true;
                    break;
                }
                UInt8[es, di] = b; di = (ushort)(di + 1);  // stosb (literal)
                cx = (ushort)(cx - 1);                 // loop loc_1B4C5
                if (cx == 0) {
                    done = true;
                    break;
                }
            }
            if (done || !atRun) {
                break;
            }
            // loc_1B4D3 — escape run
            byte count = UInt8[ds, si]; si = (ushort)(si + 1);  // lodsb count
            ushort dx = cx;                            // mov dx,cx
            byte val = UInt8[ds, si]; si = (ushort)(si + 1);    // lodsb value
            for (int k = 0; k < count; k++) {          // rep stosb
                UInt8[es, di] = val; di = (ushort)(di + 1);
            }
            bool borrow = dx < 2;                       // sub cx,2 (CF = dx<2)
            cx = (ushort)(dx - 2);
            if (cx == 0 || borrow) {                    // jcxz / jb loc_1B4CD
                break;
            }
            cx = (ushort)(cx - 1);                      // loop loc_1B4C5
            if (cx == 0) {                              // (cx==0 -> jmp loc_1B4CD)
                break;
            }
        }

        // loc_1B4CD: pop cx ; sub di,cx ; xchg cx,di
        ushort decompressed = (ushort)(di - savedDi);
        CX = decompressed;
        DI = savedDi;
        SI = si;
        BX = (ushort)((BX & 0xFF00) | escByte);         // bl = escByte (asm leaves it)
        DX = 0;                                         // dx = 0 at the literal-only/last path
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xB4EA — the savegame RLE <b>encoder</b>
    /// (<c>DNCDPRG.ASM sub_D3BA</c> / <c>DNCDPRG_RECENT.ASM sub_1B4EA</c>).
    /// Compresses the buffer at <c>DS:SI</c> into <c>ES:DI</c> (4-byte header
    /// reserved), terminates with a <c>00 00</c> word, then back-patches the
    /// header (<c>dx</c> = ESC:lastRun, length). Pure compute.
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xB4EA..), cross-verified against the cs1 dump:
    /// <code>
    /// mov dl,0xF7 ; push di ; add di,4
    /// loc_1B4F0: xor dh,dh
    /// loc_1B4F2: lodsb ; inc dh ; cmp al,[si] ; jnz loc_1B504
    ///            cmp dh,0xFF ; jz loc_1B504
    ///            dec cx ; or cx,cx ; jnz loc_1B4F2 ; inc cx
    /// loc_1B504: cmp al,dl ; jz loc_1B512        ; escape byte -> token
    ///            cmp dh,1 ; jz loc_1B51C          ; run 1 -> 1 literal
    ///            cmp dh,2 ; jz loc_1B52F          ; run 2 -> 2 literals
    /// loc_1B512: mov ah,al ; mov al,dl ; stosb ; mov al,dh ; stosb ; mov al,ah
    /// loc_1B51C: stosb ; loop loc_1B4F0
    ///            mov cx,di ; xor ax,ax ; stosw ; pop di ; sub cx,di
    ///            mov es:[di],dx ; mov es:[di+2],cx ; retn
    /// loc_1B52F: stosb ; jmp loc_1B51C            ; run 2 -> emit value twice
    /// </code>
    /// <c>dh</c> counts the current run (capped at 0xFF). <c>cx</c> is the
    /// remaining input count: the run-scan debits it per byte consumed and
    /// the <c>inc cx</c> restores the boundary when input is exhausted; the
    /// trailing <c>loop</c> debits one per emitted token. The escape byte is
    /// always token-encoded so the decoder cannot misread it.
    /// </remarks>
    public System.Action SaveRleEncode_1000_B4EA_01B4EA(int gotoAddress) {
        ushort ds = DS;
        ushort es = ES;
        ushort si = SI;
        ushort di = DI;
        ushort cx = CX;
        const byte esc = 0xF7;                          // mov dl,0xF7

        ushort savedDi = di;                            // push di
        di = (ushort)(di + 4);                          // add di,4 (reserve header)
        byte lastRun = 0;

        while (true) {
            byte dh = 0;                                 // loc_1B4F0: xor dh,dh
            byte al;
            while (true) {                               // loc_1B4F2
                al = UInt8[ds, si]; si = (ushort)(si + 1);    // lodsb
                dh = (byte)(dh + 1);                      // inc dh
                if (al != UInt8[ds, si]) {               // cmp al,[si] ; jnz loc_1B504
                    break;
                }
                if (dh == 0xFF) {                         // cmp dh,0xFF ; jz loc_1B504
                    break;
                }
                cx = (ushort)(cx - 1);                    // dec cx
                if (cx != 0) {                            // or cx,cx ; jnz loc_1B4F2
                    continue;
                }
                cx = (ushort)(cx + 1);                    // inc cx (input exhausted)
                break;
            }
            // loc_1B504
            lastRun = dh;
            if (al == esc || (dh != 1 && dh != 2)) {     // cmp al,dl jz ; cmp dh,1 ; cmp dh,2
                // loc_1B512: emit ESC, count, then value
                UInt8[es, di] = esc; di = (ushort)(di + 1);
                UInt8[es, di] = dh; di = (ushort)(di + 1);
                UInt8[es, di] = al; di = (ushort)(di + 1);   // loc_1B51C: stosb
            } else if (dh == 2) {
                // loc_1B52F: stosb ; jmp loc_1B51C  -> value emitted twice
                UInt8[es, di] = al; di = (ushort)(di + 1);
                UInt8[es, di] = al; di = (ushort)(di + 1);
            } else {
                // dh == 1 -> loc_1B51C: single literal
                UInt8[es, di] = al; di = (ushort)(di + 1);
            }
            cx = (ushort)(cx - 1);                        // loop loc_1B4F0
            if (cx == 0) {
                break;
            }
        }

        // mov cx,di ; xor ax,ax ; stosw ; pop di ; sub cx,di ; es:[di]=dx ; es:[di+2]=cx
        // NB: `mov cx,di` is captured *before* the stosw, so the length word
        // excludes the 2-byte terminator (it counts the 4-byte header + tokens).
        ushort compressedLen = (ushort)(di - savedDi);    // mov cx,di ; (sub cx,di)
        UInt8[es, di] = 0; di = (ushort)(di + 1);         // xor ax,ax ; stosw 0x0000
        UInt8[es, di] = 0; di = (ushort)(di + 1);
        ushort dxOut = (ushort)((lastRun << 8) | esc);    // dl=esc, dh=lastRun
        UInt16[es, savedDi] = dxOut;                      // es:[di]=dx
        UInt16[es, (ushort)(savedDi + 2)] = compressedLen; // es:[di+2]=cx
        AX = 0;                                            // xor ax,ax
        CX = compressedLen;
        DX = dxOut;
        SI = si;
        DI = savedDi;                                      // pop di
        return NearRet();
    }
}
