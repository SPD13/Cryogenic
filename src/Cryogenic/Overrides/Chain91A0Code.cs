namespace Cryogenic.Overrides;

using System;
using System.IO;

using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Structures;

/// <summary>
/// Partial class for the <c>cs1:0x91A0</c> (<c>sub_B070</c>) deep-chain
/// campaign — a bottom-up, byte-faithful port of the 9-node mutually-recursive
/// orchestrator subtree reached from the outer dialogue VM. Port order is the
/// topological order of the dependency tree (see <c>tools/chain_tree.py</c>):
/// CA96, C871, F08B, F35A, F4ED, C675, BA5B, B782, B070.
/// </summary>
/// <remarks>
/// Every method is a faithful reproduction of its asm, cross-verified against
/// the authoritative <c>DNCDPRG.ASM</c> (offset rule cs1 = ASM − 0x1ED0) and
/// byte-verified against <c>research/asm-nav/cs1.bin</c>. No stubs, no guesses.
/// Method names embed segment/offset/linear for disassembly traceability.
/// </remarks>
public partial class Overrides {
    /// <summary>Registers the 0x91A0 deep-chain campaign overrides with Spice86.</summary>
    public void DefineChain91A0CodeOverrides() {
        DefineFunction(cs1, 0xABC6, ClearDc2B_1000_ABC6_01ABC6);
        DefineFunction(cs1, 0xA9A1, CloseFileHandle3821_1000_A9A1_01A9A1);
        DefineFunction(cs1, 0xD1BB, GlyphStreamProcessor_1000_D1BB_01D1BB);
        DefineFunction(cs1, 0xD48A, RenderTextLineSetup_1000_D48A_01D48A);
        DefineFunction(cs1, 0xD61D, RefreshOnMenuTypeChange_1000_D61D_01D61D);
        DefineFunction(cs1, 0xA7A5, ResetAndRefresh_1000_A7A5_01A7A5);
        DefineFunction(cs1, 0x9B8B, TeardownDialogueState_1000_9B8B_019B8B);
        DefineFunction(cs1, 0x9B8E, TeardownDialogueStateCont_1000_9B8E_019B8E);
        DefineFunction(cs1, 0x98B2, ResetRenderStateThenChain_1000_98B2_0198B2);
        DefineFunction(cs1, 0x91A0, ComputeDialogueModeThenSetup_1000_91A0_0191A0);
    }

    /// <summary>
    /// Registers the <c>cs1:0x978E</c> (<c>sub_B65E</c>) deep-chain campaign
    /// overrides — a second bounded bottom-up subtree (5 nodes:
    /// BA7C, E3AD, B81F, B7D8, B65E) sharing the same exact-port discipline
    /// and <c>tools/chain_tree.py</c> workflow as the 0x91A0 campaign.
    /// </summary>
    public void DefineChain978ECodeOverrides() {
        DefineFunction(cs1, 0x9BAC, SaveSiCallGuard_1000_9BAC_019BAC);
        DefineFunction(cs1, 0xC4DD, CursorGuardThenRectRegs_1000_C4DD_01C4DD);
        DefineFunction(cs1, 0x994F, ComputeBpFromMenuState_1000_994F_01994F);
        DefineFunction(cs1, 0x9908, BuildSceneRecordPtr_1000_9908_019908);
        DefineFunction(cs1, 0x978E, DialogueSceneRebuild_1000_978E_01978E);
    }

    /// <summary>
    /// Registers the <c>cs1:0x9EFD</c> (<c>sub_BDCD</c>) deep-chain campaign
    /// overrides (10-node bounded subtree; <c>tools/chain_tree.py</c>).
    /// </summary>
    public void DefineChain9EFDCodeOverrides() {
        DefineFunction(cs1, 0xD617, SaveListReloadAx90_1000_D617_01D617);
        DefineFunction(cs1, 0x9F1C, ResetSpeakerState_1000_9F1C_019F1C);
        DefineFunction(cs1, 0x9F1F, ResetSpeakerStateCont_1000_9F1F_019F1F);
        DefineFunction(cs1, 0xA90B, SaveDirInit_1000_A90B_01A90B);
        DefineFunction(cs1, 0xA8BC, FormatSaveSlotName_1000_A8BC_01A8BC);
        DefineFunction(cs1, 0xA93F, ReadSaveChunk_1000_A93F_01A93F);
        DefineFunction(cs1, 0xAC14, SaveAllCallFar3995_1000_AC14_01AC14);
        DefineFunction(cs1, 0xA9B9, ReadResChunkThenFar39A1_1000_A9B9_01A9B9);
        DefineFunction(cs1, 0xA83F, LoadResChunkSeq_1000_A83F_01A83F);
        DefineFunction(cs1, 0xA6CC, ResolveResEntry_1000_A6CC_01A6CC);
        DefineFunction(cs1, 0x9EFD, LoadResourceRoot_1000_9EFD_019EFD);
    }

    /// <summary>
    /// Registers the <c>cs1:0x8B11</c> (<c>sub_A9E1</c>) deep-chain campaign
    /// overrides (14-node bounded subtree — the largest Phase-27-cluster
    /// member, L210 root with 7 §A indirects; <c>tools/chain_tree.py</c>).
    /// </summary>
    public void DefineChain8B11CodeOverrides() {
        DefineFunction(cs1, 0x9046, GridScanPatchSetup_1000_9046_019046);
        DefineFunction(cs1, 0xC0E8, FarCall392DBpCE7A_1000_C0E8_01C0E8);
    }

    /// <summary>
    /// cs1:0x9046 — <c>sub_AF16</c>. Sets up a <c>repne scasb</c> grid scan
    /// (<c>es=ds; cx = [0x4793]*[0x2240]; di=[0x22FC]; al=0x0F;
    /// ah = ([0xEA]&gt;0 signed) ? 8 : 0xF0; bx=0</c>) then enters the
    /// scan/patch loop at <c>loc_AF33</c> (cs1:0x9063). The setup is ported
    /// in C#; the loop is delegated to the emulated stream via
    /// <see cref="NearJump"/>(0x9063) — deliberately: the loop's
    /// <c>repne scasb</c> + <c>jnz</c> depends on x86 ZF-persistence across a
    /// CX=0 re-entry that is only faithful when executed by the emulator.
    /// </summary>
    /// <remarks>
    /// Asm head byte-verified vs cs1.bin@0x9046
    /// (<c>1E 07 A1 93 47 F7 26 40 22 8B C8 8B 3E FC 22 B8 0F F0 33 DB
    /// 80 3E EA 00 00 7E 02 B4 08</c>): <c>jle loc_AF33</c> → 0x9063;
    /// <c>mov ah,8</c> @0x9061; loc_AF33 @0x9063.
    /// </remarks>
    public Action GridScanPatchSetup_1000_9046_019046(int gotoAddress) {
        ES = DS;                                        // push ds ; pop es
        ushort a = UInt16[DS, 0x4793];                  // mov ax,ds:4793h
        uint prod = (uint)a * UInt16[DS, 0x2240];       // mul word ptr ds:2240h
        AX = (ushort)prod;
        DX = (ushort)(prod >> 16);                       //   (mul sets dx:ax)
        CX = (ushort)prod;                               // mov cx,ax
        DI = UInt16[DS, 0x22FC];                          // mov di,ds:22FCh
        ushort ax = 0xF00F;                               // mov ax,0F00Fh
        BX = 0;                                            // xor bx,bx
        if ((sbyte)UInt8[DS, 0x00EA] > 0) {              // cmp byte ds:0EAh,0 ; jle loc_AF33
            ax = (ushort)((ax & 0x00FF) | 0x0800);       // mov ah,8
        }
        AX = ax;
        return NearJump(0x9063);                          // loc_AF33 (repne-scasb loop, emulated)
    }

    /// <summary>
    /// cs1:0xC0E8 — <c>sub_DFB8</c>. Clean §A: <c>es=[0xDBD8];
    /// bp=0xCE7A; call dword ptr ds:[0x392D]; retn</c>. Pushes CS(cs1) +
    /// the raw continuation IP 0xC0F3 (the bare <c>retn</c>) then
    /// <see cref="FarJump"/> to <c>[0x392D]</c>.
    /// </summary>
    /// <remarks>
    /// Asm 12 B byte-verified vs cs1.bin@0xC0E8
    /// (<c>8E 06 D8 DB BD 7A CE FF 1E 2D 39 C3</c>): far call @0xC0EF,
    /// continuation @0xC0F3 = <c>C3</c>.
    /// </remarks>
    public Action FarCall392DBpCE7A_1000_C0E8_01C0E8(int gotoAddress) {
        ES = UInt16[DS, 0xDBD8];                          // mov es,ds:0DBD8h
        BP = 0xCE7A;                                       // mov bp,0CE7Ah
        ushort off = UInt16[DS, 0x392D];                  // call dword ptr ds:392Dh
        ushort seg = UInt16[DS, 0x392F];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xC0F3;   // continuation (raw retn)
        return FarJump(seg, off);
    }

