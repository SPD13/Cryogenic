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
        DefineFunction(cs1, 0xF11C, AllocCxPagesToDi_1000_F11C_01F11C);
        DefineFunction(cs1, 0xF13F, AllocatorAttemptToFreeSpace_1000_F13F_01F13F);
        DefineFunction(cs1, 0xF403, HsqDecompressDsSiToEsDi_1000_F403_01F403);
        // DUNE.DAT I/O leaves: serve from the managed shim only when the host
        // archive was located+validated; otherwise leave the asm (emulated-DOS
        // INT 21) path in place so a working game is never regressed.
        if (_duneDat.IsAvailable) {
            DefineFunction(cs1, 0xF2D6, DuneDatSeek_1000_F2D6_01F2D6);
            DefineFunction(cs1, 0xF2EA, DuneDatRead_1000_F2EA_01F2EA);
        } else {
            DefineFunction(cs1, 0xF2D6, "seek_dune_dat_offset_dxax_ida");
            DefineFunction(cs1, 0xF2EA, "read_dune_dat_cx_to_esdi_ida");
        }
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

    /// <summary>
    /// Override for cs1:0xF11C — <c>alloc_cx_pages_to_di_ida</c>. Returns the current
    /// allocation block as <c>ES:DI</c> from the far pointer at <c>ds[0x39B7]</c>; if the
    /// requested end (<c>ES + CX</c> paragraphs) would exceed the heap limit at
    /// <c>ds[0xCE68]</c>, it calls the compacting reclaimer
    /// <see cref="AllocatorAttemptToFreeSpace_1000_F13F_01F13F"/> (cs1:0xF13F) and retries.
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xF11C..0xF12F):
    /// <code>
    /// F11C: C4 3E B7 39   les di,[0x39B7]      ; DI=[0x39B7], ES=[0x39B9] (bump seg)
    /// F120: 8C C0         mov ax,es
    /// F122: 03 C1         add ax,cx
    /// F124: 3B 06 68 CE   cmp ax,[0xCE68]
    /// F128: 73 01         jnc F12B             ; ax>=limit -> reclaim
    /// F12A: C3            ret                  ; enough room (ES:DI = block)
    /// F12B: E8 11 00      call F13F            ; attempt_to_free_space
    /// F12E: EB EC         jmp F11C             ; retry
    /// </code>
    /// The <c>les</c> aliases the bump segment word at <c>0x39B9</c> (<c>0x39B7+2</c>),
    /// so each retry re-reads the (possibly lowered) bump pointer that <c>cs1:0xF13F</c>
    /// just rewrote. If <c>cs1:0xF13F</c> finds nothing to reclaim it tail-transfers to
    /// the fatal error path at <c>cs1:0xF130</c> and never returns here.
    /// </remarks>
    public System.Action AllocCxPagesToDi_1000_F11C_01F11C(int gotoAddress) {
        while (true) {
            ushort di = UInt16[DS, 0x39B7];                 // les di,[0x39B7]
            ushort es = UInt16[DS, 0x39B9];                 //   ES = [0x39B7+2] = [0x39B9]
            DI = di;
            ES = es;
            ushort ax = (ushort)(es + CX);                  // mov ax,es ; add ax,cx
            AX = ax;
            if (ax < UInt16[DS, 0xCE68]) {                  // cmp ax,[0xCE68] ; jnc (CF=>ax<lim->ret)
                return NearRet();
            }
            // call F13F ; jmp F11C (retry)
            if (AllocatorFreeSpaceCore()) {
                // F13F bailed to the fatal error path at cs1:0xF130 (raw asm).
                return NearJump(0xF130);
            }
        }
    }

    /// <summary>
    /// Override for cs1:0xF13F — <c>allocator_attempt_to_free_space_ida</c>: the resource
    /// heap's compacting reclaimer. Scans the 145-entry resource descriptor tables, frees
    /// the entry with the largest gap below the heap top, slides every higher allocation
    /// down to close the gap (segment-chunked <c>rep movsw</c>), fixes up all resource
    /// segment pointers, and lowers the bump pointer at <c>[0x39B9]</c>. Pure compute +
    /// memory moves — no INT / disk / driver / indirect dependencies.
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xF13F..0xF1FA, 188 B) — see the inline step comments below. The
    /// <c>loop</c> at <c>0xF1A1</c> targets <c>0xF193</c> (not <c>0xF190</c>), so the
    /// <c>0x8000</c> sentinel in <c>BX</c> is initialised once before the find-min loop.
    /// On "nothing reclaimable" (<c>BX==0</c> after the first scan) it tail-transfers to
    /// the fatal error path at <c>cs1:0xF130</c> (raw asm, non-returning).
    /// </remarks>
    public System.Action AllocatorAttemptToFreeSpace_1000_F13F_01F13F(int gotoAddress) {
        if (AllocatorFreeSpaceCore()) {
            return NearJump(0xF130);
        }
        return NearRet();
    }

    /// <summary>
    /// Byte-faithful core of cs1:0xF13F. Returns <c>true</c> when the asm would
    /// <c>jz F130</c> (no reclaimable resource → fatal error tail-transfer); <c>false</c>
    /// on a normal <c>ret</c> (heap compacted, bump pointer lowered, or topmost block
    /// simply dropped). All effects are applied directly to emulated memory.
    /// </summary>
    private bool AllocatorFreeSpaceCore() {
        // F140..F16A — scan tables for the valid entry maximising (bp - [di]).
        ushort bp = UInt16[DS, 0x0002];                     // mov bp,[0x0002]
        ushort si = 0xD844;                                 // mov si,0xD844
        ushort di = 0xDA8C;                                 // mov di,0xDA8C
        ushort dx = 0;                                       // xor dx,dx
        ushort bx = 0;                                       // mov bx,dx
        for (int n = 0x0091; n != 0; n--) {                 // mov cx,0x91 ; loop F151
            di = (ushort)(di + 2);                           // add di,2
            si = (ushort)(si + 4);                           // add si,4
            ushort ax = UInt16[DS, (ushort)(si + 2)];        // mov ax,[si+2]
            if (ax == 0) {                                    // or ax,ax ; jz F16A
                continue;
            }
            ax = (ushort)(bp - UInt16[DS, di]);              // mov ax,bp ; sub ax,[di]
            if (ax < dx) {                                    // cmp ax,dx ; jc F16A
                continue;
            }
            dx = ax;                                          // mov dx,ax
            bx = si;                                          // mov bx,si
        }

        if (bx == 0) {                                        // or bx,bx ; jz F130
            return true;                                       // fatal error tail-transfer
        }

        // F170..F184 — invalidate the cached "current resource" index if it is the victim.
        ushort idx = (ushort)(((ushort)(bx - 0xD844)) >> 2); // sub ax,0xD844 ; shr ax,1 (x2)
        if (idx == UInt16[DS, 0x2784]) {                      // cmp ax,[0x2784] ; jnz F185
            UInt16[DS, 0x2784] = 0xFFFF;                       // mov word [0x2784],0xFFFF
        }

        // F185..F189 — take the victim's segment into DX, clear its descriptor slot.
        dx = UInt16[DS, (ushort)(bx + 2)];                    // xor dx,dx ; xchg dx,[bx+2]
        UInt16[DS, (ushort)(bx + 2)] = 0;

        // F18A..F1A5 — find the smallest segment strictly above the freed block.
        si = 0xD84A;                                          // mov si,0xD84A
        bx = 0x8000;                                           // mov bx,0x8000  (sentinel, set once)
        for (int n = 0x0091; n != 0; n--) {                   // mov cx,0x91 ; loop F193
            ushort val = UInt16[DS, si];                        // lodsw
            si = (ushort)(si + 4);                             //   si+=2 ; add si,2 (stride 4)
            ushort ax = (ushort)(val - dx);                    // sub ax,dx
            if (val < dx) {                                    // jc F1A1 (unsigned borrow)
                continue;
            }
            if (ax >= bx) {                                    // cmp ax,bx ; jnc F1A1
                continue;
            }
            bx = ax;                                           // mov bx,ax
        }

        if ((short)bx < 0) {                                   // or bx,bx ; js F1F5
            UInt16[DS, 0x39B9] = dx;                            // F1F5: mov [0x39B9],dx ; ret
            return false;
        }

        // F1A7..0xF1BF — slide every resource segment pointer >= dx down by bx.
        si = 0xD846;                                           // mov si,0xD846
        for (int n = 0x0091; n != 0; n--) {                    // mov cx,0x91 ; loop F1AD
            si = (ushort)(si + 4);                              // add si,4
            if (UInt16[DS, si] < dx) {                          // cmp [si],dx ; jc F1B6
                continue;
            }
            UInt16[DS, si] = (ushort)(UInt16[DS, si] - bx);     // sub [si],bx
        }
        if (UInt16[DS, 0xDBB2] >= dx) {                         // mov si,0xDBB2 ; cmp [si],dx ; jc F1C1
            UInt16[DS, 0xDBB2] = (ushort)(UInt16[DS, 0xDBB2] - bx); // sub [si],bx
        }

        // F1C1..0xF1F4 — slide the heap contents down, segment-chunked.
        while (true) {
            ushort esSeg = dx;                                  // mov es,dx
            dx = (ushort)(dx + bx);                             // add dx,bx
            ushort dsSeg = dx;                                  // mov ds,dx
            ushort ax = (ushort)(UInt16[SS, 0x39B9] - dx);      // ss: mov ax,[0x39B9] ; sub ax,dx
            if (ax <= 0x1000) {                                 // cmp ax,0x1000 ; jbe F1E3
                int words = ax << 3;                             // mov cx,ax ; shl cx,1 (x3)
                for (int k = 0; k < words; k++) {               // rep movsw
                    UInt16[esSeg, (ushort)(k * 2)] = UInt16[dsSeg, (ushort)(k * 2)];
                }
                // push ss ; pop ds — DS = SS (the C# DS/SS props already alias the data seg)
                UInt16[DS, 0x39B9] = (ushort)(UInt16[DS, 0x39B9] - bx); // sub [0x39B9],bx
                return false;                                   // pop cx ; ret
            }
            for (int k = 0; k < 0x8000; k++) {                  // mov cx,0x8000 ; rep movsw
                UInt16[esSeg, (ushort)(k * 2)] = UInt16[dsSeg, (ushort)(k * 2)];
            }
            dx = (ushort)(esSeg + 0x1000);                      // mov dx,es ; add dx,0x1000
            // jmp F1C1
        }
    }

    /// <summary>
    /// Override for cs1:0xF403 — <c>hsq_decomp_skip_header_dssi_to_esdi_ida</c>
    /// (<c>DNCDPRG.ASM sub_112D3</c>): the inner HSQ (LZ77 + bit-stream) decompressor.
    /// Skips the 6-byte header at <c>DS:SI</c>, decodes literals / short back-references
    /// (2 length bits + 1 offset byte, off ∈ [-256,-1]) / long back-references
    /// (13-bit offset + 3-bit length, optional extended-length byte) into the output at
    /// <c>ES:DI</c>, and returns the decompressed length in <c>CX</c> with <c>CF=1</c>.
    /// Pure compute (no INT/disk/driver/indirect) — exact static port. Gates every
    /// compressed resource load.
    /// </summary>
    /// <remarks>
    /// Ported byte-faithfully from the authoritative commented disassembly
    /// <c>DNCDPRG.ASM sub_112D3</c> (37431–37553), cross-verified against cs1 dump bytes
    /// 0xF403..0xF4A8 and the format spec in <c>Tech/02-hsq-compression.md</c>.
    /// <para>
    /// The bit queue lives in <c>BP</c>: each read is <c>shr bp,1</c> (LSB → CF); when
    /// <c>BP</c> reaches 0 a fresh LE word is loaded and <c>stc; rcr bp,1</c> reseeds it
    /// with a top sentinel bit (so the queue self-empties after 16 reads) while
    /// extracting the current bit into CF. Modelled here by <c>ReadBit()</c>.
    /// </para>
    /// <code>
    /// F403: 51 57 1E         push cx; push di; push ds
    /// F406: 83 C6 06         add si,6                 ; skip HSQ header
    /// F409: 33 ED            xor bp,bp                ; empty bit queue
    /// F40B: EB 28            jmp 0xF435 (loc_11305)   ; (0xF40D loc_112DD = sub_10FA6 tail, unreached)
    /// loc_11305 0xF435: shr bp,1 ; jz reload ; jnb loc_11316 ; (CF=1) movsb literal ; jmp loc_11305
    /// loc_11316: xor cx,cx ; bit -> jb loc_11352(long) ; else 2 len bits (rcl cx,1 x2)
    ///            ; lodsb ; mov ah,0xFF -> ax=offset(-256..-1) ; loc_1133F
    /// loc_11352: lodsw ; cl=al ; ax>>=3 ; or ah,0xE0 ; cl&=7
    ///            ; cl!=0 -> copy(len=cl) ; else lodsb ext ; ext!=0 -> copy(len=ext)
    ///            ; ext==0 -> EOS: stc; cx=di; pop ds/di; add sp,2; sub cx,di; retn
    /// loc_1133F: add ax,di ; xchg ax,si ; ds=es ; cx+=2 ; rep movsb ; ds=bx ; si=ax ; jmp loc_11305
    /// </code>
    /// <c>loc_112DD</c> (0xF40D, the bulk-<c>rep movsw</c> block) is reachable only from
    /// <c>sub_10FA6</c>, never from this entry's <c>jmp 0xF435</c>, so it is not part of
    /// this override (it stays raw asm for that other caller). Back-reference copies are
    /// byte-by-byte forward within the output segment (overlap = RLE, intentional).
    /// </remarks>
    public System.Action HsqDecompressDsSiToEsDi_1000_F403_01F403(int gotoAddress) {
        ushort inSeg = DS;                          // push ds (restored at EOS)
        ushort outSeg = ES;
        ushort entryDi = DI;                        // push di (restored at EOS)
        ushort si = (ushort)(SI + 6);               // add si,6 (skip 6-byte header)
        ushort di = DI;
        ushort bp = 0;                              // xor bp,bp

        int ReadBit() {
            ushort old = bp;
            bp = (ushort)(bp >> 1);                 // shr bp,1  (CF = old bit0)
            int cf = old & 1;
            if (bp == 0) {                          // jz -> reload
                ushort w = (ushort)(UInt8[inSeg, si] | (UInt8[inSeg, (ushort)(si + 1)] << 8));
                si = (ushort)(si + 2);              // lodsw
                bp = (ushort)(0x8000 | (w >> 1));   // mov bp,ax; stc; rcr bp,1
                cf = w & 1;                         //   CF = ax & 1
            }
            return cf;
        }

        while (true) {
            // loc_11305
            if (ReadBit() == 1) {                   // (CF=1) literal
                UInt8[outSeg, di] = UInt8[inSeg, si];   // loc_1130B: movsb
                si = (ushort)(si + 1);
                di = (ushort)(di + 1);
                continue;                            // jmp loc_11305
            }

            // loc_11316 — back-reference
            ushort cx;
            ushort ax;
            if (ReadBit() == 1) {
                // loc_11352 — long reference
                ushort w = (ushort)(UInt8[inSeg, si] | (UInt8[inSeg, (ushort)(si + 1)] << 8));
                si = (ushort)(si + 2);              // lodsw
                int cl = (w & 0xFF) & 7;            // mov cl,al ; and cl,7
                ax = (ushort)((w >> 3) | 0xE000);  // shr ax,1 x3 ; or ah,0xE0 (13-bit -off)
                if (cl != 0) {
                    cx = (ushort)cl;               // jnz loc_1133F (len = cl)
                } else {
                    ushort savedOff = ax;          // mov bx,ax
                    int ext = UInt8[inSeg, si];    // lodsb
                    si = (ushort)(si + 1);
                    ax = savedOff;                 // mov ax,bx
                    if (ext == 0) {                // EOS
                        CX = (ushort)(di - entryDi);   // mov cx,di; (pop di); sub cx,di
                        SI = si;
                        DI = entryDi;              // pop di
                        DS = inSeg;                // pop ds
                        ES = outSeg;
                        AX = ax;
                        BX = ax;
                        CarryFlag = true;          // stc
                        return NearRet();
                    }
                    cx = (ushort)ext;              // jnz loc_1133F (len = ext)
                }
            } else {
                // short reference
                cx = 0;                            // xor cx,cx
                cx = (ushort)((cx << 1) | ReadBit());  // loc_1132E: rcl cx,1
                cx = (ushort)((cx << 1) | ReadBit());  // loc_1133A: rcl cx,1
                int al = UInt8[inSeg, si];         // lodsb
                si = (ushort)(si + 1);
                ax = (ushort)(0xFF00 | al);        // mov ah,0xFF (off ∈ [-256,-1])
            }

            // loc_1133F — overlapping back-reference copy within the output segment.
            ax = (ushort)(ax + di);                // add ax,di (source = di + signed off)
            ushort cpSi = ax;                      // xchg ax,si (si = source)
            ushort restoreSi = si;                 //   ax = old input si
            cx = (ushort)(cx + 2);                 // inc cx ; inc cx
            ushort cpDi = di;
            for (int k = 0; k < cx; k++) {         // rep movsb (ds=es; overlap ok)
                UInt8[outSeg, cpDi] = UInt8[outSeg, cpSi];
                cpSi = (ushort)(cpSi + 1);
                cpDi = (ushort)(cpDi + 1);
            }
            di = cpDi;                             // di advanced by the copy
            si = restoreSi;                        // mov si,ax (input si unchanged)
            // jmp loc_11305
        }
    }

    /// <summary>
    /// Override for cs1:0xF2D6 — <c>seek_dune_dat_offset_dxax_ida</c>
    /// (<c>DNCDPRG.ASM sub_111A6</c>): DOS LSEEK (INT&#160;21 AX=4200) to the
    /// absolute DUNE.DAT offset in <c>DX:AX</c>. Served by the managed shim
    /// (<see cref="DuneDatService"/>) instead of emulated DOS.
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xF2D6..0xF2E6):
    /// <code>
    /// push cx
    /// mov bx, ss:[0xDBBA]      ; file handle (singleton — ignored by the shim)
    /// mov cx, dx              ; cx = offset hi
    /// mov dx, ax              ; dx = offset lo
    /// mov ax, 4200h
    /// int 21h                ; DX:AX = resulting absolute position
    /// pop cx                 ; CX restored
    /// retn
    /// </code>
    /// The asm clobbers BX (= handle) and returns the new position in DX:AX;
    /// CX is preserved by the push/pop. Success ⇒ CF=0.
    /// </remarks>
    public System.Action DuneDatSeek_1000_F2D6_01F2D6(int gotoAddress) {
        uint offset = ((uint)DX << 16) | AX;
        uint pos = _duneDat.Seek(offset);
        BX = UInt16[SS, 0xDBBA];                    // mov bx, ss:[0xDBBA]
        DX = (ushort)(pos >> 16);                   // INT 21 4200 -> DX:AX = new pos
        AX = (ushort)(pos & 0xFFFF);
        CarryFlag = false;                          // success
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xF2EA — <c>read_dune_dat_cx_to_esdi_ida</c>
    /// (<c>DNCDPRG.ASM sub_111BA</c>): DOS read-with-handle (INT&#160;21 AH=3F)
    /// of <c>CX</c> bytes into <c>ES:DI</c>, then <c>cmp ax,cx</c>. Served by
    /// the managed shim.
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xF2EA..0xF2FB):
    /// <code>
    /// push ds ; push es ; pop ds   ; ds = es
    /// mov bx, ss:[0xDBBA]          ; handle
    /// mov dx, di                   ; ds:dx = es:di buffer
    /// mov ah, 3Fh
    /// int 21h                      ; ax = bytes read
    /// cmp ax, cx                   ; CF=1 iff short read (ax&lt;cx); ZF iff exact
    /// pop ds                       ; ds restored
    /// retn
    /// </code>
    /// DS is net-unchanged (push ds … pop ds). The asm clobbers BX (= handle)
    /// and DX (= di). The trailing <c>cmp ax,cx</c> sets the flags the callers
    /// branch on, so the INT&#160;21 CF is irrelevant.
    /// </remarks>
    public System.Action DuneDatRead_1000_F2EA_01F2EA(int gotoAddress) {
        uint dest = MemoryUtils.ToPhysicalAddress(ES, DI);
        int n = _duneDat.ReadInto(Memory, dest, CX);
        BX = UInt16[SS, 0xDBBA];                    // mov bx, ss:[0xDBBA]
        DX = DI;                                    // mov dx, di
        AX = (ushort)n;                             // ax = bytes read
        Alu16.Sub(AX, CX);                          // cmp ax,cx (sets CF/ZF/SF/OF/AF/PF)
        return NearRet();
    }
}
