namespace Cryogenic.Overrides;

/// <summary>
/// Partial class containing audio / resource-file state helpers (Phase 32 — Tech/04, 06, 26, 51).
/// </summary>
/// <remarks>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly. Routines that touch INT 21
/// (DOS file I/O), the PIT via IN/OUT, or interrupt-flag manipulation stay as asm —
/// Spice86 emulates those at the CPU level (see Plans/09 §B).
/// </remarks>
public partial class Overrides {
    /// <summary>
    /// Registers audio / resource-file-state function overrides with Spice86.
    /// </summary>
    public void DefineAudioCodeOverrides() {
        DefineFunction(cs1, 0xA9E7, PcmTestAudioDone_1000_A9E7_01A9E7);
        DefineFunction(cs1, 0xABA3, CheckResFileOpen_1000_ABA3_01ABA3);
        DefineFunction(cs1, 0xAA70, TransferSdBlock_1000_AA70_01AA70);
        DefineFunction(cs1, 0xCFA0, CheckAmrOrEngLanguage_1000_CFA0_01CFA0);
    }

    /// <summary>
    /// Override for cs1:0xAA70 — <c>transfer_sd_block_qq_ida</c>. Block-copies a
    /// length-prefixed sound-driver buffer from <c>[ds:0xDBDE]:[ds:0xDC1C]</c> into the
    /// caller-supplied <c>es:di</c> destination, then writes a 4-byte trailer at
    /// <c>[ds:si+4..7]</c> (where <c>si</c> is the caller's original SI).
    /// </summary>
    /// <remarks>
    /// Asm (38 bytes):
    /// <code>
    /// AA70: 56            push si
    /// AA71: 1E            push ds
    /// AA72: 8B 36 1C DC   mov  si, [0xDC1C]
    /// AA76: 8E 1E DE DB   mov  ds, [0xDBDE]
    /// AA7A: AD            lodsw                ; ax = block length
    /// AA7B: 2D 04 00      sub  ax, 0x0004
    /// AA7E: 8B C8         mov  cx, ax
    /// AA80: D1 E9         shr  cx, 1           ; CF = odd-byte flag
    /// AA82: F3 A5         rep  movsw
    /// AA84: 12 C9         adc  cl, cl          ; cx==0 → cl = CF
    /// AA86: F3 A4         rep  movsb           ; copy trailing odd byte
    /// AA88: 1F            pop  ds
    /// AA89: 5E            pop  si
    /// AA8A: 89 44 04      mov  [si+0x04], ax   ; ax still = length-4
    /// AA8D: C6 44 06 01   mov  byte [si+0x06], 0x01
    /// AA91: C6 44 07 41   mov  byte [si+0x07], 0x41
    /// AA95: C3            ret
    /// </code>
    /// Pure leaf. The source segment/offset are read with the caller's DS (the
    /// <c>push ds</c> only saves it; DS is reloaded afterwards). Forward DF assumed
    /// (ambient <c>cld</c>, DOS convention). Destination is the caller's ES:DI.
    /// </remarks>
    public System.Action TransferSdBlock_1000_AA70_01AA70(int gotoAddress) {
        ushort savedSi = SI;
        ushort savedDs = DS;
        ushort si = UInt16[savedDs, 0xDC1C];
        ushort srcSeg = UInt16[savedDs, 0xDBDE];
        // lodsw from srcSeg:si
        ushort ax = UInt16[srcSeg, si];
        si = (ushort)(si + 2);
        ax = (ushort)(ax - 4);
        AX = ax;
        ushort cx = ax;
        bool oddByte = (cx & 1) != 0;
        cx = (ushort)(cx >> 1);
        // rep movsw : srcSeg:si -> ES:DI
        while (cx != 0) {
            UInt16[ES, DI] = UInt16[srcSeg, si];
            si = (ushort)(si + 2);
            DI = (ushort)(DI + 2);
            cx--;
        }
        // adc cl,cl with cx==0 → cl = CF (odd-byte flag)
        byte cl = (byte)(oddByte ? 1 : 0);
        while (cl != 0) {
            UInt8[ES, DI] = UInt8[srcSeg, si];
            si = (ushort)(si + 1);
            DI = (ushort)(DI + 1);
            cl--;
        }
        CX = 0;
        // pop ds; pop si
        DS = savedDs;
        SI = savedSi;
        UInt16[DS, (ushort)(savedSi + 4)] = ax;
        UInt8[DS, (ushort)(savedSi + 6)] = 0x01;
        UInt8[DS, (ushort)(savedSi + 7)] = 0x41;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xCFA0 — <c>check_amr_or_eng_language_ida</c>. If PCM is enabled
    /// (<see cref="CheckPcmEnabled_1000_AE2F_1AE2F"/> clears ZF) and the language byte
    /// <c>ds[0xCEEB]</c> is either 0 or 3, forces the AMR/ENG selector bytes
    /// <c>ds[0x28E7]</c> / <c>ds[0x28E8]</c> to 2.
    /// </summary>
    /// <remarks>
    /// Asm (25 bytes):
    /// <code>
    /// CFA0: E8 8C DE      call AE2F            ; CheckPcmEnabled → ZF = !(DBC8 &amp; 1)
    /// CFA3: 74 13         jz   CFB8
    /// CFA5: A0 EB CE      mov  al, [0xCEEB]
    /// CFA8: 0A C0         or   al, al
    /// CFAA: 74 04         jz   CFB0
    /// CFAC: 3C 03         cmp  al, 0x03
    /// CFAE: 75 08         jnz  CFB8
    /// CFB0: B0 02         mov  al, 0x02
    /// CFB2: A2 E7 28      mov  [0x28E7], al
    /// CFB5: A2 E8 28      mov  [0x28E8], al
    /// CFB8: C3            ret
    /// </code>
    /// One call, to the already-ported <see cref="CheckPcmEnabled_1000_AE2F_1AE2F"/>
    /// (sets ZF via <c>and (DBC8),1</c>; returns no register). Invoked directly for its
    /// flag side effect; its NearRet result is discarded (the asm <c>call</c> just
    /// returns and execution falls through here).
    /// </remarks>
    public System.Action CheckAmrOrEngLanguage_1000_CFA0_01CFA0(int gotoAddress) {
        CheckPcmEnabled_1000_AE2F_1AE2F(0);
        if (ZeroFlag) {
            return NearRet();
        }
        byte al = UInt8[DS, 0xCEEB];
        if (al != 0 && al != 0x03) {
            return NearRet();
        }
        UInt8[DS, 0x28E7] = 0x02;
        UInt8[DS, 0x28E8] = 0x02;
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xA9E7 — <c>pcm_test_audio_done_ida</c>. Returns ZF=1 when
    /// either PCM channel state byte (<c>ds[0x3817]</c> or <c>ds[0x381F]</c>) equals 3
    /// ("playback finished").
    /// </summary>
    /// <remarks>
    /// Asm (13 bytes):
    /// <code>
    /// A9E7: 80 3E 17 38 03    cmp byte [0x3817], 0x03
    /// A9EC: 74 05              jz  A9F3
    /// A9EE: 80 3E 1F 38 03    cmp byte [0x381F], 0x03
    /// A9F3: C3                 ret
    /// </code>
    /// Pure leaf. If <c>[0x3817] == 3</c> the first cmp sets ZF=1 and the jz short-circuits;
    /// otherwise ZF reflects <c>[0x381F] == 3</c>.
    /// </remarks>
    public System.Action PcmTestAudioDone_1000_A9E7_01A9E7(int gotoAddress) {
        byte v3817 = UInt8[DS, 0x3817];
        if (v3817 == 0x03) {
            ZeroFlag = true;
            return NearRet();
        }
        byte v381F = UInt8[DS, 0x381F];
        ZeroFlag = (v381F == 0x03);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xABA3 — <c>check_res_file_open_ida</c>. Returns ZF=1 when no
    /// resource file handle is open (<c>ds[0x3821] == 0</c>).
    /// </summary>
    /// <remarks>
    /// Asm (6 bytes):
    /// <code>
    /// ABA3: 83 3E 21 38 00    cmp word [0x3821], 0
    /// ABA8: C3                 ret
    /// </code>
    /// Pure leaf.
    /// </remarks>
    public System.Action CheckResFileOpen_1000_ABA3_01ABA3(int gotoAddress) {
        ushort handle = UInt16[DS, 0x3821];
        ZeroFlag = (handle == 0);
        return NearRet();
    }
}