    /// <summary>
    /// cs1:0x9EFD — <c>sub_BDCD</c>, the 0x9EFD-campaign root. Snapshots
    /// <c>al=[0x47DC] → [0x47DD]</c>, loads <c>ax=[0x4780]</c>,
    /// <c>bx=[0x47C4]</c>, then <c>call sub_C59C</c>; bails on CF=0
    /// (<c>jnb locret_BDCC</c> 0x9EFC = <c>retn</c>); else if
    /// <c>[0x47C4] &lt; 0x10</c> runs <c>sub_BDEC</c>, and finally
    /// <c>jmp loc_C62C</c> into its own non-contiguous FUNCTION CHUNK at
    /// cs1:0xA75C. The 4-instruction compute head is ported in C#; the
    /// call-chain + chunk tail is delegated to the emulated stream via
    /// <see cref="NearJump"/>(0x9F0A) — <c>sub_C59C</c> (0xA6CC, sets the CF
    /// that <c>jnb</c> tests) and <c>sub_BDEC</c> (0x9F1C) are
    /// NearJump-delegating ports, and <c>loc_C62C</c>/0xA75C is a raw-asm
    /// code chunk: all are faithful only through the real emulated path.
    /// </summary>
    /// <remarks>
    /// Asm byte-verified vs cs1.bin@0x9EFD
    /// (<c>A0 DC 47 A2 DD 47 A1 80 47 8B 1E C4 47 E8 BF 07 73 ED
    /// 83 3E C4 47 10 73 03 E8 03 00 E9 40 08</c>):
    /// <code>
    /// 9EFD: A0 DC 47    mov al,ds:47DCh
    /// 9F00: A2 DD 47    mov ds:47DDh,al
    /// 9F03: A1 80 47    mov ax,ds:4780h
    /// 9F06: 8B 1E C4 47 mov bx,ds:47C4h
    /// 9F0A: E8 BF 07    call sub_C59C (0xA6CC)
    /// 9F0D: 73 ED       jnb locret_BDCC (0x9EFC=retn)
    /// 9F0F: 83 3E C4 47 10 cmp word ds:47C4h,10h
    /// 9F14: 73 03       jnb loc_BDE9 (0x9F19)
    /// 9F16: E8 03 00    call sub_BDEC (0x9F1C)
    /// 9F19: E9 40 08    jmp loc_C62C (chunk @0xA75C)
    /// </code>
    /// </remarks>
    public Action LoadResourceRoot_1000_9EFD_019EFD(int gotoAddress) {
        AL = UInt8[DS, 0x47DC];            // mov al,ds:47DCh
        UInt8[DS, 0x47DD] = AL;            // mov ds:47DDh,al
        AX = UInt16[DS, 0x4780];           // mov ax,ds:4780h
        BX = UInt16[DS, 0x47C4];           // mov bx,ds:47C4h
        return NearJump(0x9F0A);           // call sub_C59C onward (emulated; incl. chunk@0xA75C)
    }

    /// <summary>
    /// cs1:0xA83F — <c>sub_C70F</c>. Clears <c>[0xDC26]</c>, then if PCM is
    /// enabled (<see cref="CheckPcmEnabled_1000_AE2F_1AE2F"/> sets ZF) loads
    /// the next resource chunk (<c>sub_CAE4; sub_C7DB; cmc/jnb</c>; the
    /// <c>les di,[0x3811]</c> type-5 sub-chunk parse; <c>sub_C889; stc</c>).
    /// The <c>[0xDC26]=0; call sub_CCFF; jz locret_C74D</c> head is ported in
    /// C# (sub_CCFF/0xAE2F is NearRet-inline and sets ZF); the call-heavy
    /// body is delegated to the emulated stream via <see cref="NearJump"/>(0xA84A)
    /// because <c>sub_CAE4</c> (0xAC14 §A FarJump), <c>sub_C7DB</c> (0xA90B)
    /// and <c>sub_C889</c> (0xA9B9) are all NearJump/FarJump-returning ports
    /// (faithful only through real emulated calls).
    /// </summary>
    /// <remarks>
    /// Asm (L27) byte-verified vs cs1.bin@0xA83F
    /// (<c>C7 06 26 DC 00 00 E8 E7 05 74 33 E8 C7 03 E8 BB 00 F5 73 2A
    /// C4 3E 11 38 83 C7 1A 26 80 3D 05 75 11 26 8B 4D 01 83 C7 04 8B C7
    /// 05 02 00 A3 26 DC 03 F9 89 3E 11 38 29 3E 15 38 E8 3D 01 F9 C3</c>):
    /// locret_C74D 0xA87D = <c>C3</c>; delegate point 0xA84A.
    /// </remarks>
    public Action LoadResChunkSeq_1000_A83F_01A83F(int gotoAddress) {
        UInt16[DS, 0xDC26] = 0;                         // mov word ds:0DC26h,0
        CheckPcmEnabled_1000_AE2F_1AE2F(0);             // call sub_CCFF (sets ZF, NearRet-safe)
        if (ZeroFlag) {                                 // jz locret_C74D
            return NearRet();
        }
        return NearJump(0xA84A);                          // call sub_CAE4 onward (emulated)
    }

    /// <summary>
    /// cs1:0xA6CC — <c>sub_C59C</c>. Resolves a resource entry from
    /// <c>bx</c>. Contains <b>self-modifying code</b> at <c>loc_C5A1</c>
    /// (<c>xor byte ptr cs:loc_C5A1+2,10h</c> toggles the
    /// <c>mov ax,0FFFh</c> immediate between 0x0FFF/0x1FFF each call) plus
    /// many calls — so only the entry decision <c>cmp bx,0FFFFh; jnz</c> is
    /// ported in C#; both bodies are delegated to the emulated stream via
    /// <see cref="NearJump"/> (loc_C5A1 cs1:0xA6D1 / loc_C5B6 cs1:0xA6E6).
    /// This is deliberate and exact: the self-modification mutates emulated
    /// code memory and is only faithful when executed there — reimplementing
    /// it in C# would not reproduce the toggled-immediate behaviour.
    /// </summary>
    /// <remarks>
    /// Asm head byte-verified vs cs1.bin@0xA6CC
    /// (<c>83 FB FF 75 15 B8 FF 0F 2E 80 36 D3 A6 10 ...</c>):
    /// <c>cmp bx,0FFFFh</c>; <c>jnz +0x15</c> → loc_C5B6 0xA6E6; fall →
    /// loc_C5A1 0xA6D1 (<c>mov ax,0FFFh</c>; <c>xor byte cs:[0xA6D3],10h</c>).
    /// </remarks>
    public Action ResolveResEntry_1000_A6CC_01A6CC(int gotoAddress) {
        if (BX != 0xFFFF) {                             // cmp bx,0FFFFh ; jnz loc_C5B6
            return NearJump(0xA6E6);                     // loc_C5B6 (emulated)
        }
        return NearJump(0xA6D1);                          // loc_C5A1 (self-modifying, emulated)
    }

    /// <summary>
    /// cs1:0xAC14 — <c>sub_CAE4</c>. Saves 7 registers, runs
    /// <c>sub_F92F(si=0xAB92)</c> + <c>sub_C871</c> (both NearRet-inline,
    /// call-and-discard-safe), then a far-indirect <c>call dword ptr
    /// ds:[0x3995]</c> whose raw-asm continuation (pop es/bp/di/si/cx/bx/ax;
    /// retn) restores them. Clean §A FarJump-continuation: push the 7 regs +
    /// CS(cs1) + the continuation IP 0xAC28, then <see cref="FarJump"/> to
    /// <c>[0x3995]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (L20) byte-verified vs cs1.bin@0xAC14
    /// (<c>50 53 51 56 57 55 06 BE 92 AB E8 3E 2E E8 7D FD FF 1E 95 39
    /// 07 5D 5F 5E 59 5B 58 C3</c>): far call @0xAC24, continuation @0xAC28.
    /// </remarks>
    public Action SaveAllCallFar3995_1000_AC14_01AC14(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = AX;   // push ax
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BX;   // push bx
        SP = (ushort)(SP - 2); UInt16[SS, SP] = CX;   // push cx
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;   // push si
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DI;   // push di
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BP;   // push bp
        SP = (ushort)(SP - 2); UInt16[SS, SP] = ES;   // push es
        SI = 0xAB92;                                   // mov si,0AB92h
        ListRemoveRecordBySi_1000_DA5F_1DA5F(0);        // call sub_F92F (NearRet-safe)
        CloseFileHandle3821_1000_A9A1_01A9A1(0);        // call sub_C871 (NearRet-safe)
        ushort off = UInt16[DS, 0x3995];                // call dword ptr ds:3995h
        ushort seg = UInt16[DS, 0x3997];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xAC28; // continuation (raw pop*7; retn)
        return FarJump(seg, off);
    }

    /// <summary>
    /// cs1:0xA9B9 — <c>sub_C889</c>. Returns early
    /// (<c>locret_C8B6</c> 0xA9E6 = <c>retn</c>) if the resource file is not
    /// open (<see cref="CheckResFileOpen_1000_ABA3_01ABA3"/> sets ZF) or via
    /// the <c>[0x3817]/[0x381F]</c> guard; else selects the descriptor
    /// (<c>si=0x3811</c> or <c>0x3819</c>), loads <c>bx=[0x3821]</c>,
    /// <c>es:dx=[si]</c>, <c>dx+=6</c>. The guard logic is ported in C#;
    /// <c>push si; call sub_C80F; pop si; jb locret_C8B6;
    /// call dword ptr ds:[0x39A1]</c> is delegated to the emulated stream via
    /// <see cref="NearJump"/>(0xA9DB) because <c>sub_C80F</c> (0xA93F, my
    /// port) has a <c>NearJump(0xA9A1)</c> path and <c>[0x39A1]</c> is a §A
    /// far-indirect — both faithful only through real emulated calls.
    /// </summary>
    /// <remarks>
    /// Asm (L23) byte-verified vs cs1.bin@0xA9B9
    /// (<c>E8 E7 01 74 28 BE 11 38 80 3E 17 38 00 74 0A 80 3E 1F 38 00
    /// 75 17 BE 19 38 8B 1E 21 38 C4 14 83 C2 06 56 E8 60 FF 5E 72 04
    /// FF 1E A1 39 C3</c>): loc_C8A2=0xA9D2, delegate@0xA9DB, locret=0xA9E6.
    /// </remarks>
    public Action ReadResChunkThenFar39A1_1000_A9B9_01A9B9(int gotoAddress) {
        CheckResFileOpen_1000_ABA3_01ABA3(0);          // call sub_CA73 (sets ZF, NearRet-safe)
        if (ZeroFlag) {                                 // jz locret_C8B6
            return NearRet();
        }
        SI = 0x3811;                                    // mov si,3811h
        if (UInt8[DS, 0x3817] != 0) {                   // cmp byte [0x3817],0 ; jz loc_C8A2
            if (UInt8[DS, 0x381F] != 0) {               // cmp byte [0x381F],0 ; jnz locret_C8B6
                return NearRet();
            }
            SI = 0x3819;                                // mov si,3819h
        }
        // loc_C8A2:
        BX = UInt16[DS, 0x3821];                         // mov bx,[0x3821]
        ES = UInt16[DS, (ushort)(SI + 2)];              // les dx,[si]
        DX = (ushort)(UInt16[DS, SI] + 6);              //   dx=[si] ; add dx,6
        return NearJump(0xA9DB);                          // push si; call sub_C80F; ... (emulated)
    }

