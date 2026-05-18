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
        DefineFunction(cs1, 0x8C8A, FlushPendingTextWrites_1000_8C8A_018C8A);
        DefineFunction(cs1, 0xD0E3, LookupKeyInCsTable_1000_D0E3_01D0E3);
        DefineFunction(cs1, 0x7B0F, ResetThenSwapSiDiCallDfb8_1000_7B0F_017B0F);
        DefineFunction(cs1, 0x9D94, EmitGlyphRunLoop_1000_9D94_019D94);
        DefineFunction(cs1, 0x9D6A, EmitGlyphList_1000_9D6A_019D6A);
        DefineFunction(cs1, 0x9D6F, EmitGlyphListLoop_1000_9D6F_019D6F);
        DefineFunction(cs1, 0x8ED3, MeasureTextWidth_1000_8ED3_018ED3);
        DefineFunction(cs1, 0x8F28, SetupTextBoxGeometry_1000_8F28_018F28);
        DefineFunction(cs1, 0x8E16, LayoutTextLines_1000_8E16_018E16);
        DefineFunction(cs1, 0x79EE, MeasureMenuEntry_1000_79EE_0179EE);
        DefineFunction(cs1, 0x8CCD, BuildMenuLayout_1000_8CCD_018CCD);
        DefineFunction(cs1, 0x8B11, RenderDialogueBox_1000_8B11_018B11);
        // cs1:0xC370 sub_E240 — intentionally NOT overridden: pure §A
        // orchestrator (first op is a §A SS-far call; L87 with multiple
        // interleaved §A SS-far calls + lds + bp-frame + 2 loops, no
        // portable compute head). Exact-as-emulated — a C# shell would
        // add zero fidelity and a full multi-§A-continuation
        // reconstruction would only add fragility. Reached only via
        // sub_ADF8's emulated-delegated tail, where it executes exactly.
    }

    /// <summary>
    /// Registers the <c>cs1:0xC13E</c> (<c>sub_E00E</c>) deep-chain campaign
    /// overrides (10-node bounded subtree; <c>tools/chain_tree.py</c>).
    /// </summary>
    public void DefineChainC13ECodeOverrides() {
        DefineFunction(cs1, 0xC1AA, ToggleDbb4FromCounter_1000_C1AA_01C1AA);
        DefineFunction(cs1, 0xEBAA, ClearAndWalkEsList_1000_EBAA_01EBAA);
        DefineFunction(cs1, 0xF229, ReloadSaveThenCopy36C4_1000_F229_01F229);
        DefineFunction(cs1, 0xEBE3, ResCacheReclaim_1000_EBE3_01EBE3);
        DefineFunction(cs1, 0xEB74, FindOldestCacheSlot_1000_EB74_01EB74);
        DefineFunction(cs1, 0xF244, OpenResRetryLoop_1000_F244_01F244);
        DefineFunction(cs1, 0xF0D6, EnsureResCacheSpace_1000_F0D6_01F0D6);
        DefineFunction(cs1, 0xF0B9, LoadResById_1000_F0B9_01F0B9);
        DefineFunction(cs1, 0xEAB7, ReclaimCachePages_1000_EAB7_01EAB7);
        DefineFunction(cs1, 0xC13E, ResolveResHandle_1000_C13E_01C13E);
    }

    /// <summary>
    /// cs1:0xEAB7 — <c>sub_10987</c> (L104). Large cache-page reclaim
    /// orchestrator with a 6-byte bp-frame, <c>repne scasw</c> scan and two
    /// loops. No portable compute head — first op is
    /// <c>call sub_10140</c> (cs1:0xE270 PushAll, NearRet-inline so
    /// call-and-discard-safe). The L104 body (the bp-frame setup, the
    /// <c>repne scasw</c>/<c>sub_10A44</c>/<c>sub_10B29</c>/<c>sub_10A7A</c>
    /// loops and the closing <c>call sub_10153</c> PopAll) is delegated to
    /// the emulated stream via <see cref="NearJump"/>(0xEABA) — faithful
    /// only when the bp-frame, repne and the calls run emulated.
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0xEAB7
    /// (<c>E8 B6 F7 1E 06 83 EC 06 8B EC ...</c>): call sub_10140 @0xEAB7 →
    /// 0xE270, body @0xEABA.</remarks>
    public Action ReclaimCachePages_1000_EAB7_01EAB7(int gotoAddress) {
        PushAll_1000_E270_01E270(0);          // call sub_10140 (NearRet-safe)
        return NearJump(0xEABA);              // L104 bp-frame/loop body (emulated)
    }

    /// <summary>
    /// cs1:0xC13E — <c>sub_E00E</c>, the <b>0xC13E-campaign root</b> (L64).
    /// Resolves/caches a resource handle keyed by <c>ax</c>. Head ported in
    /// C#: <c>or ax,ax; js locret_E079</c> (negative → retn 0xC1A9);
    /// <c>push bx; bx=ax; xchg bx,[0x2784]; cmp ax,bx; jz loc_E078</c>
    /// (unchanged → 0xC1A8 pop bx; retn). The body (cs1:0xC14D onward —
    /// the <c>0xD844</c> table walk, <c>les di,[si]</c>, the
    /// continuation-unsafe <c>sub_E07A</c>/<c>sub_10F89</c> calls and the
    /// §A <c>call dword ptr ds:[0x3905]</c>) is delegated to the emulated
    /// stream via <see cref="NearJump"/>; the pushed <c>bx</c> is balanced
    /// by the emulated <c>loc_E078: pop bx; retn</c>.
    /// </summary>
    /// <remarks>Asm head byte-verified vs cs1.bin@0xC13E
    /// (<c>0B C0 78 67 53 8B D8 87 1E 84 27 3B C3 74 5B 56 57</c>):
    /// js +0x67 → locret_E079 0xC1A9; jz +0x5B → loc_E078 0xC1A8;
    /// body @0xC14D.</remarks>
    public Action ResolveResHandle_1000_C13E_01C13E(int gotoAddress) {
        if ((short)AX < 0) {                              // or ax,ax ; js locret_E079
            return NearRet();
        }
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BX;       // push bx
        ushort bxNew = AX;                                 // mov bx,ax
        ushort old = UInt16[DS, 0x2784];                   // xchg bx,ds:2784h
        UInt16[DS, 0x2784] = bxNew;
        BX = old;
        if (AX == old) {                                   // cmp ax,bx ; jz loc_E078
            return NearJump(0xC1A8);                        // loc_E078: pop bx; retn (emulated)
        }
        return NearJump(0xC14D);                            // body (emulated)
    }

    /// <summary>
    /// cs1:0xEBE3 — <c>sub_10AB3</c>. Resource-cache reclaim. Head ported in
    /// C# (push dx/ds; <c>si=[0x39A9]; ds=[0xCE6C]; si=(si+ax)*2;
    /// ax=ds:[si]</c>; null → <c>loc_10B13</c> 0xEC43 = <c>pop ds;pop dx;
    /// retn</c>). The reclaim loop body (cs1:0xEBF7) — which calls the
    /// continuation-unsafe §A <c>sub_10B16</c> (cs1:0xEC46) — is delegated
    /// to the emulated stream via <see cref="NearJump"/>; the pushed dx/ds
    /// are balanced by the emulated tail's <c>pop ds;pop dx;retn</c>.
    /// </summary>
    /// <remarks>Asm head byte-verified vs cs1.bin@0xEBE3
    /// (<c>52 1E 8B 36 A9 39 8E 1E 6C CE 03 F0 D1 E6 8B 04 0B C0 74 4C</c>):
    /// jz +0x4C → loc_10B13 0xEC43; body @0xEBF7.</remarks>
    public Action ResCacheReclaim_1000_EBE3_01EBE3(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DX;       // push dx
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DS;       // push ds
        SI = UInt16[DS, 0x39A9];                           // mov si,ds:39A9h
        DS = UInt16[DS, 0xCE6C];                            // mov ds,ds:0CE6Ch
        SI = (ushort)(SI + AX);                             // add si,ax
        SI = (ushort)(SI << 1);                             // shl si,1
        AX = UInt16[DS, SI];                                // mov ax,[si]
        if (AX == 0) {                                       // or ax,ax ; jz loc_10B13
            return NearJump(0xEC43);                         // pop ds;pop dx;retn (emulated)
        }
        return NearJump(0xEBF7);                             // reclaim loop body (emulated)
    }

    /// <summary>
    /// cs1:0xEB74 — <c>sub_10A44</c>. Scans the 0xB9-entry cache table at
    /// <c>es:[bp+0]</c> for the oldest live slot (max
    /// <c>word_10945 - es:[si+0x172]</c>) and, if found, retires it via
    /// <see cref="ClearAndWalkEsList_1000_EBAA_01EBAA"/> (sub_10A7A —
    /// NearRet-inline, call-and-discard-safe). Fully ported.
    /// </summary>
    /// <remarks>Asm (L32) byte-verified vs cs1.bin@0xEB74
    /// (<c>52 56 8B 76 00 B9 B9 00 2E 8B 16 75 EA 33 DB ... E8 03 00 5E ..</c>):
    /// <c>loop loc_10A53</c> (dec cx; jnz); <c>jb loc_10A69</c> skips the
    /// di/bx update when ax&lt;bx. <c>word_10945</c> is the cs:0xEA75 word.</remarks>
    public Action FindOldestCacheSlot_1000_EB74_01EB74(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DX;       // push dx
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;       // push si
        ushort si = UInt16[SS, (ushort)(BP + 0)];          // mov si,[bp+0]
        ushort cx = 0x00B9;                                 // mov cx,0B9h
        ushort dx = UInt16[cs1, 0xEA75];                    // mov dx,cs:word_10945
        ushort bx = 0;                                       // xor bx,bx
        ushort di = DI;
        do {
            ushort ax = UInt16[ES, si];                     // mov ax,es:[si]
            if (ax != 0) {                                   // or ax,ax ; jz loc_10A69
                ax = (ushort)(dx - UInt16[ES, (ushort)(si + 0x172)]); // ax=dx; sub ax,es:[si+172h]
                if (ax >= bx) {                              // cmp ax,bx ; jb loc_10A69
                    di = si;                                 // mov di,si
                    bx = ax;                                 // mov bx,ax
                }
            }
            si = (ushort)(si + 2);                           // loc_10A69: add si,2
            cx = (ushort)(cx - 1);                           // loop loc_10A53
        } while (cx != 0);
        SI = si; DI = di; BX = bx; DX = dx; CX = 0;
        if (bx != 0) {                                       // or bx,bx ; jz loc_10A77
            SI = di;                                         // mov si,di
            ClearAndWalkEsList_1000_EBAA_01EBAA(0);          // call sub_10A7A (NearRet-safe)
        }
        SI = UInt16[SS, SP]; SP = (ushort)(SP + 2);          // loc_10A77: pop si
        DX = UInt16[SS, SP]; SP = (ushort)(SP + 2);          // pop dx
        return NearRet();                                     // retn
    }

    /// <summary>
    /// cs1:0xF244 — <c>sub_11114</c>. Retry loop: <c>push dx;
    /// call sub_110F9; pop dx; cmp bx,[0xDBBA]; jnz sub_11130;
    /// call sub_111BA; jb sub_11114; retn</c>. First op is the call to the
    /// continuation-port <c>sub_110F9</c> (cs1:0xF229) so it is modelled
    /// with the call-continuation idiom: push dx, push raw post-call IP
    /// 0xF248, <see cref="NearJump"/> to sub_110F9; the
    /// <c>cmp/jnz/call sub_111BA/jb-loop/retn</c> tail (which re-enters this
    /// override on the <c>jb sub_11114</c> back-edge) runs emulated.
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0xF244
    /// (<c>52 E8 E1 FF 5A 3B 1E BA DB 75 11 E8 98 00 72 F0 C3</c>):
    /// call sub_110F9 @0xF245 → 0xF229, continuation @0xF248.</remarks>
    public Action OpenResRetryLoop_1000_F244_01F244(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DX;       // push dx
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xF248;   // call sub_110F9 (cont = raw tail)
        return NearJump(0xF229);
    }

    /// <summary>
    /// cs1:0xF0D6 — <c>sub_10FA6</c>. Ensures resource-cache space. Head
    /// decision ported in C# (<c>ax=[0xCE78]; cmp al,[0xCE70]</c>); the
    /// call-heavy body — <c>call sub_10AB3</c> /
    /// <c>loc_10FB4: call sub_11114; …; call sub_10987</c> and
    /// <c>jmp loc_112A3</c> into the FUNCTION CHUNK @0xF3D3 — is delegated
    /// to the emulated stream via <see cref="NearJump"/> (0xF0DF when
    /// <c>al &lt; [0xCE70]</c>, else loc_10FB4 0xF0E4).
    /// </summary>
    /// <remarks>Asm head byte-verified vs cs1.bin@0xF0D6
    /// (<c>A1 78 CE 3A 06 70 CE 73 05 E8 01 FB 72 0F E8 5D 01 ...</c>):
    /// jnb +5 → loc_10FB4 0xF0E4.</remarks>
    public Action EnsureResCacheSpace_1000_F0D6_01F0D6(int gotoAddress) {
        AX = UInt16[DS, 0xCE78];                           // mov ax,ds:0CE78h
        if (AL >= UInt8[DS, 0xCE70]) {                     // cmp al,ds:0CE70h ; jnb loc_10FB4
            return NearJump(0xF0E4);                         // loc_10FB4 (emulated)
        }
        return NearJump(0xF0DF);                             // call sub_10AB3 ... (emulated)
    }

    /// <summary>
    /// cs1:0xF0B9 — <c>sub_10F89</c>. Resolves a resource by id: records
    /// <c>[0xCE78]=si</c>, indexes the <c>0x31FF</c> table
    /// (<c>si=[si*2+0x31FF]</c>), reads the size word; size 0 → tail to
    /// <c>sub_10FA6</c> (cs1:0xF0D6); else delegates
    /// <c>cx=ax; push dx; call sub_10FEC; pop dx; call sub_10FA6;
    /// jmp sub_10FCF</c> to the emulated stream (cs1:0xF0CA) — both
    /// sub_10FEC/0xF11C and sub_10FA6 are reached via real emulated calls.
    /// The address-computation head is ported in C#.
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0xF0B9
    /// (<c>89 36 78 CE D1 E6 8B B4 FF 31 AD 8B D6 0B C0 74 0C 8B C8 52
    /// E8 4C 00 5A E8 02 00 EB 29</c>): jz +0xC → sub_10FA6 0xF0D6;
    /// delegate point 0xF0CA.</remarks>
    public Action LoadResById_1000_F0B9_01F0B9(int gotoAddress) {
        UInt16[DS, 0xCE78] = SI;                            // mov ds:0CE78h,si
        ushort si = (ushort)(SI << 1);                      // shl si,1
        si = UInt16[DS, (ushort)(si + 0x31FF)];             // mov si,[si+31FFh]
        ushort ax = UInt16[DS, si];                         // lodsw
        si = (ushort)(si + 2);
        SI = si;
        DX = si;                                             // mov dx,si
        AX = ax;
        if (ax == 0) {                                       // or ax,ax ; jz sub_10FA6
            return NearJump(0xF0D6);                          // tail -> sub_10FA6 (emulated)
        }
        return NearJump(0xF0CA);                              // cx=ax; push dx; call sub_10FEC ... (emulated)
    }

    /// <summary>
    /// cs1:0xC1AA — <c>sub_E07A</c>. Snapshots the low byte of the
    /// <c>[0x2784]</c> counter into <c>[0xDBB4]</c> (<c>xchg</c>); if it was
    /// unchanged returns (<c>locret_E079</c> 0xC1A9 = <c>retn</c>),
    /// otherwise sets <c>si=2</c> and falls through into <c>sub_E08A</c>
    /// (cs1:0xC1BA, delegated to the emulated stream via
    /// <see cref="NearJump"/>).
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0xC1AA
    /// (<c>A1 84 27 8A E0 86 06 B4 DB 3A C4 74 F2 BE 02 00</c>):
    /// <c>jz -0xE</c> → locret_E079 0xC1A9; no retn, falls into 0xC1BA.</remarks>
    public Action ToggleDbb4FromCounter_1000_C1AA_01C1AA(int gotoAddress) {
        AX = UInt16[DS, 0x2784];                          // mov ax,ds:2784h
        AH = AL;                                           // mov ah,al
        byte old = UInt8[DS, 0xDBB4];                      // xchg al,ds:0DBB4h
        UInt8[DS, 0xDBB4] = AL;
        AL = old;
        if (AL == AH) {                                    // cmp al,ah ; jz locret_E079
            return NearRet();
        }
        SI = 2;                                            // mov si,2
        return NearJump(0xC1BA);                            // fall into sub_E08A (emulated)
    }

    /// <summary>
    /// cs1:0xEBAA — <c>sub_10A7A</c>. Walks/clears an <c>es:</c> linked list:
    /// per node take-and-zero <c>es:[si]</c>, <c>bx&lt;&lt;=1</c> (CF = old
    /// bit 15 = sentinel), <c>si = bx-2</c>; continues while CF==0,
    /// returns when the sentinel bit is set. Call-free — fully ported as a
    /// C# loop.
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0xEBAA
    /// (<c>33 DB 26 87 1C D1 E3 8B F3 4E 4E 73 F3 C3</c>):
    /// <c>jnb sub_10A7A</c> loops while CF==0 (CF from <c>shl bx,1</c>;
    /// the two <c>dec si</c> don't affect CF).</remarks>
    public Action ClearAndWalkEsList_1000_EBAA_01EBAA(int gotoAddress) {
        while (true) {
            ushort bx = UInt16[ES, SI];                    // xor bx,bx ; xchg bx,es:[si]
            UInt16[ES, SI] = 0;
            bool cf = (bx & 0x8000) != 0;                   // shl bx,1 -> CF = old bit15
            bx = (ushort)(bx << 1);
            BX = bx;
            SI = (ushort)(bx - 2);                          // mov si,bx ; dec si ; dec si
            if (cf) {                                       // jnb sub_10A7A (loop while CF==0)
                return NearRet();
            }
        }
    }

    /// <summary>
    /// cs1:0xF229 — <c>sub_110F9</c>. <c>call subLoadSavegame</c> then on
    /// success returns, else copies the 12-byte header (<c>ds:dx</c> →
    /// <c>0x36C4</c>), points <c>[0x3CBC]</c> at <c>0x36B4</c> and
    /// <c>jmp</c>s out. The first op is the call; <c>subLoadSavegame</c>
    /// (cs1:0xF1FB) is still a symbolic-stub (asm) routine, so this is
    /// modelled with the call-continuation idiom: push the raw post-call IP
    /// 0xF22C, <see cref="NearJump"/> to subLoadSavegame; the
    /// <c>jb/retn</c> + <c>rep movsb</c> + out-jmp tail runs in the emulated
    /// stream.
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0xF229
    /// (<c>E8 CF FF 72 01 C3 8B F2 BF C4 36 B9 0C 00 1E 07 F3 A4
    /// C7 06 BC 3C B4 36 E9 F6 0D</c>): call subLoadSavegame @0xF229 →
    /// 0xF1FB, continuation @0xF22C.</remarks>
    public Action ReloadSaveThenCopy36C4_1000_F229_01F229(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xF22C;   // call subLoadSavegame (cont = raw tail)
        return NearJump(0xF1FB);
    }

    /// <summary>
    /// Registers the <c>cs1:0x5DCE</c> (<c>sub_7C9E</c>) deep-chain campaign
    /// overrides (9-node bounded subtree, the final verb-13 cluster item;
    /// <c>tools/chain_tree.py</c>).
    /// </summary>
    public void DefineChain5DCECodeOverrides() {
        DefineFunction(cs1, 0x62C9, IsRecordVisibleGate_1000_62C9_0162C9);
        DefineFunction(cs1, 0x62D6, PointInClipRect_1000_62D6_0162D6);
        DefineFunction(cs1, 0x63C7, SpriteCellDispatch_1000_63C7_0163C7);
        DefineFunction(cs1, 0x7C8F, ScaledDistanceClamp_1000_7C8F_017C8F);
        DefineFunction(cs1, 0x7C93, ScaledDistanceClampCont_1000_7C93_017C93);
    }

    /// <summary>
    /// cs1:0x62C9 — <c>sub_8199</c>. If <c>[0x46EB] == 0</c> returns
    /// out-of-bounds (CF=1, <c>locret_81C1</c> 0x62F1 = <c>retn</c>);
    /// otherwise loads <c>dx=[si+2]; bx=[si+4]</c> and falls through into
    /// <c>sub_81A6</c> (cs1:0x62D6) — modelled by
    /// <see cref="NearJump"/>(0x62D6) so the C#
    /// <see cref="PointInClipRect_1000_62D6_0162D6"/> override dispatches.
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0x62C9
    /// (<c>80 3E EB 46 01 72 21 8B 54 02 8B 5C 04</c>): <c>jb</c> (CF=1)
    /// → locret_81C1 0x62F1; falls into sub_81A6 @0x62D6.</remarks>
    public Action IsRecordVisibleGate_1000_62C9_0162C9(int gotoAddress) {
        if (UInt8[DS, 0x46EB] < 1) {                       // cmp byte ds:46EBh,1 ; jb locret_81C1
            CarryFlag = true;
            return NearRet();                               // locret_81C1 0x62F1
        }
        DX = UInt16[DS, (ushort)(SI + 2)];                 // mov dx,[si+2]
        BX = UInt16[DS, (ushort)(SI + 4)];                 // mov bx,[si+4]
        return NearJump(0x62D6);                            // fall into sub_81A6
    }

    /// <summary>
    /// cs1:0x62D6 — <c>sub_81A6</c>. Point-in-clip-rect predicate: runs
    /// <see cref="WorldToScreenScale_1000_B647_1B647"/> (sub_D517 —
    /// NearRet-inline, call-and-discard-safe) then returns CF=0 iff
    /// <c>[0x46E3] &lt;= dx &lt; [0x46E7]</c> and
    /// <c>[0x46E5] &lt;= bx &lt; [0x46E9]</c>, CF=1 otherwise
    /// (<c>locret_81C1</c> 0x62F1 = <c>retn</c>). Fully ported.
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0x62D6
    /// (<c>E8 6E 53 3B 16 E3 46 72 12 3B 16 E7 46 F5 72 0B
    /// 3B 1E E5 46 72 05 3B 1E E9 46 F5 C3</c>): the
    /// <c>cmp/cmc/jb</c> chain — CF of the final <c>cmp bx,[0x46E9]; cmc</c>
    /// is the in/out result.</remarks>
    public Action PointInClipRect_1000_62D6_0162D6(int gotoAddress) {
        WorldToScreenScale_1000_B647_1B647(0);             // call sub_D517 (NearRet-safe)
        bool cf;
        if (DX < UInt16[DS, 0x46E3]) cf = true;            // cmp dx,[0x46E3]; jb (CF=1)
        else if (DX >= UInt16[DS, 0x46E7]) cf = true;      // cmp dx,[0x46E7]; cmc; jb (dx>=hi -> CF=1)
        else if (BX < UInt16[DS, 0x46E5]) cf = true;       // cmp bx,[0x46E5]; jb (CF=1)
        else cf = !(BX < UInt16[DS, 0x46E9]);              // cmp bx,[0x46E9]; cmc -> CF=!(bx<hi)
        CarryFlag = cf;
        return NearRet();                                   // locret_81C1
    }

    /// <summary>
    /// cs1:0x63C7 — <c>sub_8297</c>. Sprite-cell dispatch: saves 6 regs,
    /// picks <c>ax=0x78</c>/<c>0x79</c> by <c>ah==0x10</c>, folds
    /// <c>di</c>/<c>bp</c> into <c>bx</c>/<c>dx</c>, then the §A
    /// <c>call sub_E213</c> (cs1:0xC343 — FarJump, call-and-discard-unsafe)
    /// and restores the 6 regs. The 6-reg save + the index compute are
    /// ported in C#; <c>call sub_E213; pop×6; retn</c> is delegated to the
    /// emulated stream via <see cref="NearJump"/>(0x63E6) (the pushes are
    /// balanced by the emulated pops).
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0x63C7
    /// (<c>53 52 56 57 55 06 80 FC 10 B8 78 00 75 01 40 03 EF 83 E7 03
    /// D1 ED D1 ED 83 E5 03 03 DF 03 D5 E8 5A 5F 07 5D 5F 5E 5A 5B C3</c>):
    /// call sub_E213 @0x63E6, continuation @0x63E9.</remarks>
    public Action SpriteCellDispatch_1000_63C7_0163C7(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BX;       // push bx
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DX;       // push dx
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;       // push si
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DI;       // push di
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BP;       // push bp
        SP = (ushort)(SP - 2); UInt16[SS, SP] = ES;       // push es
        ushort ax = 0x0078;                                 // mov ax,78h
        if (AH == 0x10) {                                   // cmp ah,10h ; jnz loc_82A6
            ax = 0x0079;                                    // inc ax
        }
        AX = ax;
        BP = (ushort)(BP + DI);                             // add bp,di
        DI = (ushort)(DI & 0x0003);                         // and di,3
        BP = (ushort)(BP >> 1);                             // shr bp,1
        BP = (ushort)(BP >> 1);                             // shr bp,1
        BP = (ushort)(BP & 0x0003);                         // and bp,3
        BX = (ushort)(BX + DI);                             // add bx,di
        DX = (ushort)(DX + BP);                             // add dx,bp
        return NearJump(0x63E6);                            // call sub_E213; pop×6; retn (emulated)
    }

    /// <summary>
    /// cs1:0x7C8F — <c>sub_9B5F</c> entry. Models <c>push si;
    /// call sub_5F4E</c> (cs1:0x407E — has a NearJump path so
    /// call-and-discard is unsafe) with the call-continuation idiom: push
    /// <c>si</c>, push the post-call IP 0x7C93, <see cref="NearJump"/> to
    /// sub_5F4E; the pure-compute tail is
    /// <see cref="ScaledDistanceClampCont_1000_7C93_017C93"/>.
    /// </summary>
    /// <remarks>Asm: <c>7C8F: 56 push si | 7C90: E8 EB C3 call sub_5F4E
    /// (0x407E)</c> (byte-verified cs1.bin@0x7C8F); continuation 0x7C93.</remarks>
    public Action ScaledDistanceClamp_1000_7C8F_017C8F(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;       // push si
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0x7C93;   // call sub_5F4E (cont)
        return NearJump(0x407E);
    }

    /// <summary>
    /// cs1:0x7C93 — continuation of <see cref="ScaledDistanceClamp_1000_7C8F_017C8F"/>
    /// after <c>sub_5F4E</c>. Pure-compute: <c>bp = |bx*2|</c> →
    /// <c>[bp+0x4880]</c> scale; <c>ax = |[si+2]-dx| / bp</c> (16-bit
    /// <c>div bp</c>); clamps <c>ax</c> to <c>|bx-[si+4]|</c>
    /// (<c>min</c>); returns (<c>locret_9B8A</c> 0x7CBA = <c>retn</c>).
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0x7C93
    /// (<c>5E 8B EB D1 E5 79 02 F7 DD 8B AE 80 48 8B 44 02 2B C2 79 02
    /// F7 D8 33 D2 F7 F5 2B 5C 04 79 02 F7 DB 3B C3 73 02 8B C3 C3</c>):
    /// <c>jns</c> = skip <c>neg</c> when non-negative (signed abs).</remarks>
    public Action ScaledDistanceClampCont_1000_7C93_017C93(int gotoAddress) {
        SI = UInt16[SS, SP]; SP = (ushort)(SP + 2);        // pop si
        short bp = (short)(BX << 1);                        // mov bp,bx ; shl bp,1
        if (bp < 0) bp = (short)-bp;                        // jns loc_9B6C ; neg bp
        BP = UInt16[DS, (ushort)((ushort)bp + 0x4880)];     // mov bp,[bp+4880h]
        short ax = (short)(UInt16[DS, (ushort)(SI + 2)] - DX); // mov ax,[si+2] ; sub ax,dx
        if (ax < 0) ax = (short)-ax;                        // jns loc_9B79 ; neg ax
        DX = 0;                                              // xor dx,dx
        ushort q = (ushort)((ushort)ax / BP);               // div bp
        DX = (ushort)((ushort)ax % BP);
        AX = q;
        short bx = (short)(BX - UInt16[DS, (ushort)(SI + 4)]); // sub bx,[si+4]
        if (bx < 0) bx = (short)-bx;                         // jns loc_9B84 ; neg bx
        BX = (ushort)bx;
        if (AX < (ushort)bx) {                               // cmp ax,bx ; jnb locret_9B8A
            AX = (ushort)bx;                                 // mov ax,bx
        }
        return NearRet();                                    // locret_9B8A
    }

    /// <summary>
    /// Registers the Phase-27 verb-13 / verb-8 helper quick-win overrides.
    /// </summary>
    public void DefineVerbHelpersCodeOverrides() {
        DefineFunction(cs1, 0x5B55, StoreDiPairTo197C_1000_5B55_015B55);
        DefineFunction(cs1, 0xC21B, BlitRunListLoop_1000_C21B_01C21B);
        DefineFunction(cs1, 0xA186, VerbB8MenuYPos_1000_A186_01A186);
    }

    /// <summary>
    /// cs1:0x5B55 — <c>sub_7A25</c> (verb-13 helper). <c>dx=[di+2];
    /// bx=[di+4]; jmp loc_7A30</c>. The two loads are ported in C#; the
    /// shared <c>loc_7A30</c> tail (cs1:0x5B60: <c>mov [0x197E],bx;
    /// mov [0x197C],dx; retn</c>) is delegated to the emulated stream via
    /// <see cref="NearJump"/> (it is a shared <c>loc_</c> target).
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0x5B55
    /// (<c>8B 55 02 8B 5D 04 EB 03</c>): <c>jmp +3</c> → loc_7A30 0x5B60.</remarks>
    public Action StoreDiPairTo197C_1000_5B55_015B55(int gotoAddress) {
        DX = UInt16[DS, (ushort)(DI + 2)];   // mov dx,[di+2]
        BX = UInt16[DS, (ushort)(DI + 4)];   // mov bx,[di+4]
        return NearJump(0x5B60);             // jmp loc_7A30 (emulated)
    }

    /// <summary>
    /// cs1:0xC21B — <c>sub_E0EB</c> (verb-13 helper). Loop over a
    /// <c>(w1,w2,w3)</c>-triple list at <c>ds:si</c> terminated by
    /// <c>0xFFFF</c> (<c>locret_E13A</c> 0xC26A = <c>retn</c>); per entry
    /// sets <c>ax=w1, dx=w2, bx=w3</c> and runs <c>sub_E0FF</c>. One C#
    /// invocation == one iteration; the <c>call sub_E0FF</c> (cs1:0xC22F)
    /// uses the call-continuation idiom (push raw cont IP 0xC22C =
    /// <c>pop si; jmp sub_E0EB</c> → re-enters this override) so it is
    /// faithful regardless of sub_E0FF's return shape.
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0xC21B
    /// (<c>AD 3D FF FF 74 49 8B D8 AD 8B D0 AD 93 56 E8 03 00 5E EB EC</c>):
    /// call sub_E0FF @0xC229 → 0xC22F, continuation @0xC22C.</remarks>
    public Action BlitRunListLoop_1000_C21B_01C21B(int gotoAddress) {
        ushort w = UInt16[DS, SI]; SI = (ushort)(SI + 2);   // lodsw
        if (w == 0xFFFF) {                                  // cmp ax,0FFFFh ; jz locret_E13A
            AX = w;
            return NearRet();                                // 0xC26A = retn
        }
        BX = w;                                              // mov bx,ax
        DX = UInt16[DS, SI]; SI = (ushort)(SI + 2);          // lodsw ; mov dx,ax
        ushort w3 = UInt16[DS, SI]; SI = (ushort)(SI + 2);   // lodsw
        AX = BX;                                             // xchg ax,bx -> ax=w1
        BX = w3;                                              //              bx=w3
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;          // push si
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0xC22C;      // call sub_E0FF (cont = raw pop si;jmp sub_E0EB)
        return NearJump(0xC22F);
    }

    /// <summary>
    /// cs1:0xA186 — verb-8 scene sub-handler (task #12). Computes the
    /// menu-row Y position into <c>[0x00D5]</c>: if <c>[0x000A]&amp;2</c> it
    /// kicks counter 0x28 (<see cref="UpdateCounter0029AndTailKick_1000_6F78_016F78"/>
    /// — NearRet-inline, call-and-discard-safe) and uses ax=0xFFCE; else it
    /// reads <c>[0x1176]</c> (special-casing ==1 with a second kick of
    /// counter 0x0A), adds 0x14, stores it back, and for values &lt; 0x64
    /// derives <c>bl = 0x80 - ax/6</c> (8-bit <c>div bl</c>). Fully ported.
    /// </summary>
    /// <remarks>
    /// Asm (0xA186..0xA1C3 ret) byte-verified vs cs1.bin@0xA186
    /// (<c>F6 06 0A 00 02 74 0A B0 28 E8 E6 CD B8 CE FF EB 13 A1 76 11
    /// 3D 01 00 75 0B 05 0A 00 B0 0A E8 D1 CD B8 0A 00 05 14 00 A3 76 11
    /// 32 DB 3D 64 00 73 08 B3 06 F6 F3 B3 80 2A D8 88 1E D5 00 C3</c>).
    /// </remarks>
    public Action VerbB8MenuYPos_1000_A186_01A186(int gotoAddress) {
        ushort ax;
        if ((UInt8[DS, 0x000A] & 0x02) != 0) {              // test byte [0xa],2 ; jz A197
            AL = 0x28;                                       // mov al,0x28
            UpdateCounter0029AndTailKick_1000_6F78_016F78(0);// call 0x6F78
            ax = 0xFFCE;                                      // mov ax,0FFCEh ; jmp A1AA
        } else {
            ax = UInt16[DS, 0x1176];                          // mov ax,[0x1176]
            if (ax == 1) {                                    // cmp ax,1 ; jnz A1AA
                ax = (ushort)(ax + 0x000A);                   // add ax,0Ah
                AX = ax; AL = 0x0A;                            // mov al,0Ah
                UpdateCounter0029AndTailKick_1000_6F78_016F78(0); // call 0x6F78
                ax = 0x000A;                                  // mov ax,0Ah
            }
        }
        ax = (ushort)(ax + 0x0014);                          // A1AA: add ax,14h
        AX = ax;
        UInt16[DS, 0x1176] = ax;                              // mov [0x1176],ax
        byte bl;
        if (ax < 0x0064) {                                    // xor bl,bl ; cmp ax,64h ; jnc A1BF
            byte al = (byte)(ax / 6);                          // mov bl,6 ; div bl
            byte ah = (byte)(ax % 6);
            AX = (ushort)((ah << 8) | al);
            bl = (byte)(0x80 - al);                            // mov bl,80h ; sub bl,al
        } else {
            bl = 0x00;
        }
        BL = bl;
        UInt8[DS, 0x00D5] = bl;                                // A1BF: mov [0xd5],bl
        return NearRet();                                      // retn
    }

    /// <summary>
    /// cs1:0x8E16 — <c>sub_ACE6</c>. Lays out wrapped text lines into the
    /// <c>0xA9D2</c> table. Head (<c>es=ds; [0x478C]=0; di=0xA9D2; dh=0;
    /// bx=[0x478F]; dl=0</c>) ported in C#; the <c>loc_ACF8</c> word-wrap
    /// loop — which calls the continuation-unsafe <c>sub_AD6E</c>
    /// (cs1:0x8E9E, NearJump path) and <c>sub_ADA3</c> — is delegated to
    /// the emulated stream via <see cref="NearJump"/>(0x8E28).
    /// </summary>
    /// <remarks>Asm head byte-verified vs cs1.bin@0x8E16
    /// (<c>1E 07 C6 06 8C 47 00 BF D2 A9 32 F6 8B 1E 8F 47 32 D2</c>);
    /// loc_ACF8 @0x8E28.</remarks>
    public Action LayoutTextLines_1000_8E16_018E16(int gotoAddress) {
        ES = DS;                                       // push ds ; pop es
        UInt8[DS, 0x478C] = 0;                          // mov byte ds:478Ch,0
        DI = 0xA9D2;                                    // mov di,0A9D2h
        DH = 0;                                          // xor dh,dh
        BX = UInt16[DS, 0x478F];                         // mov bx,ds:478Fh
        DL = 0;                                          // xor dl,dl
        return NearJump(0x8E28);                          // loc_ACF8 (word-wrap loop, emulated)
    }

    /// <summary>
    /// cs1:0x79EE — <c>sub_98BE</c> (L108). Records <c>[0x46EF]=si</c> then
    /// models <c>call sub_87E7</c> (cs1:0x6917) and delegates the whole
    /// L108 body (the menu-geometry branch tree + deeper calls) to the
    /// emulated stream: push the post-call IP 0x79F5, <see cref="NearJump"/>
    /// to sub_87E7 — there is no portable compute head and the body is
    /// call/branch-dominated, faithful only when emulated.
    /// </summary>
    /// <remarks>Asm byte-verified vs cs1.bin@0x79EE
    /// (<c>89 36 EF 46 E8 22 EF ...</c>): call sub_87E7 @0x79F2 → 0x6917,
    /// continuation @0x79F5.</remarks>
    public Action MeasureMenuEntry_1000_79EE_0179EE(int gotoAddress) {
        UInt16[DS, 0x46EF] = SI;                         // mov ds:46EFh,si
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0x79F5; // call sub_87E7 (cont = raw body)
        return NearJump(0x6917);
    }

    /// <summary>
    /// cs1:0x8CCD — <c>sub_AB9D</c> (L130). Sets <c>[0x4799]=9</c>,
    /// <c>[0xDBE4]=0xF0</c>, then branches on <c>[0x46EB]</c>/<c>[0x46EF]</c>.
    /// The two stores and the branch decision are ported in C#; all three
    /// targets (loc_ABCB 0x8CFB, loc_ABC5 0x8CF5, and the
    /// <c>call sub_10140/sub_98BE/sub_10153</c> body at 0x8CE6) are
    /// delegated to the emulated stream via <see cref="NearJump"/>
    /// (call/§A-heavy, incl. the continuation-unsafe <c>sub_98BE</c>).
    /// </summary>
    /// <remarks>Asm head byte-verified vs cs1.bin@0x8CCD
    /// (<c>C6 06 99 47 09 C7 06 E4 DB F0 00 80 3E EB 46 00 74 1C
    /// 83 3E EF 46 00 75 0F</c>): <c>jz</c>→0x8CFB, <c>jnz</c>→0x8CF5,
    /// fall→0x8CE6.</remarks>
    public Action BuildMenuLayout_1000_8CCD_018CCD(int gotoAddress) {
        UInt8[DS, 0x4799] = 9;                           // mov byte ds:4799h,9
        UInt16[DS, 0xDBE4] = 0x00F0;                      // mov word ds:0DBE4h,0F0h
        if (UInt8[DS, 0x46EB] == 0) {                    // cmp byte ds:46EBh,0 ; jz loc_ABCB
            return NearJump(0x8CFB);
        }
        if (UInt16[DS, 0x46EF] != 0) {                   // cmp word ds:46EFh,0 ; jnz loc_ABC5
            return NearJump(0x8CF5);
        }
        return NearJump(0x8CE6);                          // call sub_10140 ... (emulated)
    }

    /// <summary>
    /// cs1:0x8B11 — <c>sub_A9E1</c>, the <b>0x8B11-campaign root</b> (L210).
    /// A pure call-orchestrator (<c>push si; call sub_AB5A; pop si;
    /// call sub_AB9D; jb nullsub_9; call sub_ADF8; call sub_ACC0; ...</c>)
    /// with no compute head. Models the first <c>push si; call sub_AB5A</c>
    /// faithfully — push <c>si</c>, push the post-call IP 0x8B15 — then
    /// <see cref="NearJump"/> to <c>sub_AB5A</c> (cs1:0x8C8A); the raw
    /// continuation (pop si; the L210 orchestration body) executes in the
    /// emulated stream where every callee — incl. the C# ports
    /// <c>sub_AB5A</c>/<c>sub_AB9D</c>/<c>sub_ADF8</c> reached via real
    /// emulated calls — dispatches exactly. (Reimplementing the L210
    /// call sequence via per-call continuations would add fragility with
    /// zero fidelity gain — same exact discipline as the other campaign
    /// roots.)
    /// </summary>
    /// <remarks>Asm head byte-verified vs cs1.bin@0x8B11
    /// (<c>56 E8 75 01 5E E8 B4 01 72 F5 E8 0A 04 E8 CF 02 ...</c>):
    /// call sub_AB5A @0x8B12 → 0x8C8A, continuation @0x8B15
    /// (<c>5E pop si</c> then the body).</remarks>
    public Action RenderDialogueBox_1000_8B11_018B11(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;     // push si
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0x8B15; // call sub_AB5A (cont = raw L210 body)
        return NearJump(0x8C8A);
    }

    /// <summary>
    /// cs1:0x8F28 — <c>sub_ADF8</c>. Builds the text-box geometry record at
    /// <c>ds:0x1BE2</c> from the 4-word frame at <c>ss:[bp+0..6]</c>:
    /// emits <c>x,y</c> (and derives the <c>[0x478D..0x4797]</c> bound
    /// globals), accumulates <c>dx/bx</c>, clamps width to 0x140, and emits
    /// the clamped <c>w,h</c>. This whole coordinate-setup head is ported in
    /// C#; the branch tree from <c>cmp byte [0x46EB],0</c> (cs1:0x8F80) —
    /// which leads to <c>sub_AEA1</c>, <c>loc_AEC5</c>, the §A
    /// <c>call [0x3919]</c>, <c>sub_E007</c>/<c>sub_E240</c> and the
    /// FUNCTION CHUNK @0x8FF5 — is delegated to the emulated stream via
    /// <see cref="NearJump"/>(0x8F80) (call/§A/chunk-heavy, faithful only
    /// emulated).
    /// </summary>
    /// <remarks>
    /// Asm head byte-verified vs cs1.bin@0x8F28
    /// (<c>89 2E 9E 47 BF E2 1B 1E 07 8B 46 00 AB 8B D0 03 06 84 47
    /// A3 91 47 A3 95 47 8B 46 02 AB 8B D8 03 06 88 47 A3 93 47 A3 97 47
    /// 8B 46 04 03 D0 2B 06 84 47 2B 06 86 47 A3 8F 47 8B 46 06 03 D8
    /// 2B 06 88 47 2B 06 8A 47 A3 8D 47 8B C2 3D 40 01 72 03 B8 40 01 AB
    /// 8B C3 AB 80 3E EB 46 00</c>): clamp <c>jb loc_AE4B</c> → 0x8F7B;
    /// delegate point 0x8F80. <c>[bp+n]</c> is SS-relative.
    /// </remarks>
    public Action SetupTextBoxGeometry_1000_8F28_018F28(int gotoAddress) {
        UInt16[DS, 0x479E] = BP;                          // mov ds:479Eh,bp
        DI = 0x1BE2;                                       // mov di,1BE2h
        ES = DS;                                            // push ds ; pop es
        ushort ax = UInt16[SS, (ushort)(BP + 0)];          // mov ax,[bp+0]
        UInt16[ES, DI] = ax; DI = (ushort)(DI + 2);        // stosw
        DX = ax;                                            // mov dx,ax
        ushort t = (ushort)(ax + UInt16[DS, 0x4784]);      // add ax,ds:4784h
        UInt16[DS, 0x4791] = t;                             // mov ds:4791h,ax
        UInt16[DS, 0x4795] = t;                             // mov ds:4795h,ax
        ax = UInt16[SS, (ushort)(BP + 2)];                 // mov ax,[bp+2]
        UInt16[ES, DI] = ax; DI = (ushort)(DI + 2);        // stosw
        BX = ax;                                            // mov bx,ax
        t = (ushort)(ax + UInt16[DS, 0x4788]);             // add ax,ds:4788h
        UInt16[DS, 0x4793] = t;                             // mov ds:4793h,ax
        UInt16[DS, 0x4797] = t;                             // mov ds:4797h,ax
        ax = UInt16[SS, (ushort)(BP + 4)];                 // mov ax,[bp+4]
        DX = (ushort)(DX + ax);                             // add dx,ax
        ax = (ushort)(ax - UInt16[DS, 0x4784]);            // sub ax,ds:4784h
        ax = (ushort)(ax - UInt16[DS, 0x4786]);            // sub ax,ds:4786h
        UInt16[DS, 0x478F] = ax;                            // mov ds:478Fh,ax
        ax = UInt16[SS, (ushort)(BP + 6)];                 // mov ax,[bp+6]
        BX = (ushort)(BX + ax);                             // add bx,ax
        ax = (ushort)(ax - UInt16[DS, 0x4788]);            // sub ax,ds:4788h
        ax = (ushort)(ax - UInt16[DS, 0x478A]);            // sub ax,ds:478Ah
        UInt16[DS, 0x478D] = ax;                            // mov ds:478Dh,ax
        ax = DX;                                            // mov ax,dx
        if (ax >= 0x0140) {                                 // cmp ax,140h ; jb loc_AE4B
            ax = 0x0140;                                    // mov ax,140h
        }
        AX = ax;
        UInt16[ES, DI] = ax; DI = (ushort)(DI + 2);        // loc_AE4B: stosw
        ax = BX;                                            // mov ax,bx
        AX = ax;
        UInt16[ES, DI] = ax; DI = (ushort)(DI + 2);        // stosw
        return NearJump(0x8F80);                            // cmp byte [0x46EB],0 ... (emulated tail)
    }

    /// <summary>
    /// cs1:0x9D6A — <c>sub_BC3A</c> entry: <c>es=ss:[0xDBD8]</c> then falls
    /// into the loop top <c>loc_BC3F</c> (cs1:0x9D6F, the C#
    /// <see cref="EmitGlyphListLoop_1000_9D6F_019D6F"/>).
    /// </summary>
    /// <remarks>Asm: <c>9D6A: 36 8E 06 D8 DB mov es,ss:0DBD8h</c>
    /// (byte-verified cs1.bin@0x9D6A); loc_BC3F @0x9D6F.</remarks>
    public Action EmitGlyphList_1000_9D6A_019D6A(int gotoAddress) {
        ES = UInt16[SS, 0xDBD8];          // mov es,ss:0DBD8h
        return NearJump(0x9D6F);          // -> loc_BC3F
    }

    /// <summary>
    /// cs1:0x9D6F — <c>sub_BC3A</c> loop body (<c>loc_BC3F</c>). One C#
    /// invocation == one iteration: <c>lodsb</c>; byte 0 terminates
    /// (<c>locret_BC63</c> 0x9D93 = <c>retn</c>); a leading <c>1</c> escape
    /// reads an extra byte into <c>ah</c>; computes
    /// <c>si = ss:[0x47CC] + ds:[(ax-2)*2 + si]</c> then runs
    /// <c>sub_BC64</c> for that run and loops. The <c>call sub_BC64</c>
    /// (cs1:0x9D94, a §A-loop port) uses the call-continuation idiom: push
    /// the raw continuation IP 0x9D90 (raw <c>pop si; jmp loc_BC3F</c> →
    /// re-enters this override) then <see cref="NearJump"/> to sub_BC64.
    /// </summary>
    /// <remarks>
    /// Asm byte-verified vs cs1.bin@0x9D6F
    /// (<c>AC 32 E4 0A C0 74 1D 3C 01 75 03 8A E0 AC 56 2D 02 00 D1 E0
    /// 8B E8 36 8B 36 CC 47 3E 03 32 E8 04 00 5E EB DC C3</c>):
    /// call sub_BC64 @0x9D8D, continuation @0x9D90.
    /// </remarks>
    public Action EmitGlyphListLoop_1000_9D6F_019D6F(int gotoAddress) {
        byte b = UInt8[DS, SI]; SI = (ushort)(SI + 1);   // lodsb
        AX = b;                                          // xor ah,ah
        if (b == 0) {                                    // or al,al ; jz locret_BC63
            return NearRet();
        }
        if (b == 1) {                                    // cmp al,1 ; jnz loc_BC4D
            AH = 1;                                       // mov ah,al
            byte b2 = UInt8[DS, SI]; SI = (ushort)(SI + 1);   // lodsb
            AL = b2;                                      //   ax = 0x0100|b2
        }
        // loc_BC4D:
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;      // push si
        AX = (ushort)(AX - 2);                            // sub ax,2
        AX = (ushort)(AX << 1);                           // shl ax,1
        BP = AX;                                          // mov bp,ax
        SI = UInt16[SS, 0x47CC];                          // mov si,ss:47CCh
        SI = (ushort)(SI + UInt16[DS, (ushort)(BP + SI)]); // add si,ds:[bp+si]
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0x9D90;  // call sub_BC64 (cont = raw pop si;jmp loc_BC3F)
        return NearJump(0x9D94);
    }

    /// <summary>
    /// cs1:0x8ED3 — <c>sub_ADA3</c>. Measures the rendered width of the
    /// text at <c>ds:si</c> into <c>cl</c>, stopping at space / CR / a
    /// signed-negative byte (and rewinding <c>si</c> by 1). Per char it
    /// either adds the width-table entry (<c>xlat</c> on <c>[0x47A0]</c>) or,
    /// for the special <c>[0x2518]==0xD0FF &amp;&amp; si==0xA6B1</c> case,
    /// the value from <see cref="LookupKeyInCsTable_1000_D0E3_01D0E3"/>
    /// (<c>sub_EFB3</c> — NearRet-inline, call-and-discard-safe, sets CF/AL);
    /// control bytes 6/8 switch the active width table
    /// (<c>[0x47A0]=0xCF6C/0xCEEC</c>). Fully ported.
    /// </summary>
    /// <remarks>
    /// Asm (L54) byte-verified vs cs1.bin@0x8ED3
    /// (<c>33 C9 53 8B 1E A0 47 AC 3C 20 74 46 3C 0D 74 42 0A C0 74 06
    /// 3C 09 72 1E 78 38 81 3E 18 25 FF D0 75 0F 81 FE B1 A6 75 09
    /// E8 E5 41 72 04 02 C8 EB D6 D7 02 C8 ...</c>). <c>bx</c> is the
    /// width-table base loaded once from <c>[0x47A0]</c> at entry (the
    /// later <c>[0x47A0]</c> writes do not reload it). <c>cmp al,9; jb
    /// loc_ADD9; js loc_ADF5</c> ⇒ <c>al&lt;9</c>→control,
    /// <c>al≥0x80</c>→stop, <c>9≤al&lt;0x80</c>→measure.
    /// </remarks>
    public Action MeasureTextWidth_1000_8ED3_018ED3(int gotoAddress) {
        CX = 0;                                           // xor cx,cx
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BX;      // push bx
        ushort tbl = UInt16[DS, 0x47A0];                  // mov bx,ds:47A0h
        BX = tbl;
        while (true) {                                    // loc_ADAA
            byte al = UInt8[DS, SI]; SI = (ushort)(SI + 1);   // lodsb
            AL = al;
            if (al == 0x20 || al == 0x0D) {               // jz loc_ADF5 (space/CR)
                break;
            }
            if (al != 0 && al < 0x09) {                   // or al,al/jz loc_ADBD ; cmp al,9/jb loc_ADD9
                // loc_ADD9: control bytes (al in 1..8)
                if (al == 0x06) {                         // jz loc_ADED
                    UInt16[DS, 0x47A0] = 0xCF6C;
                } else if (al == 0x08) {                  // cmp al,8 ; jnz loc_ADAA
                    UInt16[DS, 0x47A0] = 0xCEEC;
                }
                continue;                                  // jmp loc_ADAA
            }
            if (al != 0 && (al & 0x80) != 0) {            // js loc_ADF5 (al >= 0x80)
                break;
            }
            // loc_ADBD / loc_ADD4  (al == 0, or 9 <= al < 0x80)
            byte add;
            if (UInt16[DS, 0x2518] == 0xD0FF && SI == 0xA6B1) {
                LookupKeyInCsTable_1000_D0E3_01D0E3(0);   // call sub_EFB3 (sets CF/AL)
                if (CarryFlag) {                           // jb loc_ADD4
                    add = UInt8[DS, (ushort)(BX + AL)];   // loc_ADD4: xlat
                } else {
                    add = AL;                              // add cl,al ; loop
                }
            } else {
                add = UInt8[DS, (ushort)(BX + AL)];        // loc_ADD4: xlat
            }
            CL = (byte)(CL + add);                          // add cl,al
        }
        // loc_ADF5:
        SI = (ushort)(SI - 1);                              // dec si
        BX = UInt16[SS, SP]; SP = (ushort)(SP + 2);        // pop bx
        return NearRet();                                   // retn
    }

    /// <summary>
    /// cs1:0x7B0F — <c>sub_99DF</c>: <c>[0x46D8]=0; push si; xchg si,di;
    /// call sub_DFB8; pop si</c> then falls through into the next proc
    /// (cs1:0x7B1B). Zero compute beyond the head; modelled with the
    /// near-call-continuation idiom because <c>sub_DFB8</c> (cs1:0xC0E8) is
    /// a §A FarJump-returning port (call-and-discard-unsafe): set
    /// <c>[0x46D8]=0</c>, push <c>si</c>, swap <c>si/di</c>, push the raw
    /// continuation IP 0x7B1A (raw <c>pop si</c> → fall into the emulated
    /// next proc), then <see cref="NearJump"/> to sub_DFB8.
    /// </summary>
    /// <remarks>Asm 11 B byte-verified vs cs1.bin@0x7B0F
    /// (<c>C6 06 D8 46 00 56 87 F7 E8 CE 45 5E</c>): call sub_DFB8 @0x7B17
    /// → 0xC0E8; continuation 0x7B1A (<c>5E pop si</c>); no retn.</remarks>
    public Action ResetThenSwapSiDiCallDfb8_1000_7B0F_017B0F(int gotoAddress) {
        UInt8[DS, 0x46D8] = 0;                          // mov byte ds:46D8h,0
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;     // push si
        ushort t = SI; SI = DI; DI = t;                 // xchg si,di
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0x7B1A; // push continuation (raw pop si; fall-through)
        return NearJump(0xC0E8);                          // call sub_DFB8
    }

    /// <summary>
    /// cs1:0x9D94 — <c>sub_BC64</c>. Per-glyph emit loop: reads a
    /// <c>(count,dx,bx)</c> triple from <c>ds:si</c>, applies the
    /// SS-relative world→screen transform
    /// (<c>+[ss:1BF0/1BF2] -[ss:46D2/46D4] +[ss:47D4/47D6]</c>), reloads
    /// <c>ds:si</c> from the vertex table (<c>lds si,ss:[0xDBB0]</c> indexed
    /// by <c>bp</c>), fetches <c>di</c>/<c>cx</c>, then a §A SS-far
    /// <c>call dword ptr ss:[0x38CD]</c> and loops. One C# invocation == one
    /// iteration; the §A call uses the near-/far-call-continuation idiom
    /// (push CS+raw cont IP 0x9DDF — which is <c>pop ds; pop si;
    /// jmp sub_BC64</c> → re-enters this override), terminator byte 0 →
    /// <see cref="NearRet"/> (locret_BC63 0x9D93 = <c>retn</c>).
    /// </summary>
    /// <remarks>
    /// Asm byte-verified vs cs1.bin@0x9D94
    /// (<c>AC 25 FF 00 74 F9 32 E4 8B E8 AC 8B D0 AC 8B D8 36 03 16 F0 1B
    /// 36 03 1E F2 1B 36 2B 16 D2 46 36 2B 1E D4 46 36 03 16 D4 47
    /// 36 03 1E D6 47 56 1E 4D 36 C5 36 B0 DB D1 E5 3E 03 32 AD 8B F8 AD
    /// 32 E4 8B C8 BD D4 47 36 FF 1E CD 38 1F 5E EB B1</c>): far call
    /// @0x9DDA, continuation @0x9DDF.
    /// </remarks>
    public Action EmitGlyphRunLoop_1000_9D94_019D94(int gotoAddress) {
        byte count = UInt8[DS, SI]; SI = (ushort)(SI + 1);   // lodsb
        AX = count;                                          // and ax,0FFh
        if (count == 0) {                                    // jz locret_BC63
            return NearRet();
        }
        BP = count;                                          // xor ah,ah ; mov bp,ax
        byte b1 = UInt8[DS, SI]; SI = (ushort)(SI + 1);      // lodsb
        DX = b1;                                             // mov dx,ax (ah=0)
        byte b2 = UInt8[DS, SI]; SI = (ushort)(SI + 1);      // lodsb
        BX = b2;                                             // mov bx,ax
        DX = (ushort)(DX + UInt16[SS, 0x1BF0]);              // add dx,ss:[0x1BF0]
        BX = (ushort)(BX + UInt16[SS, 0x1BF2]);              // add bx,ss:[0x1BF2]
        DX = (ushort)(DX - UInt16[SS, 0x46D2]);              // sub dx,ss:[0x46D2]
        BX = (ushort)(BX - UInt16[SS, 0x46D4]);              // sub bx,ss:[0x46D4]
        DX = (ushort)(DX + UInt16[SS, 0x47D4]);              // add dx,ss:[0x47D4]
        BX = (ushort)(BX + UInt16[SS, 0x47D6]);              // add bx,ss:[0x47D6]
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;          // push si
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DS;          // push ds
        BP = (ushort)(BP - 1);                                // dec bp
        ushort ldsSi = UInt16[SS, 0xDBB0];                   // lds si,ss:[0xDBB0]
        DS = UInt16[SS, 0xDBB2];
        SI = ldsSi;
        BP = (ushort)(BP << 1);                               // shl bp,1
        SI = (ushort)(SI + UInt16[DS, (ushort)(BP + SI)]);   // add si,ds:[bp+si]
        ushort w0 = UInt16[DS, SI]; SI = (ushort)(SI + 2);   // lodsw
        AX = w0; DI = w0;                                     // mov di,ax
        ushort w1 = UInt16[DS, SI]; SI = (ushort)(SI + 2);   // lodsw
        AX = (ushort)(w1 & 0x00FF);                           // xor ah,ah
        CX = AX;                                              // mov cx,ax
        BP = 0x47D4;                                          // mov bp,47D4h
        ushort off = UInt16[SS, 0x38CD];                      // call dword ptr ss:[0x38CD]
        ushort seg = UInt16[SS, 0x38CF];
        SP = (ushort)(SP - 2); UInt16[SS, SP] = cs1;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = 0x9DDF;       // continuation (raw pop ds;pop si;jmp sub_BC64)
        return FarJump(seg, off);
    }

    /// <summary>
    /// cs1:0x8C8A — <c>sub_AB5A</c>. Take-and-clear <c>[0x479E]</c>; if it
    /// was &lt; 2 return (<c>locret_AB9C</c> 0x8CCC = <c>retn</c>); else
    /// <c>si=0x1470</c> and branch on <c>[0x28E7]</c>: zero → loc_AB85
    /// (cs1:0x8CB5), non-zero → the <c>[0x28E7]</c> path
    /// (cs1:0x8C9F: <c>bp=0x1BE2; si=0x4C60; es=[0xDBDE];
    /// call dword ptr ds:[0x391D]; ...; loc_AB85</c>). The head decision is
    /// ported in C#; both tails are delegated to the emulated stream via
    /// <see cref="NearJump"/> because they contain a §A far-indirect
    /// <c>[0x391D]</c> and the continuation-unsafe C# calls
    /// <c>sub_E316</c>/0xC446 (FarJump path), <c>sub_BA7C</c>/0x9BAC
    /// (push-cont+NearJump) and <c>sub_E3AD</c>/0xC4DD (NearJump) — all
    /// faithful only through real emulated calls.
    /// </summary>
    /// <remarks>
    /// Asm head byte-verified vs cs1.bin@0x8C8A
    /// (<c>33 C0 87 06 9E 47 3D 02 00 72 37 BE 70 14 80 3E E7 28 00 74 16</c>):
    /// <c>jb +0x37</c> → locret_AB9C 0x8CCC; <c>jz +0x16</c> → loc_AB85
    /// 0x8CB5; the §A branch begins 0x8C9F.
    /// </remarks>
    public Action FlushPendingTextWrites_1000_8C8A_018C8A(int gotoAddress) {
        ushort old = UInt16[DS, 0x479E];                // xor ax,ax ; xchg ax,ds:479Eh
        UInt16[DS, 0x479E] = 0;
        AX = old;
        if (old < 2) {                                  // cmp ax,2 ; jb locret_AB9C
            return NearRet();
        }
        SI = 0x1470;                                    // mov si,1470h
        if (UInt8[DS, 0x28E7] == 0) {                   // cmp byte ds:28E7h,0 ; jz loc_AB85
            return NearJump(0x8CB5);                     // loc_AB85 (emulated)
        }
        return NearJump(0x8C9F);                          // [0x28E7] §A branch (emulated)
    }

    /// <summary>
    /// cs1:0xD0E3 — <c>sub_EFB3</c>. Scans the 9-byte CS-resident key table
    /// at <c>cs:0xD0D1</c> for <c>al</c> (<c>repne scasb</c>, single fixed
    /// pass — no loop-back, so no ZF-persistence subtlety) and, on a hit,
    /// returns <c>al = cs:[di+8]</c> (the parallel value table) and
    /// <c>ah = 0x0D - cl</c>. CF = 1 on miss (the <c>stc</c>); on hit CF is
    /// the borrow of <c>sub ah,cl</c>. Fully ported (self-contained leaf,
    /// no calls).
    /// </summary>
    /// <remarks>
    /// Asm byte-verified vs cs1.bin@0xD0E3
    /// (<c>51 57 06 0E 07 BF D1 D0 B9 09 00 F2 AE 07 F9 75 08
    /// 2E 8A 45 08 B4 0D 2A E1 5F 59 C3</c>):
    /// <code>
    /// D0E3: 51 57 06        push cx ; push di ; push es
    /// D0E6: 0E 07           push cs ; pop es     (es = cs = cs1)
    /// D0E8: BF D1 D0        mov di,0D0D1h
    /// D0EB: B9 09 00        mov cx,9
    /// D0EE: F2 AE           repne scasb
    /// D0F0: 07              pop es
    /// D0F1: F9              stc
    /// D0F2: 75 08           jnz loc_EFCC (0xD0FC)
    /// D0F4: 2E 8A 45 08     mov al,cs:[di+8]
    /// D0F8: B4 0D           mov ah,0Dh
    /// D0FA: 2A E1           sub ah,cl
    /// D0FC: 5F 59 C3        pop di ; pop cx ; retn   (loc_EFCC)
    /// </code>
    /// </remarks>
    public Action LookupKeyInCsTable_1000_D0E3_01D0E3(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = CX;     // push cx
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DI;     // push di
        SP = (ushort)(SP - 2); UInt16[SS, SP] = ES;     // push es
        ES = CS;                                         // push cs ; pop es  (= cs1)
        DI = 0xD0D1;                                      // mov di,0D0D1h
        CX = 9;                                           // mov cx,9
        byte al = AL;
        bool found = false;                               // repne scasb
        while (CX != 0) {
            byte t = UInt8[ES, DI];
            DI = (ushort)(DI + 1);
            CX = (ushort)(CX - 1);
            if (al == t) { found = true; break; }
        }
        ES = UInt16[SS, SP]; SP = (ushort)(SP + 2);      // pop es
        CarryFlag = true;                                 // stc
        if (found) {                                      // jnz loc_EFCC (taken when NOT found)
            AL = UInt8[cs1, (ushort)(DI + 8)];           // mov al,cs:[di+8]
            byte cl = CL;
            AH = (byte)(0x0D - cl);                       // mov ah,0Dh ; sub ah,cl
            CarryFlag = 0x0D < cl;                         //   CF = borrow
        }
        DI = UInt16[SS, SP]; SP = (ushort)(SP + 2);      // loc_EFCC: pop di
        CX = UInt16[SS, SP]; SP = (ushort)(SP + 2);      // pop cx
        return NearRet();                                 // retn
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