    /// <summary>
    /// cs1:0xA93F — <c>sub_C80F</c>. Seeks the savegame file to the 32-bit
    /// position <c>[0xDBC2]:[0xDBC0]</c> (<c>int 21h</c> AX=0x4200 LSEEK
    /// SEEK_SET) and reads up to 0x2000 bytes — or the remaining
    /// <c>[0xDBC6]:[0xDBC4]</c> count — into <c>es:dx</c> (<c>int 21h</c>
    /// AH=0x3F READ), recording the byte count at <c>[si+4]</c>, advancing
    /// the file position, bumping the slot counter <c>[0x3823]</c> and
    /// writing the per-chunk descriptor at <c>[si+6]/[si+7]</c>. Both
    /// <c>int 21h</c> calls are modelled with the same managed
    /// <see cref="DosFileManager"/> path HnmCode uses
    /// (<c>MoveFilePointerUsingHandle</c> / <c>ReadFileOrDevice</c>);
    /// the asm's <c>jb locret_C888</c> read-error branch is unreachable in
    /// the managed model (errors throw), so the success path is taken.
    /// On the final-negative-count path it sets bit 7 of <c>[si+7]</c> and
    /// falls through into <c>sub_C871</c> (cs1:0xA9A1, already C#) modelled
    /// by <see cref="NearJump"/>; otherwise returns (locret_C888 0xA9B8 =
    /// <c>retn</c>, CF=0 from the preceding <c>clc</c>).
    /// </summary>
    /// <remarks>
    /// Asm (L45) byte-verified vs cs1.bin@0xA93F
    /// (<c>52 8B 16 C0 DB 8B 0E C2 DB B8 00 42 CD 21 5A 56 1E B9 00 20
    /// A1 C4 DB 29 0E C4 DB 83 1E C6 DB 00 73 03 8B C8 41 06 1F B4 3F CD 21
    /// 1F 5E 89 44 04 72 47 ... F8 79 1B 80 4C 07 80</c>). BX = caller's
    /// file handle. The 32-bit <c>sub [0xDBC4],0x2000 / sbb [0xDBC6],0 /
    /// jnb</c> selects full-block (0x2000) vs final partial
    /// (<c>cx = old[0xDBC4]+1</c>) read.
    /// </remarks>
    public Action ReadSaveChunk_1000_A93F_01A93F(int gotoAddress) {
        DosFileManager fm = Machine.Dos.FileManager;
        ushort handle = BX;                               // BX = file handle (caller)
        // push dx ; dx=[0xDBC0]; cx=[0xDBC2]; ax=4200h; int 21h (LSEEK SEEK_SET) ; pop dx
        uint seekOff = (uint)((UInt16[DS, 0xDBC2] << 16) | UInt16[DS, 0xDBC0]);
        fm.MoveFilePointerUsingHandle(SeekOrigin.Begin, handle, (int)seekOff);
        // push si ; push ds ; cx=2000h ; ax=[0xDBC4]
        ushort cx = 0x2000;
        ushort ax0 = UInt16[DS, 0xDBC4];
        // sub [0xDBC4],cx ; sbb word [0xDBC6],0 ; jnb loc_C834 ; (else) cx=ax ; inc cx
        ushort borrow1 = (ushort)(ax0 < cx ? 1 : 0);
        UInt16[DS, 0xDBC4] = (ushort)(ax0 - cx);
        ushort hi0 = UInt16[DS, 0xDBC6];
        UInt16[DS, 0xDBC6] = (ushort)(hi0 - borrow1);
        bool finalBorrow = hi0 < borrow1;                 // CF out of `sbb [0xDBC6],0`
        if (finalBorrow) {                                // NOT jnb -> partial last read
            cx = (ushort)(ax0 + 1);
        }
        CX = cx;
        // loc_C834: push es ; pop ds (ds=es) ; ah=3Fh ; int 21h (READ) ; pop ds ; pop si
        uint target = (uint)((ES << 4) + DX);             // DS:DX with DS=ES, DX=caller's dx
        DosFileOperationResult r = fm.ReadFileOrDevice(handle, cx, target);
        ushort bytesRead = (ushort)(r.Value ?? 0);
        AX = bytesRead;
        UInt16[DS, (ushort)(SI + 4)] = bytesRead;          // mov [si+4],ax  (jb unreachable)
        // add [0xDBC0],ax ; adc word [0xDBC2],0
        uint pos = ((uint)(UInt16[DS, 0xDBC2] << 16) | UInt16[DS, 0xDBC0]) + bytesRead;
        UInt16[DS, 0xDBC0] = (ushort)(pos & 0xFFFF);
        UInt16[DS, 0xDBC2] = (ushort)(pos >> 16);
        UInt8[DS, 0x376A] = 0xFF;                          // mov byte [0x376A],0FFh
        UInt8[DS, (ushort)(SI + 6)] = 1;                   // mov byte [si+6],1
        byte bl = UInt8[DS, 0x3823];                       // mov bl,[0x3823]
        if (bl < 0x3F) {                                   // cmp bl,3Fh ; jnb loc_C862
            UInt8[DS, 0x3823] = (byte)(UInt8[DS, 0x3823] + 1);  // inc byte [0x3823]
            bl = (byte)(bl + 1);                           // inc bl
        }
        BL = bl;
        UInt8[DS, (ushort)(SI + 7)] = bl;                  // loc_C862: mov [si+7],bl
        short dbc6 = (short)UInt16[DS, 0xDBC6];             // cmp word [0xDBC6],0
        CarryFlag = false;                                 // clc
        if (dbc6 >= 0) {                                   // jns locret_C888 (SF=0)
            return NearRet();                               // 0xA9B8 = retn
        }
        UInt8[DS, (ushort)(SI + 7)] =
            (byte)(UInt8[DS, (ushort)(SI + 7)] | 0x80);    // or byte [si+7],80h
        return NearJump(0xA9A1);                            // fall into sub_C871 (C#)
    }

    /// <summary>
    /// cs1:0xD617 — <c>sub_F4E7</c>: <c>push ax; mov ax,0x90;
    /// jmp loc_F4F1</c> — the AX=0x90 alternate entry of the shared
    /// <c>sub_F4ED</c> body (loc_F4F1 cs1:0xD621; cf. the AX=0x9F entry
    /// <see cref="RefreshOnMenuTypeChange_1000_D61D_01D61D"/>). Models
    /// <c>push ax</c> on the emulated stack then
    /// <see cref="NearJump"/>(0xD621) — the loc_F4F1 raw-asm body runs
    /// exactly as it does for 0xD61D.
    /// </summary>
    /// <remarks>Asm 6 B, byte-verified cs1.bin@0xD617:
    /// <c>50 B8 90 00 EB 04</c> (jmp +4 → 0xD61D+? ; 0xD61D+0x0A=0xD621).</remarks>
    public Action SaveListReloadAx90_1000_D617_01D617(int gotoAddress) {
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = AX;          // push ax
        AX = 0x0090;                  // mov ax,90h
        return NearJump(0xD621);      // jmp loc_F4F1 (shared sub_F4ED body, emulated)
    }

    /// <summary>
    /// cs1:0x9F1C — <c>sub_BDEC</c> entry. Models <c>call sub_B067</c>
    /// (cs1:0x9197 GuardSceneNotTerminator — has a NearJump(0x91A0) path so
    /// call-and-discard is unsafe) via the near-call-continuation idiom: push
    /// the post-call IP 0x9F1F, <see cref="NearJump"/> to sub_B067.
    /// </summary>
    /// <remarks>Asm: <c>9F1C: E8 78 F2 call sub_B067 (0x9197)</c>
    /// (byte-verified cs1.bin@0x9F1C; <c>0x9F1F-0xD88=0x9197</c>).</remarks>
    public Action ResetSpeakerState_1000_9F1C_019F1C(int gotoAddress) {
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x9F1F;      // push continuation IP
        return NearJump(0x9197);      // call sub_B067
    }

    /// <summary>
    /// cs1:0x9F1F — continuation of <see cref="ResetSpeakerState_1000_9F1C_019F1C"/>.
    /// <c>or byte ds:47D1h,10h</c> ported in C#, then delegates the rest
    /// (<c>call sub_B94B; xor ah,ah; call sub_B930; mov ds:47C6h,si; retn</c>)
    /// to the emulated stream via <see cref="NearJump"/>(0x9F24) — sub_B94B
    /// (0x9A7B) has a NearJump(0xE3B7) path so call-and-discard is unsafe;
    /// it dispatches correctly when reached through the real emulated call.
    /// </summary>
    /// <remarks>Asm byte-verified cs1.bin@0x9F1F:
    /// <c>80 0E D1 47 10</c> (or byte [0x47D1],10h) then 0x9F24
    /// (<c>E8 54 FB call sub_B94B</c>).</remarks>
    public Action ResetSpeakerStateCont_1000_9F1F_019F1F(int gotoAddress) {
        UInt8[DS, 0x47D1] = (byte)(UInt8[DS, 0x47D1] | 0x10);   // or byte ds:47D1h,10h
        return NearJump(0x9F24);      // call sub_B94B onward (emulated)
    }

    /// <summary>
    /// cs1:0xA90B — <c>sub_C7DB</c>: zeroes the save-directory scratch
    /// (<c>dx=0x37DA; [0x3811]/[0x3817]/[0x381F]=0; [0x3823]=0</c>) then
    /// <c>call subLoadSavegame; jb locret_C7DA; ...; les dx,[0x3811];
    /// fall into sub_C80F</c>. The zeroing head is ported in C#; the rest is
    /// delegated to the emulated stream via <see cref="NearJump"/>(0xA91C)
    /// because <c>subLoadSavegame</c> (cs1:0xF1FB) is still a symbolic-stub
    /// (asm) routine and the tail falls through into <c>sub_C80F</c>
    /// (cs1:0xA93F).
    /// </summary>
    /// <remarks>Asm byte-verified cs1.bin@0xA90B:
    /// <c>BA DA 37 33 C0 A3 11 38 A3 17 38 A3 1F 38 A2 23 38 E8 DC 48 ...</c>
    /// (0xA91C = <c>E8 DC 48 call subLoadSavegame</c>;
    /// locret_C7DA 0xA90A = <c>C3</c>).</remarks>
    public Action SaveDirInit_1000_A90B_01A90B(int gotoAddress) {
        DX = 0x37DA;                   // mov dx,37DAh
        AX = 0;                        // xor ax,ax
        UInt16[DS, 0x3811] = 0;        // mov ds:3811h,ax
        UInt16[DS, 0x3817] = 0;        // mov ds:3817h,ax
        UInt16[DS, 0x381F] = 0;        // mov ds:381Fh,ax
        UInt8[DS, 0x3823] = 0;         // mov ds:3823h,al
        return NearJump(0xA91C);       // call subLoadSavegame onward (emulated)
    }

    /// <summary>
    /// cs1:0xA8BC — <c>sub_C78C</c>: formats the 8.3-style save-slot display
    /// name into the buffer at <c>ds:0x37DB</c> — letter <c>'A'+bl</c>, the
    /// hex nibbles of <c>bh</c>/<c>bl</c> via
    /// <see cref="Unknown_1000_A8B1_01A8B1"/> (cs1:0xA8B1 sub_C781, NearRet-
    /// inline, call-and-discard-safe), an <c>'O'</c>/<c>'I'</c> flag from
    /// <c>[0xEA]/[6]/[4]</c>, and a trailing space-or-letter. Fully ported.
    /// </summary>
    /// <remarks>
    /// Asm (L46) byte-verified cs1.bin@0xA8BC
    /// (<c>BF DB 37 1E 07 50 8A C3 04 41 AA 47 47 AA 5B B1 04 8A C7 E8 DF FF
    /// AA 8A C3 D2 E8 E8 D7 FF AA 8A C3 E8 D1 FF AA B0 4F 80 3E EA 00 00 7F 10
    /// 80 3E 06 00 80 75 09 80 3E 04 00 01 74 02 B0 49 AA B0 20 D2 EF
    /// 0A 3E E0 47 74 04 8A C7 04 41 AA C3</c>). <c>al=0x49</c> iff
    /// <c>NOT([0xEA]&gt;0 signed) &amp;&amp; [6]==0x80 &amp;&amp; [4]!=1</c>.
    /// </remarks>
    public Action FormatSaveSlotName_1000_A8BC_01A8BC(int gotoAddress) {
        DI = 0x37DB;                                   // mov di,37DBh
        ES = DS;                                       // push ds ; pop es
        ushort savedAx = AX;                           // push ax
        AL = (byte)(BL + 0x41);                        // mov al,bl ; add al,41h
        UInt8[ES, DI] = AL; DI = (ushort)(DI + 1);     // stosb
        DI = (ushort)(DI + 2);                          // inc di ; inc di
        UInt8[ES, DI] = AL; DI = (ushort)(DI + 1);     // stosb
        BX = savedAx;                                  // pop bx
        CL = 4;                                         // mov cl,4
        AL = BH;                                         // mov al,bh
        Unknown_1000_A8B1_01A8B1(0);                    // call sub_C781
        UInt8[ES, DI] = AL; DI = (ushort)(DI + 1);     // stosb
        AL = (byte)(BL >> 4);                            // mov al,bl ; shr al,cl(=4)
        Unknown_1000_A8B1_01A8B1(0);                    // call sub_C781
        UInt8[ES, DI] = AL; DI = (ushort)(DI + 1);     // stosb
        AL = BL;                                         // mov al,bl
        Unknown_1000_A8B1_01A8B1(0);                    // call sub_C781
        UInt8[ES, DI] = AL; DI = (ushort)(DI + 1);     // stosb
        AL = 0x4F;                                       // mov al,4Fh ('O')
        if (!((sbyte)UInt8[DS, 0x00EA] > 0)             // cmp [0xEA],0 ; jg loc_C7CA
              && UInt8[DS, 0x0006] == 0x80              // cmp [6],80h ; jnz loc_C7CA
              && UInt8[DS, 0x0004] != 0x01) {           // cmp [4],1 ; jz loc_C7CA
            AL = 0x49;                                   // mov al,49h ('I')
        }
        UInt8[ES, DI] = AL; DI = (ushort)(DI + 1);     // loc_C7CA: stosb
        AL = 0x20;                                       // mov al,20h (' ')
        byte bh = (byte)((BH >> 4) | UInt8[DS, 0x47E0]); // shr bh,cl ; or bh,ds:47E0h
        BH = bh;
        if (bh != 0) {                                   // jz loc_C7D9
            AL = (byte)(bh + 0x41);                      // mov al,bh ; add al,41h
        }
        UInt8[ES, DI] = AL; DI = (ushort)(DI + 1);     // loc_C7D9: stosb
        return NearRet();                                // locret_C7DA: retn
    }

    /// <summary>
    /// cs1:0x978E — <c>sub_B65E</c>, the 0x978E-campaign root. Runs
    /// <c>sub_699A</c> then bails if <c>[0x47C4]==0xFFFF</c>
    /// (<c>locret_B69E</c> 0x97CE = <c>retn</c>); otherwise rebuilds the
    /// dialogue scene (<c>sub_B070</c>, <c>sub_B7D8</c>, conditional
    /// <c>sub_E347</c>/<c>sub_BA7C</c>/<c>sub_AEF5</c>, <c>sub_DFC4</c>,
    /// <c>jmp sub_E3AD</c>). The <c>sub_699A</c> call (cs1:0x4ACA, NearRet-inline,
    /// call-and-discard-safe) and the <c>[0x47C4]==0xFFFF</c> early-out are
    /// ported in C#; the call-heavy body is delegated to the emulated stream
    /// via <see cref="NearJump"/>(0x9799) because every body callee is
    /// continuation-unsafe (<c>sub_B070</c>/0x91A0 &amp; <c>sub_B7D8</c>/0x9908
    /// have NearJump paths, <c>sub_BA7C</c>/0x9BAC is a near-call-continuation
    /// port, <c>sub_E347</c>/0xC477 &amp; <c>sub_AEF5</c>/0x9025 &amp;
    /// <c>sub_DFC4</c>/0xC0F4 are §A FarJump) — uniform with the 0x91A0
    /// campaign / sub_A77F technique.
    /// </summary>
    /// <remarks>
    /// Asm byte-verified vs cs1.bin@0x978E
    /// (<c>E8 39 B3 A1 C4 47 3D FF FF 74 35 E8 04 FA ...</c>):
    /// <code>
    /// 978E: E8 39 B3    call sub_699A (0x4ACA)
    /// 9791: A1 C4 47    mov ax,ds:47C4h
    /// 9794: 3D FF FF    cmp ax,0FFFFh
    /// 9797: 74 35       jz locret_B69E (0x97CE=retn)
    /// 9799: E8 04 FA    call sub_B070 (0x91A0)   ; body delegated from here
    /// ...   (call sub_B7D8 ; [0x479E] checks ; sub_E347/BA7C/AEF5 ;
    ///        loc_B698: call sub_DFC4 ; jmp sub_E3AD)
    /// </code>
    /// </remarks>
    public Action DialogueSceneRebuild_1000_978E_01978E(int gotoAddress) {
        SetUnknown11CATo1_1000_4ACA_14ACA(0);     // call sub_699A (NearRet-inline)
        AX = UInt16[DS, 0x47C4];                    // mov ax,ds:47C4h
        if (AX == 0xFFFF) {                          // cmp ax,0FFFFh ; jz locret_B69E
            return NearRet();
        }
        return NearJump(0x9799);                      // call sub_B070 onward (emulated body)
    }

    /// <summary>
    /// cs1:0x9908 — <c>sub_B7D8</c>. Builds the scene/dialogue record pointer:
    /// <c>si=[0x47CA]; es=[0xDBB2]; bp = sub_B81F();</c> set
    /// <c>[0x47D1]=0xC0</c>, <c>[0x47CE]=([0x478C]&amp;0xFF)&lt;&lt;2</c>,
    /// <c>si += es:[bp+si]</c>, <c>sub_B83C()</c>, <c>[0x47C8]=si</c>,
    /// take-and-swap <c>[0x47C6]</c>; bail (CF unchanged → <c>locret_B81E</c>
    /// 0x994E = <c>retn</c>) if the swapped value was non-zero or
    /// <c>[0x00EA]&gt;0</c> (signed) or <c>sub_314C(ax=[0x47C4])</c> sets CF;
    /// otherwise falls through into <c>sub_B815</c> (cs1:0x9945). Fully ported
    /// in C# — all three callees (<see cref="ComputeBpFromMenuState_1000_994F_01994F"/>
    /// 0x994F, <c>sub_B83C</c>/0x996C <c>Skip32StringsIf47D0</c>,
    /// <c>sub_314C</c>/0x127C <c>CheckAl4AndByte2ARange</c>) are NearRet-inline
    /// so call-and-discard-safe; the fall-through is the exact
    /// <see cref="NearJump"/>(0x9945) model.
    /// </summary>
    /// <remarks>
    /// Asm byte-verified vs cs1.bin@0x9908
    /// (<c>8B 36 CA 47 8E 06 B2 DB E8 3C 00 C6 06 D1 47 C0 A0 8C 47 32 E4
    /// D1 E0 D1 E0 A3 CE 47 26 03 32 E8 42 00 89 36 C8 47 87 36 C6 47 0B F6
    /// 75 18 80 3E EA 00 00 7F 11 A1 C4 47 E8 39 79 72 09</c>):
    /// <code>
    /// 9908: 8B 36 CA 47    mov si,ds:47CAh
    /// 990C: 8E 06 B2 DB    mov es,ds:0DBB2h
    /// 9910: E8 3C 00       call sub_B81F (0x994F)
    /// 9913: C6 06 D1 47 C0 mov byte ds:47D1h,0C0h
    /// 9918: A0 8C 47       mov al,ds:478Ch
    /// 991B: 32 E4          xor ah,ah
    /// 991D: D1 E0 / D1 E0  shl ax,1 ; shl ax,1
    /// 9921: A3 CE 47       mov ds:47CEh,ax
    /// 9924: 26 03 32       add si,es:[bp+si]
    /// 9927: E8 42 00       call sub_B83C (0x996C)
    /// 992A: 89 36 C8 47    mov ds:47C8h,si
    /// 992E: 87 36 C6 47    xchg si,ds:47C6h
    /// 9932: 0B F6 / 75 18  or si,si ; jnz locret_B81E (0x994E)
    /// 9936: 80 3E EA 00 00 cmp byte ds:0EAh,0
    /// 993B: 7F 11          jg locret_B81E (0x994E)
    /// 993D: A1 C4 47       mov ax,ds:47C4h
    /// 9940: E8 39 79       call sub_314C (0x127C)
    /// 9943: 72 09          jb locret_B81E (0x994E)
    /// 9945: (fall through into sub_B815)
    /// </code>
    /// </remarks>
    public Action BuildSceneRecordPtr_1000_9908_019908(int gotoAddress) {
        SI = UInt16[DS, 0x47CA];                       // mov si,ds:47CAh
        ES = UInt16[DS, 0xDBB2];                        // mov es,ds:0DBB2h
        ComputeBpFromMenuState_1000_994F_01994F(0);     // call sub_B81F -> bp
        UInt8[DS, 0x47D1] = 0xC0;                        // mov byte ds:47D1h,0C0h
        ushort v = (ushort)((UInt8[DS, 0x478C]) << 2);  // al=[0x478C]; xor ah,ah; shl ax,1 ;shl ax,1
        AX = v;
        UInt16[DS, 0x47CE] = v;                          // mov ds:47CEh,ax
        SI = (ushort)(SI + UInt16[ES, (ushort)(BP + SI)]);   // add si,es:[bp+si]
        Skip32StringsIf47D0_1000_996C_1996C(0);          // call sub_B83C
        UInt16[DS, 0x47C8] = SI;                          // mov ds:47C8h,si
        ushort swapped = UInt16[DS, 0x47C6];             // xchg si,ds:47C6h
        UInt16[DS, 0x47C6] = SI;
        SI = swapped;
        if (SI != 0) {                                   // or si,si ; jnz locret_B81E
            return NearRet();
        }
        if ((sbyte)UInt8[DS, 0x00EA] > 0) {              // cmp byte ds:0EAh,0 ; jg locret_B81E
            return NearRet();
        }
        AX = UInt16[DS, 0x47C4];                          // mov ax,ds:47C4h
        CheckAl4AndByte2ARange_1000_127C_1127C(0);        // call sub_314C (sets CF)
        if (CarryFlag) {                                  // jb locret_B81E
            return NearRet();
        }
        return NearJump(0x9945);                          // fall through into sub_B815
    }

    /// <summary>
    /// cs1:0x9BAC — <c>sub_BA7C</c>: <c>push si; call sub_B067; pop si</c>
    /// then falls through into <c>sub_BA81</c> (cs1:0x9BB1). Zero compute;
    /// modelled with the near-call-continuation idiom because
    /// <c>sub_B067</c> (cs1:0x9197 GuardSceneNotTerminator) has a
    /// <c>NearJump(0x91A0)</c> path so call-and-discard is unsafe: push
    /// <c>si</c>, push the raw continuation IP 0x9BB0 (raw <c>pop si</c>
    /// which then falls into the emulated <c>sub_BA81</c>), then
    /// <see cref="NearJump"/> to sub_B067.
    /// </summary>
    /// <remarks>
    /// Asm (5 B) byte-verified vs cs1.bin@0x9BAC (<c>56 E8 E7 F5 5E</c>):
    /// <c>9BAC:56 push si | 9BAD:E8 E7 F5 call sub_B067 (0x9197) |
    /// 9BB0:5E pop si</c> — no retn, falls into sub_BA81@0x9BB1.
    /// </remarks>
    public Action SaveSiCallGuard_1000_9BAC_019BAC(int gotoAddress) {
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = SI;          // push si
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x9BB0;      // push continuation IP (raw `pop si` -> fall into sub_BA81)
        return NearJump(0x9197);      // call sub_B067
    }

    /// <summary>
    /// cs1:0xC4DD — <c>sub_E3AD</c>: <c>ax=[0xDC38]; if ax&lt;0x98
    /// call sub_FA82; loc_E3B8: si=0x1470; jmp sub_E3C0</c>. The
    /// <c>[0xDC38]</c> load + threshold decision and the fast
    /// (<c>ax&gt;=0x98</c>) path are ported in C#; the <c>ax&lt;0x98</c>
    /// path delegates from <c>call sub_FA82</c> (cs1:0xC4E5) to the
    /// emulated stream — <c>sub_FA82</c> (0xDBB2) is a §A FarJump port,
    /// unsafe to call-and-discard. The tail <c>jmp sub_E3C0</c> (cs1:0xC4F0,
    /// C# <c>RectAtSiToRegs</c>) is modelled by <see cref="NearJump"/>.
    /// </summary>
    /// <remarks>
    /// Asm byte-verified vs cs1.bin@0xC4DD
    /// (<c>A1 38 DC 3D 98 00 73 03 E8 CA 16 BE 70 14 EB 03</c>):
    /// <code>
    /// C4DD: A1 38 DC    mov ax,ds:0DC38h
    /// C4E0: 3D 98 00    cmp ax,98h
    /// C4E3: 73 03       jnb loc_E3B8 (0xC4E8)
    /// C4E5: E8 CA 16    call sub_FA82 (0xDBB2)
    /// C4E8: BE 70 14    mov si,1470h            ; loc_E3B8
    /// C4EB: EB 03       jmp sub_E3C0 (0xC4F0)
    /// </code>
    /// </remarks>
    public Action CursorGuardThenRectRegs_1000_C4DD_01C4DD(int gotoAddress) {
        AX = UInt16[DS, 0xDC38];           // mov ax,ds:0DC38h
        if (AX >= 0x0098) {                // cmp ax,98h ; jnb loc_E3B8
            SI = 0x1470;                   // loc_E3B8: mov si,1470h
            return NearJump(0xC4F0);       // jmp sub_E3C0
        }
        return NearJump(0xC4E5);           // call sub_FA82 onward (emulated)
    }

    /// <summary>
    /// cs1:0x994F — <c>sub_B81F</c>: returns <c>bp</c> for the current menu
    /// state. If <c>[0x47D0]==0</c>: <c>bx=6; bp = LcgPrng() + [0x00F0]</c>;
    /// else <c>bp = ([0x47D0]-1) &lt;&lt; 1</c>. Fully ported —
    /// <see cref="LcgPrng_1000_E3B7_01E3B7"/> (cs1:0xE3B7) is NearRet-inline
    /// so call-and-discard-safe.
    /// </summary>
    /// <remarks>
    /// Asm byte-verified vs cs1.bin@0x994F
    /// (<c>A0 D0 47 0A C0 75 0D BB 06 00 E8 5B 4A 8B E8 03 2E F0 00 C3
    /// FE C8 32 E4 D1 E0 8B E8 C3</c>):
    /// <code>
    /// 994F: A0 D0 47    mov al,ds:47D0h
    /// 9952: 0A C0       or al,al
    /// 9954: 75 0D       jnz loc_B833 (0x9963)
    /// 9956: BB 06 00    mov bx,6
    /// 9959: E8 5B 4A    call sub_10287 (0xE3B7)
    /// 995C: 8B E8       mov bp,ax
    /// 995E: 03 2E F0 00 add bp,ds:0F0h
    /// 9962: C3          retn
    /// 9963: FE C8       dec al              ; loc_B833
    /// 9965: 32 E4       xor ah,ah
    /// 9967: D1 E0       shl ax,1
    /// 9969: 8B E8       mov bp,ax
    /// 996B: C3          retn
    /// </code>
    /// </remarks>
    public Action ComputeBpFromMenuState_1000_994F_01994F(int gotoAddress) {
        byte al = UInt8[DS, 0x47D0];          // mov al,ds:47D0h
        AL = al;
        if (al != 0) {                        // or al,al ; jnz loc_B833
            byte d = (byte)(al - 1);          // dec al
            AX = (ushort)(d << 1);            // xor ah,ah ; shl ax,1
            BP = AX;                          // mov bp,ax
            return NearRet();                 // retn
        }
        BX = 0x0006;                          // mov bx,6
        LcgPrng_1000_E3B7_01E3B7(0);          // call sub_10287 (NearRet-inline)
        BP = AX;                              // mov bp,ax
        BP = (ushort)(BP + UInt16[DS, 0x00F0]); // add bp,ds:0F0h
        return NearRet();                     // retn
    }

    /// <summary>
    /// cs1:0xABC6 — <c>sub_CA96</c>. Leaf: <c>mov byte ds:0DC2Bh,0; retn</c>.
    /// Byte-verified vs cs1.bin@0xABC6: <c>C6 06 2B DC 00 C3</c>.
    /// </summary>
    public Action ClearDc2B_1000_ABC6_01ABC6(int gotoAddress) {
        UInt8[DS, 0xDC2B] = 0;   // mov byte ptr ds:0DC2Bh, 0
        return NearRet();        // retn
    }

    /// <summary>
    /// cs1:0xA9A1 — <c>sub_C871</c>. Takes the file handle stashed at
    /// <c>ds:0x3821</c> (atomically clearing the slot), and if it is non-zero
    /// and not the shared DUNE.DAT handle <c>ds:0xDBBA</c>, closes it
    /// (<c>int 21h</c>/AH=3Eh → <see cref="DosFileManager.CloseFileOrDevice"/>).
    /// Always returns CF=0 (<c>clc</c>); AX is preserved across the close
    /// exactly as the asm's <c>push ax/pop ax</c> requires.
    /// </summary>
    /// <remarks>
    /// Asm (24 B), byte-verified vs cs1.bin@0xA9A1
    /// (<c>33 DB 87 1E 21 38 0B DB 74 0C 3B 1E BA DB 74 06 50 B4 3E CD 21 58 F8 C3</c>):
    /// <code>
    /// A9A1: 33 DB          xor bx, bx
    /// A9A3: 87 1E 21 38    xchg bx, ds:3821h        ; bx=old[0x3821]; [0x3821]=0
    /// A9A7: 0B DB          or  bx, bx
    /// A9A9: 74 0C          jz  loc_C887 (0xA9B7)
    /// A9AB: 3B 1E BA DB    cmp bx, ds:0DBBAh
    /// A9AF: 74 06          jz  loc_C887 (0xA9B7)
    /// A9B1: 50             push ax
    /// A9B2: B4 3E          mov ah, 3Eh
    /// A9B4: CD 21          int 21h                  ; DOS close, BX=handle
    /// A9B6: 58             pop ax
    /// A9B7: F8  loc_C887   clc
    /// A9B8: C3  locret     retn
    /// </code>
    /// </remarks>
    public Action CloseFileHandle3821_1000_A9A1_01A9A1(int gotoAddress) {
        ushort bx = UInt16[DS, 0x3821];        // xor bx,bx ; xchg bx,[0x3821]
        UInt16[DS, 0x3821] = 0;
        BX = bx;
        if (bx != 0 && bx != UInt16[DS, 0xDBBA]) {   // or bx,bx/jz ; cmp bx,[0xDBBA]/jz
            ushort axSave = AX;                       // push ax
            Machine.Dos.FileManager.CloseFileOrDevice(bx);   // mov ah,3Eh ; int 21h
            AX = axSave;                              // pop ax
        }
        CarryFlag = false;   // clc
        return NearRet();     // retn
    }

    /// <summary>
    /// cs1:0xD1BB — <c>sub_F08B</c>. Glyph/escape stream processor loop:
    /// reads bytes from <c>es:si</c> (<c>lods es:[si]</c>) and per byte —
    /// <c>0xFF</c> ends (returns), <c>0x0D</c> does the carriage-return
    /// line-advance bookkeeping on <c>[0xD82C..0xD832]</c>, otherwise
    /// maps high-bit bytes to <c>'@'</c> (0x40) and dispatches the char
    /// through the near callback slot <c>ds:[0x2518]</c> — then loops.
    /// </summary>
    /// <remarks>
    /// One C# invocation == one loop iteration; the asm's two
    /// <c>jmp short sub_F08B</c> loop-backs and the post-callback
    /// continuation all return to cs1:0xD1BB (this very override), so
    /// iteration is expressed as re-dispatch, never C# recursion. The
    /// near-indirect <c>call word ptr ds:2518h</c> is modelled with the
    /// established near-call-continuation idiom (push the raw-asm
    /// continuation IP 0xD1CF — which is <c>EB EA</c> = <c>jmp 0xD1BB</c> —
    /// then <see cref="NearJump"/> to <c>[0x2518]</c>; the callee's
    /// <c>retn</c> pops 0xD1CF, the raw jmp re-enters here). The
    /// <c>jz locret_F075</c> exit targets cs1:0xD1A5 which is a bare
    /// <c>C3</c>, hence modelled as <see cref="NearRet"/>.
    /// Asm (50 B), byte-verified vs cs1.bin@0xD1BB
    /// (<c>26 AC 3C FF 74 E4 3C 0D 74 0C 0A C0 79 02 B0 40 FF 16 18 25
    /// EB EA A1 30 D8 A3 2C D8 B8 0A 00 81 3E 18 25 2F D1 75 03 B8 07 00
    /// 01 06 32 D8 01 06 2E D8 EB CC</c>):
    /// <code>
    /// D1BB: 26 AC          lods es:[si]
    /// D1BD: 3C FF / 74 E4  cmp al,0FFh ; jz locret_F075 (0xD1A5 = retn)
    /// D1C1: 3C 0D / 74 0C  cmp al,0Dh  ; jz loc_F0A1 (0xD1D1)
    /// D1C5: 0A C0 / 79 02  or al,al    ; jns loc_F09B (0xD1CB)
    /// D1C9: B0 40          mov al,40h
    /// D1CB: FF 16 18 25    call word ptr ds:2518h     ; near indirect
    /// D1CF: EB EA          jmp sub_F08B (0xD1BB)        ; continuation
    /// D1D1: A1 30 D8       mov ax,[0xD830]              ; loc_F0A1
    /// D1D4: A3 2C D8       mov [0xD82C],ax
    /// D1D7: B8 0A 00       mov ax,0Ah
    /// D1DA: 81 3E 18 25 2F D1  cmp word [0x2518],0D12Fh
    /// D1E0: 75 03          jnz loc_F0B5 (0xD1E5)
    /// D1E2: B8 07 00       mov ax,7
    /// D1E5: 01 06 32 D8    add [0xD832],ax              ; loc_F0B5
    /// D1E9: 01 06 2E D8    add [0xD82E],ax
    /// D1ED: EB CC          jmp sub_F08B (0xD1BB)
    /// </code>
    /// </remarks>
    public Action GlyphStreamProcessor_1000_D1BB_01D1BB(int gotoAddress) {
        AL = UInt8[ES, SI];                 // lods byte ptr es:[si]
        SI = (ushort)(SI + 1);
        if (AL == 0xFF) {                   // cmp al,0FFh ; jz locret_F075 (0xD1A5=retn)
            return NearRet();
        }
        if (AL == 0x0D) {                   // cmp al,0Dh ; jz loc_F0A1
            ushort prev = UInt16[DS, 0xD830];   // mov ax,[0xD830]
            UInt16[DS, 0xD82C] = prev;          // mov [0xD82C],ax
            ushort ax = (UInt16[DS, 0x2518] == 0xD12F) ? (ushort)0x0007 : (ushort)0x000A;
            AX = ax;                             // mov ax,0Ah / (jnz) / mov ax,7
            UInt16[DS, 0xD832] = (ushort)(UInt16[DS, 0xD832] + ax);  // add [0xD832],ax
            UInt16[DS, 0xD82E] = (ushort)(UInt16[DS, 0xD82E] + ax);  // add [0xD82E],ax
            return NearJump(0xD1BB);             // jmp sub_F08B (next iteration)
        }
        if ((AL & 0x80) != 0) {             // or al,al ; jns loc_F09B ; mov al,40h
            AL = 0x40;
        }
        // loc_F09B: call word ptr ds:2518h  (near indirect, continuation @0xD1CF)
        ushort target = UInt16[DS, 0x2518];
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0xD1CF;            // push near-return IP (raw `jmp 0xD1BB`)
        return NearJump(target);
    }

    /// <summary>
    /// cs1:0xD48A — <c>sub_F35A</c>. Text-line render setup. Ports the full
    /// head/compute/branch logic in C# — the framebuffer/font selection
    /// (C# <see cref="SetTextBufferAsActiveFrameBuffer_1000_C08E_01C08E"/>,
    /// <see cref="SetFrontBufferAsActiveFrameBuffer_1000_C07C_01C07C"/>,
    /// <see cref="SetFontToMenu_1000_D075_01D075"/>), the
    /// <c>di = cl*0x0E + 0x1B48</c> record address, the
    /// <see cref="StoreDxBxToD82CD832_1000_D04E_01D04E"/> call, and the
    /// <c>and si,0x3FFF</c> / <c>test ah,0x40</c> / <c>or ah,ah</c> /
    /// <c>xchg al,[0xDBE5]</c> glyph-mode branch — then <see cref="NearJump"/>s
    /// into the emulated call-tail (loc_F3AA cs1:0xD4DA, or loc_F3B9
    /// cs1:0xD4E9 on the <c>si&amp;0x3FFF==0</c> path).
    /// </summary>
    /// <remarks>
    /// The tail is delegated, not call-and-discarded, on purpose: it is four
    /// consecutive continuation-hazard boundaries —
    /// <c>call sub_EE40</c> (0xCF70 FetchSpriteRecord, has a
    /// <c>push 0xCF7B; NearJump(0xD00F)</c> path), the near-indirect
    /// <c>call word ptr ds:[0x2518]</c>, <c>call sub_F08B</c> (0xD1BB, the
    /// iteration-via-redispatch loop override), then the tail far-indirect
    /// <c>call dword ptr ds:[0x38DD]</c> — whose stack effects are only
    /// faithful when run through the real emulated stream (same proven
    /// technique as <c>sub_A77F</c>'s loc_A79A). The entry
    /// <c>push word ptr ds:[0xDBDA]</c> is done here on the emulated stack;
    /// the delegated tail's <c>pop word ptr ds:[0xDBDA]; retn</c> balances it.
    /// Asm (~128 B), byte-verified vs cs1.bin@0xD48A. Continuation map:
    /// EE40 call@0xD4DD, [0x2518]@0xD4E2, F08B call@0xD4E6, loc_F3B9=0xD4E9,
    /// far[0x38DD]@0xD506, pop/ret@0xD50A; loc_F3AA=0xD4DA.
    /// <code>
    /// D48A: FF 36 DA DB    push word ptr ds:0DBDAh
    /// D48E: E8 ..          call sub_DF5E (0xC08E)
    /// D491: 80 3E E6 DC 00 cmp byte ds:0DCE6h,0
    /// D496: 7E 03          jle loc_F36B
    /// D498: E8 ..          call sub_DF4C (0xC07C)
    /// D49B: E8 ..          call sub_EF45 (0xD075)        ; loc_F36B
    /// D49E: 8B F0          mov si,ax
    /// D4A0: B0 0E          mov al,0Eh
    /// D4A2: F6 E1          mul cl
    /// D4A4: 8B F8          mov di,ax
    /// D4A6: 81 C7 48 1B    add di,1B48h
    /// D4AA: 8B 5D 02       mov bx,[di+2]
    /// D4AD: 43             inc bx
    /// D4AE: BA 5D 00       mov dx,5Dh
    /// D4B1: E8 ..          call sub_EF1E (0xD04E)
    /// D4B4: C6 06 E5 DB F3 mov byte ds:0DBE5h,0F3h
    /// D4B9: 80 65 08 7F    and byte [di+8],7Fh
    /// D4BD: 8B C6          mov ax,si
    /// D4BF: 81 E6 FF 3F    and si,3FFFh
    /// D4C3: 74 24          jz loc_F3B9 (0xD4E9)
    /// D4C5: B0 F5          mov al,0F5h
    /// D4C7: F6 C4 40       test ah,40h
    /// D4CA: 75 0E          jnz loc_F3AA (0xD4DA)
    /// D4CC: 80 4D 08 80    or byte [di+8],80h
    /// D4D0: B0 FA          mov al,0FAh
    /// D4D2: 0A E4          or ah,ah
    /// D4D4: 79 04          jns loc_F3AA (0xD4DA)
    /// D4D6: 86 06 E5 DB    xchg al,ds:0DBE5h
    /// D4DA: loc_F3AA  (emulated tail begins: mov [0xDBE4],al; call EE40; ...)
    /// </code>
    /// </remarks>
    public Action RenderTextLineSetup_1000_D48A_01D48A(int gotoAddress) {
        SP = (ushort)(SP - 2);                        // push word ptr ds:0DBDAh
        UInt16[SS, SP] = UInt16[DS, 0xDBDA];
        SetTextBufferAsActiveFrameBuffer_1000_C08E_01C08E(0);   // call sub_DF5E
        if ((sbyte)UInt8[DS, 0xDCE6] > 0) {           // cmp byte [0xDCE6],0 ; jle loc_F36B
            SetFrontBufferAsActiveFrameBuffer_1000_C07C_01C07C(0);   // call sub_DF4C
        }
        SetFontToMenu_1000_D075_01D075(0);            // loc_F36B: call sub_EF45
        SI = AX;                                       // mov si,ax
        AL = 0x0E;                                     // mov al,0Eh
        AX = (ushort)(0x0E * CL);                      // mul cl  (AX = AL*CL)
        DI = (ushort)(AX + 0x1B48);                    // mov di,ax ; add di,1B48h
        BX = (ushort)(UInt16[DS, (ushort)(DI + 2)] + 1);   // mov bx,[di+2] ; inc bx
        DX = 0x005D;                                   // mov dx,5Dh
        StoreDxBxToD82CD832_1000_D04E_01D04E(0);       // call sub_EF1E
        UInt8[DS, 0xDBE5] = 0xF3;                       // mov byte [0xDBE5],0F3h
        UInt8[DS, (ushort)(DI + 8)] =
            (byte)(UInt8[DS, (ushort)(DI + 8)] & 0x7F); // and byte [di+8],7Fh
        ushort origSi = SI;                            // mov ax,si
        AX = origSi;
        SI = (ushort)(SI & 0x3FFF);                    // and si,3FFFh
        if (SI == 0) {                                 // jz loc_F3B9
            return NearJump(0xD4E9);
        }
        AL = 0xF5;                                     // mov al,0F5h
        if ((AH & 0x40) != 0) {                        // test ah,40h ; jnz loc_F3AA
            return NearJump(0xD4DA);
        }
        UInt8[DS, (ushort)(DI + 8)] =
            (byte)(UInt8[DS, (ushort)(DI + 8)] | 0x80); // or byte [di+8],80h
        AL = 0xFA;                                     // mov al,0FAh
        if ((AH & 0x80) == 0) {                        // or ah,ah ; jns loc_F3AA
            return NearJump(0xD4DA);
        }
        byte tmp = UInt8[DS, 0xDBE5];                  // xchg al,ds:0DBE5h
        UInt8[DS, 0xDBE5] = AL;
        AL = tmp;
        return NearJump(0xD4DA);                        // loc_F3AA (emulated tail)
    }

    /// <summary>
    /// cs1:0xD61D — <c>sub_F4ED</c>. On a menu-type change (the value from
    /// <see cref="SetBpToCurrentMenuTypeForScreenAction_1000_D41B_01D41B"/>
    /// differs from <c>[0x1F80]</c>) and when <c>bp == 0x1F7E</c>, runs the
    /// full screen-rebuild sequence; otherwise just the
    /// <see cref="PushAll_1000_E270_01E270"/>/<see cref="PopAll_1000_E283_01E283"/>
    /// bracket. Prologue, the two call-and-discard-safe (NearRet-inline) calls,
    /// the <c>cmp [0x1F80],ax / mov / jz / cmp bp,si / jnz</c> double-branch
    /// and the <c>loc_F519</c> PopAll+pop+ret tail are ported in C#; only the
    /// 3-consecutive-continuation-hazard cluster (<c>call sub_FA82</c> §A,
    /// <c>call sub_F35A</c> NearJump-delegating, <c>call sub_FABC</c> §A) is
    /// delegated to the emulated stream via <see cref="NearJump"/>(0xD636) —
    /// same proven exact technique as <c>sub_A77F</c>/<c>sub_F35A</c>. The
    /// <c>push ax</c> is on the emulated stack; the C# tail's
    /// <c>PopAll;pop ax;ret</c> (or the delegated path's identical
    /// loc_F519@0xD649) balances it.
    /// </summary>
    /// <remarks>
    /// Asm (49 B), byte-verified vs cs1.bin@0xD61D
    /// (<c>50 B8 9F 00 E8 4C 0C E8 F4 FD BE 7E 1F 39 44 02 89 44 02 74 17
    /// 3B EE 75 13 E8 79 05 33 C9 E8 16 FE E8 49 FE C6 06 E7 DC FF E8 A3 05
    /// E8 37 0C 58 C3</c>):
    /// <code>
    /// D61D: 50          push ax
    /// D61E: B8 9F 00    mov ax,9Fh
    /// D621: E8 ..       call sub_10140 (0xE270 PushAll)     ; loc_F4F1
    /// D624: E8 ..       call sub_F2EB  (0xD41B)             ; -> ax
    /// D627: BE 7E 1F    mov si,1F7Eh
    /// D62A: 39 44 02    cmp [si+2],ax
    /// D62D: 89 44 02    mov [si+2],ax
    /// D630: 74 17       jz  loc_F519 (0xD649)
    /// D632: 3B EE       cmp bp,si
    /// D634: 75 13       jnz loc_F519 (0xD649)
    /// D636: E8 ..       call sub_FA82 (0xDBB2 §A)  ─┐ hazard
    /// D639: 33 C9       xor cx,cx                    │ cluster
    /// D63B: E8 ..       call sub_F324 (0xD454)       │ (delegated
    /// D63E: E8 ..       call sub_F35A (0xD48A)       │  to asm)
    /// D641: C6 06 E7 DC FF mov byte [0xDCE7],0FFh    │
    /// D646: E8 ..       call sub_FABC (0xDBEC §A)  ─┘
    /// D649: E8 ..       call sub_10153 (0xE283 PopAll) ; loc_F519
    /// D64C: 58          pop ax
    /// D64D: C3          retn
    /// </code>
    /// </remarks>
    public Action RefreshOnMenuTypeChange_1000_D61D_01D61D(int gotoAddress) {
        SP = (ushort)(SP - 2);                         // push ax
        UInt16[SS, SP] = AX;
        AX = 0x009F;                                   // mov ax,9Fh
        PushAll_1000_E270_01E270(0);                    // call sub_10140  (NearRet-inline)
        SetBpToCurrentMenuTypeForScreenAction_1000_D41B_01D41B(0);  // call sub_F2EB -> ax
        SI = 0x1F7E;                                    // mov si,1F7Eh
        ushort slot = UInt16[DS, (ushort)(SI + 2)];     // cmp [si+2],ax
        bool equal = slot == AX;
        UInt16[DS, (ushort)(SI + 2)] = AX;              // mov [si+2],ax
        if (equal || BP != SI) {                        // jz loc_F519 ; cmp bp,si ; jnz loc_F519
            PopAll_1000_E283_01E283(0);                 // loc_F519: call sub_10153 (NearRet-inline)
            AX = UInt16[SS, SP];                        // pop ax
            SP = (ushort)(SP + 2);
            return NearRet();                            // retn
        }
        return NearJump(0xD636);                          // hazard cluster (emulated tail)
    }

    /// <summary>
    /// cs1:0xA7A5 — <c>sub_C675</c>. Removes the record at <c>si=0xA7C2</c>
    /// from its list (<see cref="ListRemoveRecordBySi_1000_DA5F_1DA5F"/>,
    /// NearRet-inline so call-and-discard-safe), clears <c>[0xDC26]</c>, then
    /// runs the screen-refresh / conditional-teardown tail
    /// (<c>call sub_F4ED; call sub_CA9C; jz locret_C658; call sub_CA96;
    /// call sub_C871; jmp sub_CCBD</c>). The non-call prologue is ported in
    /// C#; the call-chain tail is delegated to the emulated stream via
    /// <see cref="NearJump"/>(0xA7B1) because the very first tail call
    /// <c>sub_F4ED</c> (cs1:0xD61D) is itself a NearJump-delegating port whose
    /// effect is only faithful through a real emulated <c>call</c> (same
    /// proven technique as <c>sub_A77F</c>); the rest (CA9C's ZF result for
    /// <c>jz locret_C658</c>, CA96/C871 C# leaves, the <c>jmp sub_CCBD</c>
    /// §A tail) all dispatch correctly there.
    /// </summary>
    /// <remarks>
    /// Asm (10 instr), byte-verified vs cs1.bin@0xA7A5
    /// (<c>BE C2 A7 E8 B4 32 C7 06 26 DC 00 00 E8 69 2E E8 15 04 74 CF
    /// E8 0A 04 E8 E2 01 E9 2B 06</c>):
    /// <code>
    /// A7A5: BE C2 A7          mov si,0A7C2h
    /// A7A8: E8 B4 32          call sub_F92F (0xDA5F)
    /// A7AB: C7 06 26 DC 00 00 mov word ds:0DC26h,0
    /// A7B1: E8 69 2E          call sub_F4ED (0xD61D)        ; delegate from here
    /// A7B4: E8 15 04          call sub_CA9C (0xABCC)
    /// A7B7: 74 CF             jz locret_C658 (0xA788=retn)
    /// A7B9: E8 0A 04          call sub_CA96 (0xABC6)
    /// A7BC: E8 E2 01          call sub_C871 (0xA9A1)
    /// A7BF: E9 2B 06          jmp sub_CCBD (0xADED)
    /// </code>
    /// </remarks>
    public Action ResetAndRefresh_1000_A7A5_01A7A5(int gotoAddress) {
        SI = 0xA7C2;                              // mov si,0A7C2h
        ListRemoveRecordBySi_1000_DA5F_1DA5F(0);   // call sub_F92F (NearRet-inline)
        UInt16[DS, 0xDC26] = 0;                     // mov word ds:0DC26h,0
        return NearJump(0xA7B1);                     // call sub_F4ED onward (emulated tail)
    }

    /// <summary>
    /// cs1:0x9B8B — <c>sub_BA5B</c> entry. Models <c>call sub_C675</c>
    /// (cs1:0xA7A5, itself a NearJump-delegating port — so it must run through
    /// a real emulated call) with the established near-call-continuation
    /// idiom: push the post-call IP 0x9B8E and
    /// <see cref="NearJump"/>(0xA7A5). When the sub_C675 chain returns it
    /// pops 0x9B8E → <see cref="TeardownDialogueStateCont_1000_9B8E_019B8E"/>.
    /// </summary>
    /// <remarks>
    /// Asm: <c>9B8B: E8 17 0C  call sub_C675</c> (byte-verified
    /// vs cs1.bin@0x9B8B; <c>0x9B8E+0x0C17=0xA7A5</c>).
    /// </remarks>
    public Action TeardownDialogueState_1000_9B8B_019B8B(int gotoAddress) {
        SP = (ushort)(SP - 2);
        UInt16[SS, SP] = 0x9B8E;     // push return IP (continuation)
        return NearJump(0xA7A5);     // call sub_C675
    }

    /// <summary>
    /// cs1:0x9B8E — continuation of <see cref="TeardownDialogueState_1000_9B8B_019B8B"/>
    /// after <c>sub_C675</c> returns. Pure-compute teardown: zero
    /// <c>[0x47C3]</c>/<c>[0x47CE]</c>, clear bit 7 of <c>[0x47D1]</c>,
    /// take-and-clear <c>[0x47C6]</c>; if it was zero return
    /// (<c>locret_BA7B</c>, 0x9BAB = <c>retn</c>), else <c>si=0x99BE</c> and
    /// tail-jump <c>sub_F92F</c> (cs1:0xDA5F, whose <c>retn</c> returns on
    /// this routine's behalf).
    /// </summary>
    /// <remarks>
    /// Asm (post-call body, byte-verified vs cs1.bin@0x9B8E:
    /// <c>33 C0 C6 06 C3 47 00 A3 CE 47 80 26 D1 47 7F 87 06 C6 47 0B C0
    /// 74 06 BE BE 99 E9 B4 3E</c>):
    /// <code>
    /// 9B8E: 33 C0          xor ax,ax
    /// 9B90: C6 06 C3 47 00 mov byte ds:47C3h,0
    /// 9B95: A3 CE 47       mov ds:47CEh,ax
    /// 9B98: 80 26 D1 47 7F and byte ds:47D1h,7Fh
    /// 9B9D: 87 06 C6 47    xchg ax,ds:47C6h
    /// 9BA1: 0B C0          or ax,ax
    /// 9BA3: 74 06          jz locret_BA7B (0x9BAB=retn)
    /// 9BA5: BE BE 99       mov si,99BEh
    /// 9BA8: E9 B4 3E       jmp sub_F92F (0xDA5F)
    /// </code>
    /// </remarks>
    public Action TeardownDialogueStateCont_1000_9B8E_019B8E(int gotoAddress) {
        AX = 0;                                         // xor ax,ax
        UInt8[DS, 0x47C3] = 0;                           // mov byte ds:47C3h,0
        UInt16[DS, 0x47CE] = 0;                          // mov ds:47CEh,ax (ax=0)
        UInt8[DS, 0x47D1] = (byte)(UInt8[DS, 0x47D1] & 0x7F); // and byte ds:47D1h,7Fh
        ushort prev = UInt16[DS, 0x47C6];                // xchg ax,ds:47C6h
        UInt16[DS, 0x47C6] = 0;                           //   (ax was 0)
        AX = prev;
        if (AX == 0) {                                   // or ax,ax ; jz locret_BA7B
            return NearRet();
        }
        SI = 0x99BE;                                     // mov si,99BEh
        return NearJump(0xDA5F);                          // jmp sub_F92F
    }

    /// <summary>
    /// cs1:0x98B2 — <c>sub_B782</c>. Early-out if <c>[0x47C3]!=0</c>; else
    /// resets the dialogue render state (<c>[0x4540]=0</c>, clears the top
    /// two bits of <c>[0x47D1]</c>, take-and-clear <c>[0x47C8]</c> — early-out
    /// if it was zero, <c>[0x1BF8]=0</c>, <c>[0x1C06]=0</c>) then runs the
    /// blit/rect/teardown chain (<c>call sub_E316; mov si,0x1BF0;
    /// call sub_E3C0; jmp sub_BA5B</c>). The whole head + both early returns
    /// (<c>locret_B7B5</c> 0x98E5 = <c>retn</c>) are ported in C#; the chain
    /// tail is delegated to the emulated stream via <see cref="NearJump"/>(0x98D9)
    /// because all three tail targets are continuation-unsafe to
    /// call-and-discard: <c>sub_E316</c> (0xC446) has a <c>FarJump</c> §A path,
    /// <c>sub_E3C0</c> (0xC4F0) returns <c>NearJump(0xC4FB)</c>, and
    /// <c>sub_BA5B</c> (0x9B8B) is a continuation-split port — all dispatch
    /// correctly when reached via the real emulated calls (sub_A77F technique).
    /// </summary>
    /// <remarks>
    /// Asm (20 instr), byte-verified vs cs1.bin@0x98B2
    /// (<c>80 3E C3 47 00 75 2C 33 C0 A3 40 45 80 26 D1 47 3F 87 06 C8 47
    /// 0B C0 74 1A BE F0 1B C7 44 08 00 00 C7 06 06 1C 00 00 E8 6A 2B
    /// BE F0 1B E8 0E 2C E9 A6 02 C3</c>):
    /// <code>
    /// 98B2: 80 3E C3 47 00 cmp byte ds:47C3h,0
    /// 98B7: 75 2C          jnz locret_B7B5 (0x98E5=retn)
    /// 98B9: 33 C0          xor ax,ax
    /// 98BB: A3 40 45       mov ds:4540h,ax
    /// 98BE: 80 26 D1 47 3F and byte ds:47D1h,3Fh
    /// 98C3: 87 06 C8 47    xchg ax,ds:47C8h
    /// 98C7: 0B C0          or ax,ax
    /// 98C9: 74 1A          jz locret_B7B5 (0x98E5)
    /// 98CB: BE F0 1B       mov si,1BF0h
    /// 98CE: C7 44 08 00 00 mov word [si+8],0
    /// 98D3: C7 06 06 1C 00 00 mov word ds:1C06h,0
    /// 98D9: E8 6A 2B       call sub_E316 (0xC446)   ; delegate from here
    /// 98DC: BE F0 1B       mov si,1BF0h
    /// 98DF: E8 0E 2C       call sub_E3C0 (0xC4F0)
    /// 98E2: E9 A6 02       jmp sub_BA5B (0x9B8B)
    /// 98E5: C3             retn                      ; locret_B7B5
    /// </code>
    /// </remarks>
    public Action ResetRenderStateThenChain_1000_98B2_0198B2(int gotoAddress) {
        if (UInt8[DS, 0x47C3] != 0) {                 // cmp byte [0x47C3],0 ; jnz locret_B7B5
            return NearRet();
        }
        AX = 0;                                        // xor ax,ax
        UInt16[DS, 0x4540] = 0;                         // mov ds:4540h,ax
        UInt8[DS, 0x47D1] = (byte)(UInt8[DS, 0x47D1] & 0x3F);   // and byte ds:47D1h,3Fh
        ushort prev = UInt16[DS, 0x47C8];              // xchg ax,ds:47C8h
        UInt16[DS, 0x47C8] = 0;                          //   (ax was 0)
        AX = prev;
        if (AX == 0) {                                  // or ax,ax ; jz locret_B7B5
            return NearRet();
        }
        SI = 0x1BF0;                                    // mov si,1BF0h
        UInt16[DS, (ushort)(SI + 8)] = 0;               // mov word [si+8],0
        UInt16[DS, 0x1C06] = 0;                          // mov word ds:1C06h,0
        return NearJump(0x98D9);                          // call sub_E316 onward (emulated tail)
    }

    /// <summary>
    /// cs1:0x91A0 — <c>sub_B070</c>, the campaign root. Computes the dialogue
    /// display mode into <c>[0x00F0]</c> (default 0; <c>0x0A</c> iff
    /// <c>ax==0x0C</c> <b>and</b> <c>[0x10A7] &amp; 0x10</c>), then enters
    /// <c>loc_B088</c> which resolves the topic slot
    /// (<c>call sub_AFF3</c>), conditionally tail-jumps <c>sub_B0DF</c>,
    /// rebuilds render state (<c>call sub_B782</c>, <c>call sub_B0DF</c>) and
    /// performs the <c>lds [0xDBB0]</c> record-pointer / <c>movsw×4</c> copy
    /// into <c>[0x1BF0]</c> with the <c>[0x47CC]/[0x47CA]/[0x47D2]</c> fixups.
    /// The mode-decision head is ported in C#; <c>loc_B088</c> onward is
    /// delegated to the emulated stream via <see cref="NearJump"/>(0x91B8) —
    /// every one of its three callees is continuation-unsafe to
    /// call-and-discard (<c>sub_AFF3</c>/0x9123 has a <c>NearJump(0x917A)</c>
    /// path, <c>sub_B782</c>/0x98B2 is a NearJump-delegating port,
    /// <c>sub_B0DF</c>/0x920F is an always-<c>NearJump(0xC13E)</c> thunk), so
    /// the only exact model is to run them through real emulated calls
    /// (uniform with the rest of this campaign and the sub_A77F precedent).
    /// </summary>
    /// <remarks>
    /// Asm head, byte-verified vs cs1.bin@0x91A0
    /// (<c>C7 06 F0 00 00 00 3D 0C 00 75 0D F6 06 A7 10 10 74 06
    /// C7 06 F0 00 0A 00 E8 68 FF ...</c>):
    /// <code>
    /// 91A0: C7 06 F0 00 00 00 mov word ds:0F0h,0
    /// 91A6: 3D 0C 00          cmp ax,0Ch
    /// 91A9: 75 0D             jnz loc_B088 (0x91B8)
    /// 91AB: F6 06 A7 10 10    test byte ds:10A7h,10h
    /// 91B0: 74 06             jz loc_B088 (0x91B8)
    /// 91B2: C7 06 F0 00 0A 00 mov word ds:0F0h,0Ah
    /// 91B8: E8 68 FF          call sub_AFF3 (0x9123)   ; loc_B088 (delegated)
    /// </code>
    /// </remarks>
    public Action ComputeDialogueModeThenSetup_1000_91A0_0191A0(int gotoAddress) {
        UInt16[DS, 0x00F0] = 0;                          // mov word ds:0F0h,0
        if (AX == 0x000C && (UInt8[DS, 0x10A7] & 0x10) != 0) {  // cmp ax,0Ch/jnz ; test [0x10A7],10h/jz
            UInt16[DS, 0x00F0] = 0x000A;                 // mov word ds:0F0h,0Ah
        }
        return NearJump(0x91B8);                          // loc_B088 (emulated tail)
    }
}
