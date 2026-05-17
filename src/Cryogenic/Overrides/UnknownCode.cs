namespace Cryogenic.Overrides;

using System;

/// <summary>
/// Partial class containing overrides for functions whose exact purpose is not yet fully understood.
/// </summary>
/// <remarks>
/// <para>
/// This file contains overrides for various game functions that have been reverse-engineered
/// but whose complete purpose or significance is still being researched. Function names
/// describe observable behavior or affected memory locations rather than semantic purpose.
/// </para>
/// <para>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly.
/// </para>
/// </remarks>
public partial class Overrides {

    /// <summary>
    /// Registers function overrides for routines of uncertain or partially understood purpose.
    /// </summary>
    public void DefineUnknownCodeOverrides() {
        // Clean standalone leaves harvested from the full Ghidra symbol table
        // (2026-05-15 full-symtab leaf scan; each hand-decoded). Plans/09 §E note.
        DefineFunction(cs1, 0x1DD3, NoOp_1000_1DD3_11DD3);
        DefineFunction(cs1, 0x4AB8, SetFlag4727ToFF_1000_4AB8_14AB8);
        DefineFunction(cs1, 0x50BE, Clear11CB_1000_50BE_150BE);
        DefineFunction(cs1, 0xA1C4, Set47A5ToFF_1000_A1C4_1A1C4);
        DefineFunction(cs1, 0xA5AA, Clear28BE_1000_A5AA_1A5AA);
        DefineFunction(cs1, 0xA1E2, Cmp47A5WithFF_1000_A1E2_1A1E2);
        DefineFunction(cs1, 0xA45C, AddDx2886AddBx2888_1000_A45C_1A45C);
        DefineFunction(cs1, 0x693B, LoadNibbleShr2FromSi3_1000_693B_1693B);
        DefineFunction(cs1, 0x3310, Shr4ThenAdd00D1_1000_3310_13310);
        DefineFunction(cs1, 0x127C, CheckAl4AndByte2ARange_1000_127C_1127C);
        DefineFunction(cs1, 0xD03C, SkipNonDigitsThenDigitsEsSi_1000_D03C_1D03C);
        DefineFunction(cs1, 0xCE01, InitDBE8Region_1000_CE01_1CE01);
        DefineFunction(cs1, 0xB683, AbsDiffClampBpDx_1000_B683_1B683);
        DefineFunction(cs1, 0x0CF2, BitPackTransformAxBxDx_1000_0CF2_10CF2);
        // Thunk leaves: (optional reg setup) + near jmp. Faithful as reg-set + NearJump;
        // the tail-jmp means the target's ret returns to the thunk's caller.
        DefineFunction(cs1, 0x02E0, Thunk_Jmp0A44_1000_02E0_102E0);
        DefineFunction(cs1, 0x02F8, Thunk_Jmp07EE_1000_02F8_102F8);
        DefineFunction(cs1, 0x02FB, Thunk_Jmp09AD_1000_02FB_102FB);
        DefineFunction(cs1, 0x02FE, Thunk_Jmp076A_1000_02FE_102FE);
        DefineFunction(cs1, 0x1DD4, Thunk_Jmp20A4_1000_1DD4_11DD4);
        DefineFunction(cs1, 0x1DD7, Thunk_Jmp1F64_1000_1DD7_11DD7);
        DefineFunction(cs1, 0x0737, Thunk_SetAl56_JmpC2F2_1000_0737_10737);
        DefineFunction(cs1, 0x0788, Thunk_SetAl07_Jmp099D_1000_0788_10788);
        DefineFunction(cs1, 0x0820, Thunk_SetAx2E_Jmp3978_1000_0820_10820);
        DefineFunction(cs1, 0x0A3E, Thunk_SetSi0A16_JmpDA5F_1000_0A3E_10A3E);
        DefineFunction(cs1, 0x39E6, Thunk_SetSiC0B6_JmpDA5F_1000_39E6_139E6);
        DefineFunction(cs1, 0x4D00, Thunk_SetSi4BB9_JmpDA5F_1000_4D00_14D00);
        DefineFunction(cs1, 0x40C3, Thunk_SetBp40C9_Jmp36EE_1000_40C3_140C3);
        DefineFunction(cs1, 0x920F, Thunk_AddAx2_JmpC13E_1000_920F_1920F);
        DefineFunction(cs1, 0x49EA, FillCsRing0800_1000_49EA_149EA);
        DefineFunction(cs1, 0x4A00, AppendCsRingDxBx_1000_4A00_14A00);
        DefineFunction(cs1, 0x6231, ClassifyByteDi8_1000_6231_16231);
        DefineFunction(cs1, 0x456C, IndexTransform456C_1000_456C_1456C);
        DefineFunction(cs1, 0x3EFE, TableLookupDhDl_1000_3EFE_13EFE);
        DefineFunction(cs1, 0x996C, Skip32StringsIf47D0_1000_996C_1996C);
        DefineFunction(cs1, 0x3A95, SelectDxBxByByte0005_1000_3A95_13A95);
        DefineFunction(cs1, 0xD6FE, PointInRectDxBx_1000_D6FE_1D6FE);
        DefineFunction(cs1, 0x5E42, ClassifyByteSi8WithBase_1000_5E42_15E42);
        DefineFunction(cs1, 0x409A, FindRecordByDiInTableE4_1000_409A_1409A);
        DefineFunction(cs1, 0x02DE, Thunk_ClrCx_Jmp0A44_1000_02DE_102DE);
        DefineFunction(cs1, 0x3950, Thunk_Clr46D7SetSi3916_JmpDA5F_1000_3950_13950);
        DefineFunction(cs1, 0x3901, Thunk_SetSi3916Bp10_JmpDA25_1000_3901_13901);
        DefineFunction(cs1, 0xA44C, Thunk_LoadAdd28E7_JmpA435_1000_A44C_1A44C);
        DefineFunction(cs1, 0xE3CC, LcgPrngD826_1000_E3CC_1E3CC);
        DefineFunction(cs1, 0x2AAF, TableSearchByAlDi1190_1000_2AAF_12AAF);
        DefineFunction(cs1, 0x5B93, MemCopy8Bytes46E3ToD834_1000_5B93_15B93);
        DefineFunction(cs1, 0x0CEA, BitPackOrCall0D04_1000_0CEA_10CEA);
        DefineFunction(cs1, 0x41C5, Reset4726And21FDMaybe1F11_1000_41C5_141C5);
        DefineFunction(cs1, 0x3A73, LoopTailAddDx46Bx0A_1000_3A73_13A73);
        DefineFunction(cs1, 0xDA25, ListAppendRecordDC6A_1000_DA25_1DA25);
        DefineFunction(cs1, 0xDA5F, ListRemoveRecordBySi_1000_DA5F_1DA5F);
        DefineFunction(cs1, 0x407E, LoadDxBxFrom0004Or114E_1000_407E_1407E);
        DefineFunction(cs1, 0xD792, Thunk_SetSi1C66_FallD795_1000_D792_1D792);
        DefineFunction(cs1, 0xD795, CopyBlocksTo1AEEThenJmpD1F2_1000_D795_1D795);
        DefineFunction(cs1, 0xCC0C, HnmAdvanceDC0CCursor_1000_CC0C_1CC0C);
        DefineFunction(cs1, 0xCC2B, HnmVisibilityPredicate_1000_CC2B_1CC2B);
        DefineFunction(cs1, 0xCC4E, HnmRingAdvanceFar_1000_CC4E_1CC4E);
        DefineFunction(cs1, 0x3DF4, HashInsertOpenAddr_1000_3DF4_13DF4);
        DefineFunction(cs1, 0x3D83, DoWeirdStackBuffer_1000_3D83_13D83);
        DefineFunction(cs1, 0x01E0, StoreTripletAndPackFlag_1000_01E0_101E0);
        DefineFunction(cs1, 0xB647, WorldToScreenScale_1000_B647_1B647);
        DefineFunction(cs1, 0xD694, SelectDiByWord2582_1000_D694_1D694);
        DefineFunction(cs1, 0xC827, ScanNonZeroWord_1000_C827_1C827);
        DefineFunction(cs1, 0x9A7B, ComputeIndexThenJmpE3B7_1000_9A7B_19A7B);
        DefineFunction(cs1, 0x08F0, InitVarsThenJmp2D74_1000_08F0_108F0);
        DefineFunction(cs1, 0xF3A7, TwoLevelKeyedSearch_1000_F3A7_1F3A7);
        DefineFunction(cs1, 0xB941, SetupBp204AThenJmpD338_1000_B941_1B941);
        DefineFunction(cs1, 0x945B, DispatchByWord479E_1000_945B_1945B);
        DefineFunction(cs1, 0xAD43, ClearDBCBMaybeJmpAD95_1000_AD43_1AD43);
        DefineFunction(cs1, 0x40C9, StoreDxBxOrJmp40D4_1000_40C9_140C9);
        DefineFunction(cs1, 0x4415, TestClear46EBOrJmp4420_1000_4415_14415);
        DefineFunction(cs1, 0x3120, TestSi0FBit40OrJmp30CA_1000_3120_13120);
        DefineFunction(cs1, 0x1860, GuardByte11C9OrJmp1868_1000_1860_11860);
        DefineFunction(cs1, 0xAED6, GuardByte11C9OrJmpAEDE_1000_AED6_1AED6);
        DefineFunction(cs1, 0x6B34, Bump46F6Dispatch_1000_6B34_16B34);
        DefineFunction(cs1, 0x5198, DecodePackedDelta_1000_5198_15198);
        DefineFunction(cs1, 0x2EFB, Prologue2EFBDispatch_1000_2EFB_12EFB);
        DefineFunction(cs1, 0x2FFB, SelectSiThenJmpD72B_1000_2FFB_12FFB);
        DefineFunction(cs1, 0x0F08, TestWord0010Bit80Dispatch_1000_0F08_10F08);
        DefineFunction(cs1, 0x329D, GuardSi3OrJmp32AA_1000_329D_1329D);
        DefineFunction(cs1, 0x79DE, TestClear46FAOrJmp_1000_79DE_179DE);
        DefineFunction(cs1, 0x9985, SpinUntil47CELow3Clear_1000_9985_19985);
        DefineFunction(cs1, 0x3AF9, Guard002BOrJmpC43E_1000_3AF9_13AF9);
        DefineFunction(cs1, 0xD50F, SaveRegsThenDispatchD50F_1000_D50F_1D50F);
        DefineFunction(cs1, 0xB5CF, ClampBxByAhThenAddDx_1000_B5CF_1B5CF);
        DefineFunction(cs1, 0x686E, IndexTransform686E_1000_686E_1686E);
        DefineFunction(cs1, 0x4EC6, ComputeDC02FromState_1000_4EC6_14EC6);
        DefineFunction(cs1, 0xA30B, DecodeOperandEsSi_1000_A30B_1A30B);
        DefineFunction(cs1, 0x34D0, UpdateCounterTable34D0_1000_34D0_134D0);
        DefineFunction(cs1, 0x6EFD, ClampAdjustSi15_1000_6EFD_16EFD);
        DefineFunction(cs1, 0x0F66, NoOp_1000_0F66_10F66);
        DefineFunction(cs1, 0x5B99, MemCopy8BytesDsSIToDsDi_1000_5B99_15B99);
        DefineFunction(cs1, 0x5BA0, MemCopy8BytesFrom1470ToD83C_1000_5BA0_15BA0);
        DefineFunction(cs1, 0x5BA8, MemCopy8Bytes_1000_5BA8_15BA8);
        DefineFunction(cs1, 0xAE2F, CheckPcmEnabled_1000_AE2F_1AE2F);
        DefineFunction(cs1, 0xAEC6, IsUnknownDBC80x100And2943BitmaskNonZero_1000_AEC6_1AEC6);
        DefineFunction(cs1, 0xD443, DispatcherJumpsToBX_1000_D443_01D443);
        DefineFunction(cs1, 0xD454, DispatcherHelperDeterminesWhereToJump_1000_D454_01D454);
        DefineFunction(cs1, 0x4AC4, SetUnknown11CATo0_1000_4AC4_14AC4);
        DefineFunction(cs1, 0x4ACA, SetUnknown11CATo1_1000_4ACA_14ACA);
        DefineFunction(cs1, 0xABCC, IsUnknownDC2BZero_1000_ABCC_1ABCC);
        DefineFunction(cs1, 0xAE28, IsUnknownDBC80x100_1000_AE28_1AE28);
        DefineFunction(cs1, 0xB2BE, SetUnknown2788To0_1000_B2BE_1B2BE);
        DefineFunction(cs1, 0xD917, NoOp_1000_D917_01D917);
        DefineFunction(cs1, 0xDB44, ShlDXAndCXByAX_1000_DB44_01DB44);
        DefineFunction(cs1, 0xE26F, NoOp_1000_E26F_01E26F);
        DefineFunction(cs1, 0xE75B, UnknownStructCreation_1000_E75B_01E75B);
        DefineFunction(cs1, 0xE851, CheckNextFreeMemorySegment39B9_1000_E851_01E851);
        DefineFunction(cs1, 0x3AE9, Fill47F8WithFF_1000_3AE9_013AE9);
        DefineFunction(cs1, 0xB2B9, Inc2788_1000_B2B9_01B2B9);
        DefineFunction(cs1, 0xDE4E, SetCEE8To0_1000_DE4E_01DE4E);
    }

    /// <summary>cs1:0x1DD3 — single <c>C3</c> (ret). No-op entry.</summary>
    public Action NoOp_1000_1DD3_11DD3(int gotoAddress) {
        return NearRet();
    }

    /// <summary>
    /// cs1:0x49EA — clears <c>[0x4728]=0</c>, then word-fills the cs1-resident ring
    /// buffer <c>cs:0xE40C..0xE85C</c> with 0x0800 (two <c>stosw</c> per loop). Clean leaf.
    /// </summary>
    /// <remarks>
    /// <code>
    /// 49EA: C6 06 28 47 00   mov  byte [0x4728], 0
    /// 49EF: 0E 07            push cs / pop es
    /// 49F1: BF 0C E4         mov  di, 0xE40C
    /// 49F4: B8 00 08         mov  ax, 0x0800
    /// 49F7: AB AB            stosw ; stosw
    /// 49F9: 81 FF 5C E8      cmp  di, 0xE85C
    /// 49FD: 72 F8            jc   49F7
    /// 49FF: C3               ret
    /// </code>
    /// </remarks>
    public Action FillCsRing0800_1000_49EA_149EA(int gotoAddress) {
        UInt8[DS, 0x4728] = 0;
        ES = CS;
        DI = 0xE40C;
        AX = 0x0800;
        do {
            UInt16[ES, DI] = 0x0800;
            DI = (ushort)(DI + 2);
            UInt16[ES, DI] = 0x0800;
            DI = (ushort)(DI + 2);
        } while (DI < 0xE85C);          // jc = unsigned <
        CarryFlag = false;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x4A00 — appends DX,BX (two words) at the cs1-resident ring cursor
    /// <c>[0x149A]</c>, wrapping back to 0xE40C when it reaches 0xE85C, then stores the
    /// updated cursor. Companion writer to <see cref="FillCsRing0800_1000_49EA_149EA"/>.
    /// Clean leaf.
    /// </summary>
    public Action AppendCsRingDxBx_1000_4A00_14A00(int gotoAddress) {
        ES = CS;
        ushort di = UInt16[DS, 0x149A];
        UInt16[ES, di] = DX;
        di = (ushort)(di + 2);
        UInt16[ES, di] = BX;
        di = (ushort)(di + 2);
        // cmp di,0xE85C ; jc +3 (skip reset when di < 0xE85C)
        if (di >= 0xE85C) {
            di = 0xE40C;
        }
        DI = di;
        UInt16[DS, 0x149A] = di;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x6231 — classifies <c>bl=[ds:di+8]</c> into AX:
    /// <c>&lt;0x20→0; ==0x20→1; 0x21..0x27→2; 0x28..0x2F→3; ≥0x30→1</c>. BX preserved.
    /// Clean leaf (all branches converge on <c>pop bx; ret</c>).
    /// </summary>
    public Action ClassifyByteDi8_1000_6231_16231(int gotoAddress) {
        byte bl = UInt8[DS, (ushort)(DI + 8)];
        ushort ax;
        if (bl < 0x20) {
            ax = 0;
        } else if (bl < 0x21) {
            ax = 1;
        } else if (bl < 0x28) {
            ax = 2;
        } else if (bl < 0x30) {
            ax = 3;
        } else {
            ax = 1;                     // inc to 4 then sub al,2
        }
        AX = ax;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x456C — index→offset transform. If <c>DI &gt;&gt; 8 == 0xFF</c> returns
    /// <c>DI + 0x02BC</c>; otherwise reads <c>v=[ds:DI]</c>, computes
    /// <c>((v-1).lo &lt;&lt; 4 | (v-1).hi) &amp; 0xFF) + 0x02BC</c>. Clean leaf (both
    /// paths converge on <c>add ax,0x02BC; ret</c>).
    /// </summary>
    public Action IndexTransform456C_1000_456C_1456C(int gotoAddress) {
        if ((DI >> 8) == 0xFF) {
            AX = Alu16.Add(DI, 0x02BC);
            return NearRet();
        }
        ushort v = (ushort)(UInt16[DS, DI] - 1);
        byte al = (byte)(v & 0xFF);
        byte ah = (byte)(v >> 8);
        al = (byte)(al << 4);
        al = (byte)(al | ah);
        AX = Alu16.Add(al, 0x02BC);     // ah cleared by xor ah,ah
        return NearRet();
    }

    /// <summary>
    /// cs1:0x3EFE — 2-D record lookup: <c>si = [ds:(DH*2)+0x13C4]</c> then
    /// <c>si += (DL-1)*5</c>. Returns SI (and AX = the 8-bit product). Clean leaf.
    /// </summary>
    public Action TableLookupDhDl_1000_3EFE_13EFE(int gotoAddress) {
        ushort idx = (ushort)(DH << 1);
        ushort si = UInt16[DS, (ushort)(idx + 0x13C4)];
        byte al = (byte)((DL - 1) & 0xFF);
        ushort prod = (ushort)(al * 5);   // mul ah (ah=5) → AX
        AX = prod;
        si = (ushort)(si + prod);
        SI = si;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x996C — when <c>[0x47D0] != 0</c>, advances SI past 0x20 NUL-terminated
    /// strings in <c>es:si</c> (DS is loaded from ES for the scan, then restored to SS).
    /// Clean leaf.
    /// </summary>
    /// <remarks>
    /// <code>
    /// 996C: 80 3E D0 47 00   cmp byte [0x47D0],0
    /// 9971: 74 0E            jz  9981 (ret)
    /// 9973: B9 20 00         mov cx,0x20
    /// 9976: 06 1F            push es / pop ds
    /// 9978: AC / 0A C0 / 75 FB   ; read until NUL
    /// 997D: E2 F9            loop 9978          ; repeat for cx strings
    /// 997F: 16 1F            push ss / pop ds
    /// 9981: C3               ret
    /// </code>
    /// </remarks>
    public Action Skip32StringsIf47D0_1000_996C_1996C(int gotoAddress) {
        if (UInt8[DS, 0x47D0] == 0) {
            return NearRet();
        }
        ushort cx = 0x20;
        DS = ES;                        // push es ; pop ds
        do {
            byte al;
            do {
                al = UInt8[DS, SI];
                SI = (ushort)(SI + 1);
            } while (al != 0);
            cx--;
        } while (cx != 0);              // loop
        DS = SS;                        // push ss ; pop ds
        CX = 0;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x3A95 — selects (DX,BX) by <c>[0x0005]</c>: if <c>[0x0005] &lt; 0x20</c>
    /// returns DX=0x95,BX=0x39 (CF=1); else DX=0xCA,BX=0x49 (CF=0). Clean two-exit leaf.
    /// </summary>
    public Action SelectDxBxByByte0005_1000_3A95_13A95(int gotoAddress) {
        DX = 0x0095;
        BX = 0x0039;
        byte v = UInt8[DS, 0x0005];
        if (v < 0x20) {                 // jc → ret
            CarryFlag = true;
            ZeroFlag = false;
            return NearRet();
        }
        DX = 0x00CA;
        BX = 0x0049;
        CarryFlag = false;
        ZeroFlag = v == 0x20;
        return NearRet();
    }

    /// <summary>
    /// cs1:0xD6FE — point-in-rect predicate. Returns CF=0 (clc) unless DX is strictly
    /// inside <c>([ds:di], [ds:di+4])</c> and BX &gt; <c>[ds:di+2]</c>, in which case it
    /// falls through to <c>cmp bx,[ds:di+6]</c> and returns that comparison's flags.
    /// Clean two-exit leaf.
    /// </summary>
    /// <remarks>
    /// <code>
    /// D6FE: 3B 15        cmp dx,[di]      ; jbe D710 (clc)
    /// D702: 3B 55 04     cmp dx,[di+4]    ; jnc D710 (clc)
    /// D707: 3B 5D 02     cmp bx,[di+2]    ; jbe D710 (clc)
    /// D70C: 3B 5D 06     cmp bx,[di+6]
    /// D70F: C3           ret
    /// D710: F8 C3        clc ; ret
    /// </code>
    /// </remarks>
    public Action PointInRectDxBx_1000_D6FE_1D6FE(int gotoAddress) {
        ushort dx = DX, bx = BX, di = DI;
        ushort lo = UInt16[DS, di];
        ushort hi = UInt16[DS, (ushort)(di + 4)];
        ushort blo = UInt16[DS, (ushort)(di + 2)];
        ushort bhi = UInt16[DS, (ushort)(di + 6)];
        if (dx <= lo || dx >= hi || bx <= blo) {
            CarryFlag = false;          // clc
            return NearRet();
        }
        int r = (bx - bhi) & 0xFFFF;
        CarryFlag = bx < bhi;
        ZeroFlag = bx == bhi;
        SignFlag = (r & 0x8000) != 0;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x5E42 — like <see cref="ClassifyByteDi8_1000_6231_16231"/> but the AX base is
    /// 0x7A when <c>[0x46EB] &amp; 0x80</c> else 0x3A, classifying <c>cl=[ds:si+8]</c>:
    /// <c>&lt;0x20→base; ==0x20→+1; 0x21..0x27→+2; 0x28..0x2F→+3; ≥0x30→+4</c>.
    /// Clean leaf.
    /// </summary>
    public Action ClassifyByteSi8WithBase_1000_5E42_15E42(int gotoAddress) {
        ushort ax = (UInt8[DS, 0x46EB] & 0x80) != 0 ? (ushort)0x007A : (ushort)0x003A;
        byte cl = UInt8[DS, (ushort)(SI + 8)];
        CX = (ushort)((CX & 0xFF00) | cl);
        if (cl >= 0x20) {
            ax++;
            if (cl >= 0x21) {
                ax++;
                if (cl >= 0x28) {
                    ax++;
                    if (cl >= 0x30) {
                        ax++;
                    }
                }
            }
        }
        AX = ax;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x409A — scans a 0x1C-stride table starting at <c>ds:(0xE4+0x1C)</c> for the
    /// record whose word at <c>[si+6]</c> equals DI; stops at the <c>0xFFFF</c> sentinel.
    /// Returns SI at the matched record (ZF=1) or at the sentinel (then <c>or si,si</c>).
    /// Clean leaf.
    /// </summary>
    public Action FindRecordByDiInTableE4_1000_409A_1409A(int gotoAddress) {
        ushort si = 0x00E4;
        while (true) {
            si = (ushort)(si + 0x1C);
            if (UInt16[DS, si] == 0xFFFF) {
                SI = si;
                ZeroFlag = si == 0;     // or si,si
                SignFlag = (si & 0x8000) != 0;
                CarryFlag = false;
                return NearRet();
            }
            if (DI == UInt16[DS, (ushort)(si + 6)]) {
                SI = si;
                ZeroFlag = true;        // cmp di,[si+6] equal
                CarryFlag = false;
                SignFlag = false;
                return NearRet();
            }
        }
    }

    /// <summary>
    /// cs1:0xE3CC — 16-bit LCG PRNG on state <c>[0xD826]</c>:
    /// <c>state = state*0xCBD1 + 1</c> (low word stored back); returns
    /// AX = (low byte of product&apos;s high word &lt;&lt; 8) | (high byte of new low word).
    /// DX preserved (push/pop). Clean leaf.
    /// </summary>
    /// <remarks>
    /// <code>
    /// E3CC: 52            push dx
    /// E3CD: A1 26 D8      mov  ax, [0xD826]
    /// E3D0: BA D1 CB      mov  dx, 0xCBD1
    /// E3D3: F7 E2         mul  dx              ; DX:AX = AX * 0xCBD1
    /// E3D5: 40            inc  ax
    /// E3D6: A3 26 D8      mov  [0xD826], ax
    /// E3D9: 8A C4         mov  al, ah
    /// E3DB: 8A E2         mov  ah, dl
    /// E3DD: 5A            pop  dx
    /// E3DE: C3            ret
    /// </code>
    /// </remarks>
    public Action LcgPrngD826_1000_E3CC_1E3CC(int gotoAddress) {
        ushort axOld = UInt16[DS, 0xD826];
        uint prod = (uint)axOld * 0xCBD1u;
        ushort axLo = (ushort)((prod & 0xFFFF) + 1);   // inc ax
        ushort dxHi = (ushort)(prod >> 16);
        UInt16[DS, 0xD826] = axLo;
        byte al = (byte)(axLo >> 8);                    // mov al,ah
        byte ah = (byte)(dxHi & 0xFF);                  // mov ah,dl
        AX = (ushort)((ah << 8) | al);
        return NearRet();                               // DX untouched (push/pop)
    }

    /// <summary>
    /// cs1:0x2AAF — searches the 4-byte-record table at <c>ds:0x1190</c> (count byte at
    /// <c>[0x1190]</c>, records start at 0x1191) for one whose key byte <c>[rec+1]</c>
    /// equals AL and, when AL==0x0F, whose word <c>[rec+2]</c> equals DI. On match:
    /// AX=<c>[rec]</c>, DI=<c>[rec+2]</c>, CF=1; otherwise CF=0. SI preserved. Clean leaf.
    /// </summary>
    public Action TableSearchByAlDi1190_1000_2AAF_12AAF(int gotoAddress) {
        ushort savedSi = SI;
        ushort si = 0x1190;
        ushort cx = UInt8[DS, si];          // xor cx,cx ; mov cl,[si]
        if (cx == 0) {                       // jcxz → not found
            SI = savedSi;
            CarryFlag = false;
            return NearRet();
        }
        si = (ushort)(si + 1);               // inc si
        byte al = AL;
        while (true) {
            if (al == UInt8[DS, (ushort)(si + 1)]) {
                if (al != 0x0F || DI == UInt16[DS, (ushort)(si + 2)]) {
                    AX = UInt16[DS, si];
                    DI = UInt16[DS, (ushort)(si + 2)];
                    SI = savedSi;
                    CarryFlag = true;        // stc
                    return NearRet();
                }
            }
            si = (ushort)(si + 4);
            cx--;
            if (cx == 0) {
                break;
            }
        }
        SI = savedSi;
        CarryFlag = false;                   // clc
        return NearRet();
    }

    /// <summary>
    /// cs1:0x5B93 — copies 8 bytes (4 words) from <c>ds:0x46E3</c> to <c>ds:0xD834</c>
    /// (<c>push ds; pop es</c> then <c>movsw ×4</c>). Clean leaf — sibling of the
    /// 0x5B99/0x5BA0/0x5BA8 MemCopy8Bytes family.
    /// </summary>
    public Action MemCopy8Bytes46E3ToD834_1000_5B93_15B93(int gotoAddress) {
        ES = DS;
        ushort si = 0x46E3;
        ushort di = 0xD834;
        for (int n = 0; n < 4; n++) {
            UInt16[ES, di] = UInt16[DS, si];
            si = (ushort)(si + 2);
            di = (ushort)(di + 2);
        }
        SI = si;
        DI = di;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x0CEA — wrapper over <see cref="BitPackTransformAxBxDx_1000_0CF2_10CF2"/>:
    /// <c>xor ax,ax; al=dl; or al,al; js 0x0D04</c>. If DL's sign bit is set it
    /// near-jumps to the still-asm path at <c>cs1:0x0D04</c>; otherwise it falls
    /// straight into the 0x0CF2 bit-pack body (with AX = DL, AH=0).
    /// </summary>
    public Action BitPackOrCall0D04_1000_0CEA_10CEA(int gotoAddress) {
        byte al = DL;
        AX = al;                             // xor ax,ax ; mov al,dl  (ah=0)
        if ((al & 0x80) != 0) {              // or al,al ; js 0x0D04
            return NearJump(0x0D04);
        }
        return BitPackTransformAxBxDx_1000_0CF2_10CF2(0);
    }

    /// <summary>
    /// cs1:0x41C5 — clears <c>[0x4726]=0</c> and <c>[0x21FD]=0</c>; if
    /// <c>word[0x1F12] == 0x4FFB</c> also clears <c>[0x1F11]=0</c>. Clean two-exit leaf.
    /// </summary>
    public Action Reset4726And21FDMaybe1F11_1000_41C5_141C5(int gotoAddress) {
        UInt8[DS, 0x4726] = 0;
        UInt8[DS, 0x21FD] = 0;               // al = 0
        bool eq = UInt16[DS, 0x1F12] == 0x4FFB;
        if (eq) {
            UInt8[DS, 0x1F11] = 0;
        }
        ZeroFlag = eq;                       // flags from cmp word[0x1F12],0x4FFB
        return NearRet();
    }

    /// <summary>
    /// cs1:0x3A73 — loop tail: <c>add dx,0x46; add bx,0x0A; loop 0x3A6A; ret</c>.
    /// Faithful: bump DX/BX, decrement CX, near-jump back to 0x3A6A while CX≠0.
    /// </summary>
    public Action LoopTailAddDx46Bx0A_1000_3A73_13A73(int gotoAddress) {
        DX = (ushort)(DX + 0x46);
        BX = (ushort)(BX + 0x0A);
        CX = (ushort)(CX - 1);
        if (CX != 0) {
            return NearJump(0x3A6A);
        }
        return NearRet();
    }

    /// <summary>
    /// cs1:0xDA25 — appends a 6-byte record <c>{BP, 0, SI}</c> to the array at
    /// <c>ds:0xDC6C</c> (count word at <c>[0xDC6A]</c>, capacity 0x14). No-op when full
    /// (<c>count+1 &gt; 0x14</c>). Also bumps the cursor <c>[[0xDC66]+2]</c> when
    /// <c>[0xDC66] != 0</c>. Clean leaf. Companion of
    /// <see cref="ListRemoveRecordBySi_1000_DA5F_1DA5F"/>.
    /// </summary>
    /// <remarks>
    /// <code>
    /// DA25: 1E 07            push ds / pop es
    /// DA27: BF 6A DC         mov  di, 0xDC6A
    /// DA2A: 8B 05            mov  ax, [di]            ; count
    /// DA2C: 40               inc  ax
    /// DA2D: 3D 14 00         cmp  ax, 0x0014
    /// DA30: 77 20            ja   DA52 (ret, full)
    /// DA32: AB               stosw                    ; [0xDC6A]=count+1, di=0xDC6C
    /// DA33: 48               dec  ax                  ; ax = old count
    /// DA34: 03 C0 / 8B D8 / 03 C0 / 03 C3   ; ax = old*6
    /// DA3C: 03 F8            add  di, ax              ; di = free slot
    /// DA3E: 8B C5 / AB       mov ax,bp / stosw        ; [slot+0]=bp
    /// DA41: 33 C0 / AB       xor ax,ax / stosw        ; [slot+2]=0
    /// DA44: 8B C6 / AB       mov ax,si / stosw        ; [slot+4]=si
    /// DA47: 8B 2E 66 DC      mov  bp, [0xDC66]
    /// DA4B: 0B ED / 74 03    or bp,bp / jz DA52
    /// DA4F: FF 46 02         inc  word [bp+2]
    /// DA52: C3               ret
    /// </code>
    /// </remarks>
    public Action ListAppendRecordDC6A_1000_DA25_1DA25(int gotoAddress) {
        ES = DS;
        ushort count = UInt16[DS, 0xDC6A];
        ushort ax = (ushort)(count + 1);
        if (ax > 0x0014) {              // ja DA52 (unsigned >)
            AX = ax;
            return NearRet();
        }
        UInt16[DS, 0xDC6A] = ax;        // stosw at di=0xDC6A
        ushort oldCount = (ushort)(ax - 1);
        ushort slot = (ushort)(0xDC6C + oldCount * 6);
        UInt16[DS, slot] = BP;
        UInt16[DS, (ushort)(slot + 2)] = 0;
        UInt16[DS, (ushort)(slot + 4)] = SI;
        DI = (ushort)(slot + 6);
        AX = SI;
        ushort bp = UInt16[DS, 0xDC66];
        BP = bp;
        if (bp != 0) {
            UInt16[DS, (ushort)(bp + 2)] = (ushort)(UInt16[DS, (ushort)(bp + 2)] + 1);
        }
        return NearRet();
    }

    /// <summary>
    /// cs1:0xDA5F — removes the first 6-byte record whose key word <c>[base+4]</c>
    /// equals SI from the array at <c>ds:0xDC6C</c> (count <c>[0xDC6A]</c>), shifting
    /// the trailing records down by 6 bytes and decrementing the count. Maintains the
    /// cursor at <c>[0xDC66]</c> (<c>sub [bp],6</c> or <c>dec [bp+2]</c>). Clean leaf
    /// (no calls/hardware). Record layout confirmed by
    /// <see cref="ListAppendRecordDC6A_1000_DA25_1DA25"/>. This is the shared jump
    /// target of the ported thunks 0x0A3E/0x39E6/0x4D00/0x3950.
    /// </summary>
    /// <remarks>
    /// <code>
    /// DA5F: BF 6A DC / 8B 0D       di=0xDC6A ; cx=[di]=count
    /// DA64: E3 0C                  jcxz DA72 (ret, empty)
    /// DA66: 83 C7 06               add di,6              ; di → record0 key (base+4)
    /// DA69: 39 35 / 74 06          cmp [di],si ; jz DA73 (found)
    /// DA6D: 83 C7 06 / E2 F7       add di,6 ; loop DA69
    /// DA72: C3                     ret (not found)
    /// DA73: 83 EF 04               sub di,4             ; di → matched base
    /// DA76: FF 0E 6A DC            dec word [0xDC6A]
    /// DA7A: 8B 2E 66 DC            bp=[0xDC66]
    /// DA7E: 0B ED / 74 0E          or bp,bp ; jz DA90
    /// DA82: 3B 7E 00 / 77 06       cmp di,[bp] ; ja DA8D
    /// DA87: 83 6E 00 06 / EB 03    sub word [bp],6 ; jmp DA90
    /// DA8D: FF 4E 02               dec word [bp+2]
    /// DA90: 49 / 74 DF             dec cx ; jz DA72 (ret, none trailing)
    /// DA93: 8B C1 / 03 C9 / 03 C8  cx = cx*3 (words = trailing*6 bytes)
    /// DA99: 8B F7 / 83 C6 06       si = di ; si += 6
    /// DA9E: 1E 07 / F3 A5          push ds/pop es ; rep movsw
    /// DAA2: C3                     ret
    /// </code>
    /// </remarks>
    public Action ListRemoveRecordBySi_1000_DA5F_1DA5F(int gotoAddress) {
        ushort count = UInt16[DS, 0xDC6A];
        if (count == 0) {               // jcxz DA72
            return NearRet();
        }
        ushort di = 0xDC6A;
        ushort cx = count;
        di = (ushort)(di + 6);          // → record0 key (base+4)
        while (true) {
            if (UInt16[DS, di] == SI) {
                break;                  // jz DA73 (found; cx unchanged)
            }
            di = (ushort)(di + 6);
            cx--;                       // loop
            if (cx == 0) {
                return NearRet();       // not found (loop fell through to DA72)
            }
        }
        // DA73: found — di = key addr; matched base = di-4
        di = (ushort)(di - 4);
        UInt16[DS, 0xDC6A] = (ushort)(count - 1);
        ushort bp = UInt16[DS, 0xDC66];
        BP = bp;
        if (bp != 0) {
            if (di > UInt16[DS, bp]) {              // ja DA8D
                UInt16[DS, (ushort)(bp + 2)] = (ushort)(UInt16[DS, (ushort)(bp + 2)] - 1);
            } else {
                UInt16[DS, bp] = (ushort)(UInt16[DS, bp] - 6);
            }
        }
        cx = (ushort)(cx - 1);          // DA90 dec cx
        if (cx == 0) {
            CX = 0;
            DI = di;
            return NearRet();           // jz DA72 — nothing trailing
        }
        ushort words = (ushort)(cx * 3);   // cx*3 words = trailing*6 bytes
        ushort si = (ushort)(di + 6);      // source = next record base
        ES = DS;
        for (int n = 0; n < words; n++) {
            UInt16[ES, di] = UInt16[DS, si];
            si = (ushort)(si + 2);
            di = (ushort)(di + 2);
        }
        SI = si;
        DI = di;
        CX = 0;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x407E — loads <c>DX=[0x0004], BX=[0x0006]</c>; if <c>BL==0x80</c> instead
    /// returns <c>SI=[0x114E], DX=[SI+2], BX=[SI+4]</c>; otherwise sign-extends BL into
    /// BX (<c>xchg ax,bx; cbw; xchg ax,bx</c>, AX preserved). Clean two-exit leaf.
    /// </summary>
    public Action LoadDxBxFrom0004Or114E_1000_407E_1407E(int gotoAddress) {
        DX = UInt16[DS, 0x0004];
        ushort bx = UInt16[DS, 0x0006];
        BX = bx;
        if ((bx & 0xFF) == 0x80) {
            ushort si = UInt16[DS, 0x114E];
            SI = si;
            DX = UInt16[DS, (ushort)(si + 2)];
            BX = UInt16[DS, (ushort)(si + 4)];
            return NearRet();
        }
        BX = (ushort)(short)(sbyte)(bx & 0xFF);   // xchg ax,bx ; cbw ; xchg ax,bx
        return NearRet();
    }

    /// <summary>cs1:0xD792 — <c>mov si,0x1C66</c> then falls into
    /// <see cref="CopyBlocksTo1AEEThenJmpD1F2_1000_D795_1D795"/>.</summary>
    public Action Thunk_SetSi1C66_FallD795_1000_D792_1D792(int gotoAddress) {
        SI = 0x1C66;
        return NearJump(0xD795);
    }

    /// <summary>
    /// cs1:0xD795 — <c>es=ds</c>; copies 4 blocks of two words from <c>ds:si</c> to
    /// <c>es:0x1AEE</c> (DI strides 0x0E per block); then sets <c>si=0x1AE6, cx=3</c>
    /// and tail-jumps to <c>cs1:0xD1F2</c>. Clean (no calls/hardware), ends in a
    /// near jmp.
    /// </summary>
    public Action CopyBlocksTo1AEEThenJmpD1F2_1000_D795_1D795(int gotoAddress) {
        ES = DS;
        ushort di = 0x1AEE;
        ushort si = SI;
        ushort cx = 4;
        do {
            UInt16[ES, di] = UInt16[DS, si];
            si = (ushort)(si + 2);
            di = (ushort)(di + 2);
            UInt16[ES, di] = UInt16[DS, si];
            si = (ushort)(si + 2);
            di = (ushort)(di + 2);
            di = (ushort)(di + 0x0A);
            cx--;
        } while (cx != 0);
        SI = 0x1AE6;
        CX = 0x0003;
        DI = di;
        return NearJump(0xD1F2);
    }

    /// <summary>
    /// cs1:0xCC0C — HNM stream cursor advance. <c>si += ax</c>; if that overflowed 16-bit
    /// OR <c>si &gt; [0xCE74]</c>, snapshots+resets <c>[0xDC0C]</c> into <c>[0xDC18]</c>;
    /// then <c>ax -= 2</c>, stores it at <c>[0xDC20]</c>, and bumps <c>[0xDBEA]</c>. Clean
    /// leaf. (CX is only modified on the snapshot path — preserved otherwise.)
    /// </summary>
    public Action HnmAdvanceDC0CCursor_1000_CC0C_1CC0C(int gotoAddress) {
        uint s = (uint)SI + AX;
        ushort si = (ushort)s;
        bool carry = s > 0xFFFF;
        SI = si;
        if (carry || si > UInt16[DS, 0xCE74]) {
            ushort old = UInt16[DS, 0xDC0C];   // xchg cx,[0xDC0C] (cx was 0)
            CX = old;
            UInt16[DS, 0xDC0C] = 0;
            UInt16[DS, 0xDC18] = old;          // mov [0xDC18],cx
        }
        ushort ax = (ushort)(AX - 2);
        AX = ax;
        UInt16[DS, 0xDC20] = ax;
        UInt16[DS, 0xDBEA] = (ushort)(UInt16[DS, 0xDBEA] + 1);
        return NearRet();
    }

    /// <summary>
    /// cs1:0xCC2B — HNM visibility / overlap predicate over the decoder cursors
    /// <c>[0xDC0C]/[0xDC10]/[0xDC1A]/[0xDC18]</c> (+ CX). Returns CF as the result of the
    /// final boundary compare (or CF=1 on the early-exit overlap/overflow paths). Clean
    /// single-ret leaf.
    /// </summary>
    public Action HnmVisibilityPredicate_1000_CC2B_1CC2B(int gotoAddress) {
        ushort ax = UInt16[DS, 0xDC0C];
        ushort bx = UInt16[DS, 0xDC10];
        if (ax < bx) {                          // jnc CC3F not taken
            ax = (ushort)(ax + CX);
            ax = (ushort)(ax + 0x12);
            if (bx < ax) {                      // cmp bx,ax ; jc CC4D
                AX = ax;
                CarryFlag = true;
                ZeroFlag = false;
                return NearRet();
            }
        }
        ax = UInt16[DS, 0xDC1A];
        ax = (ushort)(ax + 0x0A);
        uint sum = (uint)ax + CX;
        ax = (ushort)sum;
        AX = ax;
        if (sum > 0xFFFF) {                     // add ax,cx ; jc CC4D
            CarryFlag = true;
            return NearRet();
        }
        ushort m = UInt16[DS, 0xDC18];          // cmp [0xDC18],ax
        int r = (m - ax) & 0xFFFF;
        CarryFlag = m < ax;
        ZeroFlag = m == ax;
        SignFlag = (r & 0x8000) != 0;
        return NearRet();
    }

    /// <summary>
    /// cs1:0xCC4E — HNM ring-buffer advance via the far pointer at <c>[0xDC10]</c>
    /// (<c>les si,[0xDC10]; es:lodsw</c>). Subtracts the chunk size from <c>[0xDC1A]</c>,
    /// updates <c>[0xDC10]</c> (reset-by-2 on wrap/overflow, else += chunk), then
    /// advances/clamps the frame counter pair <c>[0xDBE8]/[0xDBEC]</c>. Clean leaf;
    /// modifies ES (the loaded segment).
    /// </summary>
    public Action HnmRingAdvanceFar_1000_CC4E_1CC4E(int gotoAddress) {
        ushort si = UInt16[DS, 0xDC10];
        ushort es = UInt16[DS, 0xDC12];
        ES = es;
        ushort lod = UInt16[es, si];            // es: lodsw
        si = (ushort)(si + 2);
        UInt16[DS, 0xDC1A] = (ushort)(UInt16[DS, 0xDC1A] - lod);
        uint s = (uint)si + lod;
        si = (ushort)s;
        bool carry = s > 0xFFFF;
        SI = si;
        if (carry || si > UInt16[DS, 0xCE74]) {
            UInt16[DS, 0xDC10] = (ushort)(lod - 2);
            // xor ax,ax ; add [0xDC10],ax  (no-op +0)
        } else {
            UInt16[DS, 0xDC10] = (ushort)(UInt16[DS, 0xDC10] + lod);
        }
        ushort a = (ushort)(UInt16[DS, 0xDBE8] + 1);
        if (a <= UInt16[DS, 0xDBEC]) {          // jbe CC81
            UInt16[DS, 0xDBE8] = a;
        } else {
            a = 1;
            UInt16[DS, 0xDBEC] = 0xFFFF;
            UInt16[DS, 0xDBE8] = a;
        }
        AX = a;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x3DF4 — open-addressing hash insert. Hashes <c>(AL + CH) mod CL</c> by
    /// repeated subtraction; if the slot <c>[ds:hash+DI]</c> is free (0xFF) writes AL
    /// there, else linear-probes indices 0..CL-1 for the first free slot (no write if
    /// the table is full). Clean leaf.
    /// </summary>
    public Action HashInsertOpenAddr_1000_3DF4_13DF4(int gotoAddress) {
        byte cl = (byte)(CX & 0xFF);
        byte ch = (byte)(CX >> 8);
        byte al = (byte)(AX & 0xFF);
        byte bl = (byte)(al + ch);              // mov bx,ax (bl=al) ; add bl,ch
        while (true) {
            bool borrow = bl < cl;              // sub bl,cl ; jnc loops while !borrow
            bl = (byte)(bl - cl);
            if (borrow) {
                break;
            }
        }
        bl = (byte)(bl + cl);                   // add bl,cl (undo last)
        ushort bx = (ushort)((AX & 0xFF00) | bl);
        if (UInt8[DS, (ushort)(bx + DI)] == 0xFF) {
            UInt8[DS, (ushort)(bx + DI)] = al;
            BX = bx;
            return NearRet();
        }
        bx = 0xFFFF;
        while (true) {
            bx = (ushort)(bx + 1);              // inc bx
            if ((byte)(bx & 0xFF) >= cl) {      // cmp bl,cl ; jnc ret
                BX = bx;
                return NearRet();
            }
            if (UInt8[DS, (ushort)(bx + DI)] == 0xFF) {
                UInt8[DS, (ushort)(bx + DI)] = al;
                BX = bx;
                return NearRet();
            }
        }
    }

    /// <summary>
    /// cs1:0x01E0 — stores DI/DX/BX at <c>[si+4]/[si+6]/[si+8]</c>, then derives a flag
    /// byte at <c>[si+0x12]</c>: <c>al=[di]&amp;0x0F</c>, <c>ah=[si+0x12]&amp;0x70</c>,
    /// toggling <c>ah</c> bit 7 once if <c>al&gt;3</c>, again if <c>al&gt;5</c>, again if
    /// <c>al&gt;9</c>; result = <c>(al | ah)</c>. Clean single-ret leaf.
    /// </summary>
    public Action StoreTripletAndPackFlag_1000_01E0_101E0(int gotoAddress) {
        ushort si = SI;
        UInt16[DS, (ushort)(si + 4)] = DI;
        UInt16[DS, (ushort)(si + 6)] = DX;
        UInt16[DS, (ushort)(si + 8)] = BX;
        byte al = (byte)(UInt8[DS, DI] & 0x0F);
        byte ah = (byte)(UInt8[DS, (ushort)(si + 0x12)] & 0x70);
        int xors = al <= 3 ? 0 : al <= 5 ? 1 : al <= 9 ? 2 : 3;
        if ((xors & 1) != 0) {
            ah ^= 0x80;
        }
        al = (byte)(al | ah);
        UInt8[DS, (ushort)(si + 0x12)] = al;
        AX = (ushort)((ah << 8) | al);
        return NearRet();
    }

    /// <summary>
    /// cs1:0xB647 — world→screen fixed-point scale. CX = 2 if <c>[0x46EB]&amp;0x80</c>
    /// else 0. <c>BX = ((BX-[0x197E]) &lt;&lt; CX) + [0xDCF8]</c>. BP = abs(BX&lt;&lt;3)
    /// table-looked-up at <c>[bp+0x494A]</c> then doubled. <c>DX:AX = (DX-[0x197C]) *
    /// BP</c> (signed), shifted left CX bits, then <c>DX += [0xDCF6]</c>. Clean leaf.
    /// </summary>
    public Action WorldToScreenScale_1000_B647_1B647(int gotoAddress) {
        int cx = (UInt8[DS, 0x46EB] & 0x80) != 0 ? 2 : 0;
        ushort bx = (ushort)(BX - UInt16[DS, 0x197E]);
        bx = (ushort)(bx << cx);
        bx = (ushort)(bx + UInt16[DS, 0xDCF8]);
        BX = bx;
        int bpv = (ushort)(BP << 3);
        if ((bpv & 0x8000) != 0) {
            bpv = (-(short)(ushort)bpv) & 0xFFFF;       // neg bp
        }
        ushort bp = UInt16[DS, (ushort)(bpv + 0x494A)];
        bp = (ushort)(bp + bp);
        BP = bp;
        short dxs = (short)(ushort)(DX - UInt16[DS, 0x197C]);
        DX = (ushort)dxs;
        int prod = (int)dxs * (short)bp;                // imul bp (signed)
        uint dxax = (uint)prod;
        ushort ax = (ushort)(dxax & 0xFFFF);
        ushort dx = (ushort)(dxax >> 16);
        for (int n = 0; n < cx; n++) {                  // jcxz skips ; else shl/rcl ×cx
            uint v = (((uint)dx << 16) | ax) << 1;
            ax = (ushort)(v & 0xFFFF);
            dx = (ushort)(v >> 16);
        }
        dx = (ushort)(dx + UInt16[DS, 0xDCF6]);
        AX = ax;
        DX = dx;
        return NearRet();
    }

    /// <summary>
    /// cs1:0xD694 — selects <c>DI = 0x1B9C + 0x0E*k</c> where k is the index of
    /// <c>[0x2582]</c> in the sequence {0x260C,0x2650,0x2694} (else k=3). Always
    /// returns CF=1 (<c>stc</c>); ZF set from the final <c>cmp ax,0x26D8</c> on the
    /// no-match path. Clean leaf.
    /// </summary>
    public Action SelectDiByWord2582_1000_D694_1D694(int gotoAddress) {
        ushort ax = UInt16[DS, 0x2582];
        AX = ax;
        ushort di;
        if (ax == 0x260C) {
            di = 0x1B9C;
        } else if (ax == 0x2650) {
            di = 0x1B9C + 0x0E;
        } else if (ax == 0x2694) {
            di = 0x1B9C + 0x1C;
        } else {
            di = 0x1B9C + 0x2A;
            ZeroFlag = ax == 0x26D8;
        }
        DI = di;
        CarryFlag = true;                               // stc
        return NearRet();
    }

    /// <summary>
    /// cs1:0xC827 — scans up to CX words at <c>ds:si</c> for the first non-zero word
    /// (<c>lodsw; or ax,ax; loopz</c>). If found: BX=SI (past it), AX=0; if all zero /
    /// CX exhausted: AX=0xFFFF. Clean self-loop leaf.
    /// </summary>
    public Action ScanNonZeroWord_1000_C827_1C827(int gotoAddress) {
        ushort ax;
        while (true) {
            ax = UInt16[DS, SI];
            SI = (ushort)(SI + 2);
            AX = ax;
            CX = (ushort)(CX - 1);                      // loopz: dec cx
            if (CX == 0 || ax != 0) {
                break;                                  // exit when cx==0 OR ZF=0
            }
        }
        if (ax == 0) {                                  // jz C833
            AX = 0xFFFF;                                // dec ax (ax was 0)
            return NearRet();
        }
        BX = SI;
        AX = 0;                                         // mov ax,1 ; dec ax
        return NearRet();
    }

    /// <summary>
    /// cs1:0x9A7B — derives an index from <c>[0x47D0]</c>/<c>[0x47C4]</c>/<c>[0x002A]</c>,
    /// computes <c>bp = (index-1)*2 + [0x00F0]</c>, then with <c>es=[0xDBB2]</c> does
    /// <c>si = [0x47CA] + es:[bp+si]</c> and tail-jumps to the C#
    /// <see cref="LcgPrng_1000_E3B7_01E3B7"/> at <c>cs1:0xE3B7</c>.
    /// </summary>
    public Action ComputeIndexThenJmpE3B7_1000_9A7B_19A7B(int gotoAddress) {
        byte al = UInt8[DS, 0x47D0];
        ushort bx = 0x0F18;
        if (al == 0) {
            al = 5;
            bx = 0x0F38;
            if (UInt16[DS, 0x47C4] == 7 && UInt8[DS, 0x002A] >= 0xC8) {
                al = 6;
            }
        }
        BX = bx;
        al = (byte)(al - 1);
        ushort ax = (ushort)(al << 1);                  // xor ah,ah ; shl ax,1
        AX = ax;
        ushort bp = (ushort)(ax + UInt16[DS, 0x00F0]);
        BP = bp;
        ushort si = UInt16[DS, 0x47CA];
        ushort es = UInt16[DS, 0xDBB2];
        ES = es;
        si = (ushort)(si + UInt16[es, (ushort)(bp + si)]);
        SI = si;
        return NearJump(0xE3B7);
    }

    /// <summary>
    /// cs1:0x08F0 — clears <c>[0x47A4]/[0x46DF]</c>, stashes <c>[0x0004]=DX,
    /// [0x0006]=BX, [0x0008]=DH</c>, computes <c>[0x114E] = 0x1C*BH + 0x00E4</c>, then
    /// tail-jumps to <c>cs1:0x2D74</c>.
    /// </summary>
    public Action InitVarsThenJmp2D74_1000_08F0_108F0(int gotoAddress) {
        UInt8[DS, 0x47A4] = 0;
        UInt8[DS, 0x46DF] = 0;
        UInt16[DS, 0x0004] = DX;
        UInt16[DS, 0x0006] = BX;
        UInt8[DS, 0x0008] = (byte)(DX >> 8);            // dh
        ushort ax = (ushort)(0x1C * (BX >> 8));         // al=0x1C ; mul bh
        ax = (ushort)(ax + 0x00E4);
        AX = ax;
        UInt16[DS, 0x114E] = ax;
        return NearJump(0x2D74);
    }

    /// <summary>
    /// cs1:0xF3A7 — two-level keyed search over the ES:DI structure pointed to by the
    /// far pointer at <c>ss:[0xDBBC]</c>. Scans the 5-byte-stride level-1 array for the
    /// first record ordered &gt;= the key (DL, AX) (compare DL vs <c>[di+4]</c>, then AX
    /// vs <c>[di+2]</c>), follows the link <c>[di]</c>, then scans the 0x0A-byte-stride
    /// level-2 array (DL vs <c>[di+2]</c>, AX vs <c>[di]</c>). Returns DI; ES modified by
    /// the <c>les</c>. Clean leaf.
    /// </summary>
    public Action TwoLevelKeyedSearch_1000_F3A7_1F3A7(int gotoAddress) {
        ushort di = UInt16[SS, 0xDBBC];
        ES = UInt16[SS, 0xDBBE];
        di = (ushort)(di - 5);
        while (true) {
            di = (ushort)(di + 5);
            byte k = UInt8[ES, (ushort)(di + 4)];
            if (DL == k) {
                if (!(AX > UInt16[ES, (ushort)(di + 2)])) {
                    break;                                  // ax <= [di+2] → exit
                }
            } else if (!(DL > k)) {
                break;                                      // dl < k → exit
            }
        }
        di = UInt16[ES, di];                                // follow link
        di = (ushort)(di - 0x0A);
        bool carry;
        while (true) {
            di = (ushort)(di + 0x0A);
            byte k2 = UInt8[ES, (ushort)(di + 2)];
            if (DL == k2) {
                if (!(AX > UInt16[ES, di])) {
                    // exit via `cmp ax,es:[di]`: CF=1 iff ax<[di] (ax==[di] => ZF, CF=0)
                    carry = AX < UInt16[ES, di];
                    break;
                }
            } else if (!(DL > k2)) {
                // exit via `cmp dl,es:[di+2]` with dl<k2 => CF=1
                carry = true;
                break;
            }
        }
        DI = di;
        // Faithful `retn` CF (asm sub_11277): CF=0 only on an exact key match
        // (dl==[di+2] && ax==[di]); CF=1 otherwise. Callers (e.g. sub_11177)
        // branch on this with `jb`.
        CarryFlag = carry;
        return NearRet();
    }

    /// <summary>
    /// cs1:0xB941 — writes a handler pair into <c>[0x2050]/[0x2052]</c> selected by
    /// <c>[0xDD02]</c> (<c>{0xB1,0xB96B}</c> if zero else <c>{0xB2,0xB961}</c>), sets
    /// BX=0xD917, and tail-jumps to <c>cs1:0xD338</c>.
    /// </summary>
    public Action SetupBp204AThenJmpD338_1000_B941_1B941(int gotoAddress) {
        ushort bp = 0x204A;
        BP = bp;
        ushort ax, bx;
        if (UInt8[DS, 0xDD02] == 0) {
            ax = 0x00B1;
            bx = 0xB96B;
        } else {
            ax = 0x00B2;
            bx = 0xB961;
        }
        UInt16[DS, (ushort)(bp + 6)] = ax;
        UInt16[DS, (ushort)(bp + 8)] = bx;
        AX = ax;
        BX = 0xD917;
        return NearJump(0xD338);
    }

    /// <summary>
    /// cs1:0x945B — if <c>word[0x479E] != 0</c> tail-jumps to the still-asm body at
    /// <c>cs1:0x9468</c>; otherwise loads <c>AX=[0x47C4]</c> and tail-jumps to
    /// <c>cs1:0x93AA</c>. Faithful conditional dispatcher.
    /// </summary>
    public Action DispatchByWord479E_1000_945B_1945B(int gotoAddress) {
        if (UInt16[DS, 0x479E] != 0) {
            return NearJump(0x9468);
        }
        AX = UInt16[DS, 0x47C4];
        return NearJump(0x93AA);
    }

    /// <summary>
    /// cs1:0xAD43 — loads <c>AL=[0xDBCC]</c>, clears <c>[0xDBCB]=0</c>; if AL != 0
    /// tail-jumps to the still-asm body at <c>cs1:0xAD95</c>, else returns.
    /// </summary>
    public Action ClearDBCBMaybeJmpAD95_1000_AD43_1AD43(int gotoAddress) {
        byte al = UInt8[DS, 0xDBCC];
        AL = al;
        UInt8[DS, 0xDBCB] = 0;
        if (al != 0) {
            return NearJump(0xAD95);
        }
        return NearRet();
    }

    /// <summary>
    /// cs1:0x40C9 — if <c>[si+0x0F] &amp; 0x40</c> is set, stores <c>[si]=DX,
    /// [si+2]=BX</c> and returns; otherwise tail-jumps to the still-asm body at
    /// <c>cs1:0x40D4</c>.
    /// </summary>
    public Action StoreDxBxOrJmp40D4_1000_40C9_140C9(int gotoAddress) {
        if ((UInt8[DS, (ushort)(SI + 0x0F)] & 0x40) == 0) {
            return NearJump(0x40D4);
        }
        UInt16[DS, SI] = DX;
        UInt16[DS, (ushort)(SI + 2)] = BX;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x4415 — test-and-clear: <c>AL = [0x46EB]; [0x46EB] = 0</c>; if the old value
    /// was non-zero tail-jumps to the still-asm body at <c>cs1:0x4420</c>, else returns.
    /// </summary>
    public Action TestClear46EBOrJmp4420_1000_4415_14415(int gotoAddress) {
        byte al = UInt8[DS, 0x46EB];
        AL = al;
        UInt8[DS, 0x46EB] = 0;
        if (al != 0) {
            return NearJump(0x4420);
        }
        return NearRet();
    }

    /// <summary>cs1:0x3120 — if <c>[si+0x0F] &amp; 0x40</c> tail-jumps to
    /// <c>cs1:0x30CA</c>, else returns.</summary>
    public Action TestSi0FBit40OrJmp30CA_1000_3120_13120(int gotoAddress) {
        if ((UInt8[DS, (ushort)(SI + 0x0F)] & 0x40) != 0) {
            return NearJump(0x30CA);
        }
        return NearRet();
    }

    /// <summary>cs1:0x1860 — if <c>[0x11C9] == 0</c> tail-jumps to the still-asm body
    /// at <c>cs1:0x1868</c>, else returns.</summary>
    public Action GuardByte11C9OrJmp1868_1000_1860_11860(int gotoAddress) {
        if (UInt8[DS, 0x11C9] == 0) {
            return NearJump(0x1868);
        }
        return NearRet();
    }

    /// <summary>cs1:0xAED6 — if <c>[0x11C9] == 0</c> tail-jumps to the still-asm body
    /// at <c>cs1:0xAEDE</c>, else returns.</summary>
    public Action GuardByte11C9OrJmpAEDE_1000_AED6_1AED6(int gotoAddress) {
        if (UInt8[DS, 0x11C9] == 0) {
            return NearJump(0xAEDE);
        }
        return NearRet();
    }

    /// <summary>
    /// cs1:0x6B34 — bumps <c>[0x46F6]</c>; if <c>([0x46F6] &amp; 3) == 0</c> tail-jumps
    /// to <c>cs1:0x6B4B</c>; else sets CX=1, loads <c>DI=[0x4752]</c> and tail-jumps to
    /// <c>cs1:0x6B55</c> when DI != 0, otherwise returns.
    /// </summary>
    public Action Bump46F6Dispatch_1000_6B34_16B34(int gotoAddress) {
        UInt8[DS, 0x46F6] = (byte)(UInt8[DS, 0x46F6] + 1);
        byte al = (byte)(UInt8[DS, 0x46F6] & 3);
        AL = al;
        if (al == 0) {
            return NearJump(0x6B4B);
        }
        CX = 1;
        ushort di = UInt16[DS, 0x4752];
        DI = di;
        if (di != 0) {
            return NearJump(0x6B55);
        }
        return NearRet();
    }

    /// <summary>
    /// cs1:0x5198 — decodes a packed byte (AL+0x20 quadrant) into a sign-extended delta
    /// and axis. Two-exit clean leaf: high-quadrant path returns BX=AX, DX=±0x20;
    /// low-quadrant path returns DX=AX, BX=±0x0020/0xFFE0.
    /// </summary>
    public Action DecodePackedDelta_1000_5198_15198(int gotoAddress) {
        byte bl = (byte)((AX & 0xFF) + 0x20);
        byte bh = (byte)(bl & 0x7F);
        byte al = (byte)(AX & 0xFF);
        if (bh >= 0x40) {                       // jc 51BA NOT taken
            short dx = 0x20;
            al = (byte)(al - 0x40);
            if ((bl & 0x80) != 0) {             // or bl,bl ; jns skips neg
                dx = (short)-dx;
                al = (byte)(al - 0x80);
                al = (byte)-(sbyte)al;
            }
            short ax = (sbyte)al;               // cbw
            BX = (ushort)ax;
            DX = (ushort)dx;
            AX = (ushort)ax;
            return NearRet();
        }
        ushort bxv = 0xFFE0;
        if ((bl & 0x80) != 0) {
            al = (byte)(al - 0x80);
            al = (byte)-(sbyte)al;
            bxv = (ushort)-(short)bxv;          // neg bx → 0x0020
        }
        short ax2 = (sbyte)al;                  // cbw
        DX = (ushort)ax2;
        BX = bxv;
        AX = (ushort)ax2;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x2EFB — prologue (<c>es=ds; [0x1F0F]=0; bx=[0x0006]; dx=[0x0004]</c>) then
    /// dispatches: if <c>BL == 0x80</c> continues into the still-asm body at
    /// <c>cs1:0x2F13</c>, else tail-jumps to <c>cs1:0x2FAA</c>.
    /// </summary>
    public Action Prologue2EFBDispatch_1000_2EFB_12EFB(int gotoAddress) {
        ES = DS;
        ushort di = 0x1F0F;
        UInt8[ES, di] = 0;                      // xor al,al ; stosb
        di = (ushort)(di + 1);
        DI = di;
        AL = 0;
        ushort bx = UInt16[DS, 0x0006];
        BX = bx;
        DX = UInt16[DS, 0x0004];
        if ((bx & 0xFF) == 0x80) {
            return NearJump(0x2F13);
        }
        return NearJump(0x2FAA);
    }

    /// <summary>
    /// cs1:0x2FFB — selects SI (0x1D72 / 0x1D1E) from <c>[0x002B]/[0x11C9]/[0x11CA]/
    /// [0x11CB]</c> and tail-jumps to <c>cs1:0xD72B</c>; if <c>([0x11C9] &amp; 3) == 0</c>
    /// tail-jumps to the continuation at <c>cs1:0x3020</c> instead.
    /// </summary>
    public Action SelectSiThenJmpD72B_1000_2FFB_12FFB(int gotoAddress) {
        if (UInt8[DS, 0x002B] != 0) {
            SI = 0x1D1E;
            return NearJump(0xD72B);
        }
        if ((UInt8[DS, 0x11C9] & 3) == 0) {
            return NearJump(0x3020);
        }
        if (UInt8[DS, 0x11CA] != 0) {
            SI = 0x1D1E;
            return NearJump(0xD72B);
        }
        if (UInt8[DS, 0x11CB] != 0) {
            SI = 0x1D72;
            return NearJump(0xD72B);
        }
        SI = 0x1D1E;
        return NearJump(0xD72B);
    }

    /// <summary>cs1:0x0F08 — if <c>word[0x0010] &amp; 0x0080</c> tail-jumps to the
    /// still-asm body at <c>cs1:0x0F13</c>, else tail-jumps to <c>cs1:0x0960</c>.</summary>
    public Action TestWord0010Bit80Dispatch_1000_0F08_10F08(int gotoAddress) {
        if ((UInt16[DS, 0x0010] & 0x0080) != 0) {
            return NearJump(0x0F13);
        }
        return NearJump(0x0960);
    }

    /// <summary>cs1:0x329D — if <c>[si+3] == 0</c> tail-jumps to the still-asm body at
    /// <c>cs1:0x32AA</c>; else <c>AX=0; [si+0x10] &amp;= 0xFFF3</c> and returns.</summary>
    public Action GuardSi3OrJmp32AA_1000_329D_1329D(int gotoAddress) {
        if (UInt8[DS, (ushort)(SI + 3)] == 0) {
            return NearJump(0x32AA);
        }
        AX = 0;
        UInt16[DS, (ushort)(SI + 0x10)] = (ushort)(UInt16[DS, (ushort)(SI + 0x10)] & 0xFFF3);
        return NearRet();
    }

    /// <summary>cs1:0x79DE — test-and-clear <c>[0x46FA]</c>: if old value 0 tail-jumps
    /// to <c>cs1:0x79DB</c>, else <c>SI=0x18DF</c> and tail-jumps to <c>cs1:0x5F9F</c>.</summary>
    public Action TestClear46FAOrJmp_1000_79DE_179DE(int gotoAddress) {
        ushort v = UInt16[DS, 0x46FA];
        AX = v;
        UInt16[DS, 0x46FA] = 0;
        if (v == 0) {
            return NearJump(0x79DB);
        }
        SI = 0x18DF;
        return NearJump(0x5F9F);
    }

    /// <summary>cs1:0x9985 — busy-wait: while <c>[0x47CE] &amp; 7 != 0</c> loops back to
    /// the poll setup at <c>cs1:0x9982</c>; returns once clear. (Bit cleared by an ISR;
    /// Spice86 services interrupts between dispatches.)</summary>
    public Action SpinUntil47CELow3Clear_1000_9985_19985(int gotoAddress) {
        if ((UInt16[DS, 0x47CE] & 0x0007) != 0) {
            return NearJump(0x9982);
        }
        return NearRet();
    }

    /// <summary>cs1:0x3AF9 — if <c>[0x002B] == 0</c> continues into the still-asm body
    /// at <c>cs1:0x3B03</c>, else tail-jumps to <c>cs1:0xC43E</c>.</summary>
    public Action Guard002BOrJmpC43E_1000_3AF9_13AF9(int gotoAddress) {
        if (UInt8[DS, 0x002B] == 0) {
            return NearJump(0x3B03);
        }
        return NearJump(0xC43E);
    }

    /// <summary>
    /// cs1:0xD50F — pushes BX,CX,DX,SI,DI,BP then dispatches: if <c>[0x4774] == 0</c>
    /// tail-jumps to the still-asm body at <c>cs1:0xD523</c>, else loads
    /// <c>CL=[0x4775]</c> and tail-jumps to <c>cs1:0xD5DD</c>. The pushed registers are
    /// popped by the asm continuation (chain-port: real SS:SP pushes).
    /// </summary>
    public Action SaveRegsThenDispatchD50F_1000_D50F_1D50F(int gotoAddress) {
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BX;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = CX;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DX;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = SI;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = DI;
        SP = (ushort)(SP - 2); UInt16[SS, SP] = BP;
        if (UInt8[DS, 0x4774] == 0) {
            return NearJump(0xD523);
        }
        CX = (ushort)((CX & 0xFF00) | UInt8[DS, 0x4775]);
        return NearJump(0xD5DD);
    }

    /// <summary>
    /// cs1:0xB5CF — adjusts BH by AH (direction = sign of AH); if the resulting BL would
    /// leave the signed range (0x62 for +AH / 0x9E for −AH) the change is reverted.
    /// Always finishes <c>cbw; dx += (int16)AL; ret</c>. Clean single-ret leaf.
    /// </summary>
    public Action ClampBxByAhThenAddDx_1000_B5CF_1B5CF(int gotoAddress) {
        byte al = (byte)(AX & 0xFF);
        byte ah = (byte)(AX >> 8);
        byte bl = (byte)(BX & 0xFF);
        byte bh = (byte)(BX >> 8);
        if (ah != 0) {
            if ((sbyte)ah < 0) {                       // js B5E6
                int s = bh + ah;
                bh = (byte)s;
                if (s <= 0xFF) {                       // jc B5F5 NOT taken
                    bl = (byte)(bl - 1);
                    if (!((sbyte)bl > unchecked((sbyte)0x9E))) {  // jg B5F5 NOT taken
                        bl = (byte)(bl + 1);
                        bh = (byte)(bh - ah);
                    }
                }
            } else {                                   // ah > 0
                int s = bh + ah;
                bh = (byte)s;
                if (s > 0xFF) {                         // jnc B5F5 NOT taken (carry)
                    bl = (byte)(bl + 1);
                    if (!((sbyte)bl < (sbyte)0x62)) {  // jl B5F5 NOT taken
                        bl = (byte)(bl - 1);
                        bh = (byte)(bh - ah);
                    }
                }
            }
        }
        BX = (ushort)((bh << 8) | bl);
        short ax = (sbyte)al;                          // cbw
        AX = (ushort)ax;
        DX = (ushort)(DX + (ushort)ax);
        return NearRet();
    }

    /// <summary>
    /// cs1:0x686E — table-driven index transform. Early-returns CF=1 when
    /// <c>[0x46EB] &lt; 0x80</c>; tail-jumps to the still-asm body at <c>cs1:0x68AF</c>
    /// when <c>[si+3] &amp; 0x40</c>. Otherwise computes
    /// <c>DX = (int8)[idx*2+0x1672] + [si+2]</c>, <c>AX = (int8)[idx*2+0x1673]</c> where
    /// <c>idx = ((([si+2]-1) ^ (([di+0x0A]&amp;2)?8:0)) &amp; 0x0F)</c>, then folds
    /// <c>BX=[si+4]</c> (CF=1 if BH&lt;0x80 else <c>bh=0; bx+=bx; clc</c>). Clean leaf
    /// (DI preserved by push/pop).
    /// </summary>
    public Action IndexTransform686E_1000_686E_1686E(int gotoAddress) {
        byte v = UInt8[DS, 0x46EB];
        if (v < 0x80) {                                // cmp v,0x80 ; jc 68AE
            CarryFlag = true;
            ZeroFlag = v == 0x80;
            return NearRet();
        }
        if ((UInt8[DS, (ushort)(SI + 3)] & 0x40) != 0) {
            return NearJump(0x68AF);
        }
        ushort bx = (ushort)((BX & 0xFF00) | UInt8[DS, (ushort)(SI + 2)]);
        bx = (ushort)(bx - 1);                         // dec bx
        ushort di = UInt16[DS, (ushort)(SI + 4)];      // push di ; mov di,[si+4]
        if ((UInt8[DS, (ushort)(di + 0x0A)] & 2) != 0) {
            bx = (ushort)((bx & 0xFF00) | (byte)((bx & 0xFF) ^ 8));   // xor bl,8
        }
        // pop di (DI register unchanged for caller)
        bx = (ushort)((bx & 0x000F) << 1);             // and bx,0x0F ; add bx,bx
        short dx = (sbyte)UInt8[DS, (ushort)(bx + 0x1672)];     // cbw ; mov dx,ax
        short ax = (sbyte)UInt8[DS, (ushort)(bx + 0x1673)];     // cbw
        dx = (short)(dx + UInt16[DS, (ushort)(SI + 2)]);        // add dx,[si+2]
        bx = UInt16[DS, (ushort)(SI + 4)];             // mov bx,[si+4]
        if ((bx >> 8) < 0x80) {                        // cmp bh,0x80 ; jc 68AE
            DX = (ushort)dx;
            AX = (ushort)ax;
            BX = bx;
            CarryFlag = true;
            return NearRet();
        }
        bx = (ushort)((bx & 0x00FF) << 1);             // xor bh,bh ; add bx,bx
        DX = (ushort)dx;
        AX = (ushort)ax;
        BX = bx;
        CarryFlag = false;                             // clc
        return NearRet();
    }

    /// <summary>
    /// cs1:0x4EC6 — derives <c>[0xDC02]</c> (and sometimes <c>[0x487E]</c>) from
    /// <c>[0x487E]</c>, <c>AL&amp;0x0F</c> vs 8, and <c>[0xDC00]</c>. BX preserved
    /// (push/pop); AX becomes <c>[0xDC00]</c> on the main paths. Clean two-exit leaf.
    /// </summary>
    public Action ComputeDC02FromState_1000_4EC6_14EC6(int gotoAddress) {
        ushort savedBx = BX;
        ushort bx = UInt16[DS, 0x487E];
        if (bx < 2) {
            UInt16[DS, 0xDC02] = bx;
            BX = savedBx;
            return NearRet();
        }
        bool alGE8 = (AL & 0x0F) >= 8;                 // cmp al,8 ; jnc 4EF3
        ushort ax = UInt16[DS, 0xDC00];
        AX = ax;
        if (!alGE8) {
            if (ax <= 2) {
                // bx stays [0x487E]
            } else if (ax <= 4) {
                bx = 5;
            } else {
                bx = 2;
                UInt16[DS, 0x487E] = bx;
            }
        } else {
            bx = (ax <= 2 || ax == 5) ? (ushort)3 : (ushort)4;
        }
        UInt16[DS, 0xDC02] = bx;
        BX = savedBx;
        return NearRet();
    }

    /// <summary>
    /// cs1:0xA30B — variable-length operand reader from <c>es:si</c>. Reads the opcode
    /// byte; if &lt;0x80 reads a 1-byte index and returns <c>AX = byte[idx]</c> (opcode
    /// 1) or <c>word[idx]</c> (otherwise); if ==0x80 returns the next byte; if &gt;0x80
    /// returns the next word. Clean multi-exit leaf (BX preserved on the &lt;0x80 path).
    /// </summary>
    public Action DecodeOperandEsSi_1000_A30B_1A30B(int gotoAddress) {
        byte al = UInt8[ES, SI];
        SI = (ushort)(SI + 1);
        AL = al;
        if (al < 0x80) {
            ushort savedBx = BX;
            ushort bx = UInt8[ES, SI];                 // es:[si] ; xor bh,bh
            SI = (ushort)(SI + 1);
            if (al == 1) {
                AX = UInt8[DS, bx];                    // mov al,[bx] ; xor ah,ah
            } else {
                AX = UInt16[DS, bx];                   // mov ax,[bx]
            }
            BX = savedBx;
            return NearRet();
        }
        if (al != 0x80) {
            ushort w = UInt16[ES, SI];                 // es: lodsw
            SI = (ushort)(SI + 2);
            AX = w;
            return NearRet();
        }
        byte b = UInt8[ES, SI];                        // es: lodsb
        SI = (ushort)(SI + 1);
        AX = b;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x34D0 — bumps counter bytes in the table at <c>ds:0x61</c>/<c>0x7F</c>
    /// (selected by <c>[si+3]&amp;0x40</c>) indexed by <c>[si+3]&amp;0x0F</c>, plus a
    /// secondary counter at <c>0x71 + ([si+0x12]&amp;0x0F)</c>. No-op when
    /// <c>[si+3]&amp;0x20</c>; special-cases <c>[si+3]==0x80</c> (bump <c>[0x0090]</c>).
    /// Clean multi-branch leaf (all paths converge on a <c>ret</c>).
    /// </summary>
    public Action UpdateCounterTable34D0_1000_34D0_134D0(int gotoAddress) {
        if ((UInt8[DS, (ushort)(SI + 3)] & 0x20) != 0) {
            return NearRet();
        }
        byte al = UInt8[DS, (ushort)(SI + 3)];
        ushort dx = (al & 0x40) != 0 ? (ushort)0x007F : (ushort)0x0061;
        ushort bx = dx;
        if ((UInt8[DS, (ushort)(SI + 0x10)] & 0x80) == 0) {
            bx = (ushort)(bx - 1);
            if (al == 0x80) {
                UInt8[DS, 0x0090] = (byte)(UInt8[DS, 0x0090] + 1);
                return NearRet();
            }
        }
        UInt8[DS, bx] = (byte)(UInt8[DS, bx] + 1);     // inc byte[bx]
        byte ah = (byte)(al & 0x03);
        al = (byte)(al & 0x0F);
        if (ah == 3) {
            al = (byte)(al & 0xFC);
        }
        ushort ax = al;                                // xor ah,ah
        bx = (ushort)(dx + ax);
        UInt8[DS, (ushort)(bx + 1)] = (byte)(UInt8[DS, (ushort)(bx + 1)] + 1);
        if (bx >= 0x7F) {                              // cmp bx,0x7F ; jnc 351A
            BX = bx;
            AX = ax;
            return NearRet();
        }
        ushort a2 = (ushort)(UInt16[DS, (ushort)(SI + 0x12)] & 0x000F);
        bx = (ushort)(0x71 + a2);
        UInt8[DS, bx] = (byte)(UInt8[DS, bx] + 1);
        BX = bx;
        AX = a2;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x6EFD — clamps/adjusts the accumulator byte <c>[si+0x15]</c>: starts from a
    /// base (optionally +0x14 when <c>[0x00FA]≠0</c>), special-cases <c>ah==6</c> (DI vs
    /// <c>[0x114E]</c>, +0x1E) and <c>(ah&amp;0xFE)==8</c>, ceilings at 0x64, applies a
    /// −0x28/floor-0x0A when <c>0x64 ≤ [0x002A] &lt; 0x68</c>, then
    /// <c>[si+0x15] = min([si+0x15]+al, 0x64)</c>. Clean single-ret leaf (DI push/pop).
    /// </summary>
    public Action ClampAdjustSi15_1000_6EFD_16EFD(int gotoAddress) {
        byte ah = (byte)(UInt8[DS, (ushort)(SI + 3)] & 0x0F);
        byte al = UInt8[DS, (ushort)(SI + 0x15)];
        if (UInt8[DS, 0x00FA] != 0) {
            al = (byte)(al + 0x14);
        }
        bool gotoBlockC = false;
        if (ah == 6) {
            if (UInt16[DS, (ushort)(SI + 4)] != UInt16[DS, 0x114E]) {
                gotoBlockC = true;                    // jnz 6F31
            } else {
                al = (byte)(al + 0x1E);               // then block B
            }
        } else {
            ah = (byte)(ah & 0xFE);
            if (ah == 8) {
                al = 0x64;                            // 6F2F
                gotoBlockC = true;
            }
        }
        if (!gotoBlockC) {
            // block B (6F2B): cmp al,0x64 ; jc 6F31 ; else al=0x64
            if (al >= 0x64) {
                al = 0x64;
            }
        }
        // block C (6F31)
        byte v2A = UInt8[DS, 0x002A];
        if (v2A >= 0x64 && v2A < 0x68) {
            al = (byte)(al - 0x28);
            if ((sbyte)al < 0x0A) {                   // jge 6F47 (signed) else al=0x0A
                al = 0x0A;
            }
        }
        // 6F47: add [si+0x15],al ; ceiling 0x64
        byte c = (byte)(UInt8[DS, (ushort)(SI + 0x15)] + al);
        if (c > 0x64) {
            c = 0x64;
        }
        UInt8[DS, (ushort)(SI + 0x15)] = c;
        AX = (ushort)((ah << 8) | al);
        return NearRet();
    }

    /// <summary>
    /// cs1:0x3D83 — <c>do_weird_shit_with_stack_buffer_ida</c>. Fills the 0x17-byte
    /// buffer at <c>[0x47F6]</c> with 0xFF, then either copies a CS-resident length-
    /// prefixed block (when <c>[0x4774]≠0</c> and <c>[0x4778]≠0</c>) or builds a hash
    /// table into it via <see cref="HashInsertOpenAddr_1000_3DF4_13DF4"/> driven by the
    /// bit pattern <c>[0x0012]^[0x0010]</c> and the count <c>[0x476A]</c>; finally
    /// advances <c>[0x47F6]</c> by <c>[ds:si]-1</c>. Clean leaf (only C# calls).
    /// </summary>
    /// <remarks>
    /// <code>
    /// 3D83: 1E 07               push ds / pop es
    /// 3D85: B8 FFFF / B9 0017 / 8B 3E F6 47 / F3 AA   ; memset [0x47F6],0xFF,0x17
    /// 3D91: 8B 3E F6 47         mov di,[0x47F6]
    /// 3D95: cmp [0x4774],0 / jz 3DB0
    /// 3D9C: ax=[0x4778] / or ax,ax / jz 3DB0
    /// 3DA3: push si / si=ax / cs:lodsb / cl=al / rep cs:movsb / pop si / jmp 3DE5
    /// 3DB0: dx=[0x0012] ^ [0x0010] ; cl=[si] ; or cl,cl ; jz 3DE5
    /// 3DBE: ch=[0x00C5]&0x0F ; ax=0xFFFF
    /// 3DC8: inc ax ; shr dx,1 ; jnc 3DD0 ; call 3DF4 ; 3DD0: or dx,dx ; jnz 3DC8
    /// 3DD4: dl=[0x476A] ; dec dx ; jle 3DE5
    /// 3DDB: ax=0x000F ; 3DDE: inc ax ; call 3DF4 ; dec dx ; jnz 3DDE
    /// 3DE5: lodsb ; xor ah,ah ; di=[0x47F6] ; dec ax ; di+=ax ; [0x47F6]=di ; ret
    /// </code>
    /// </remarks>
    public Action DoWeirdStackBuffer_1000_3D83_13D83(int gotoAddress) {
        ES = DS;                                       // push ds ; pop es
        AX = 0xFFFF;
        ushort di = UInt16[DS, 0x47F6];
        for (int n = 0; n < 0x17; n++) {               // rep stosb (al=0xFF)
            UInt8[ES, di] = 0xFF;
            di = (ushort)(di + 1);
        }
        CX = 0;
        di = UInt16[DS, 0x47F6];                       // mov di,[0x47F6]
        DI = di;
        bool to3DB0 = UInt8[DS, 0x4774] == 0;
        if (!to3DB0) {
            ushort ax = UInt16[DS, 0x4778];
            AX = ax;
            if (ax == 0) {
                to3DB0 = true;
            } else {
                // push si ; si=ax ; cs:lodsb ; cl=al ; rep cs:movsb ; pop si ; jmp 3DE5
                ushort savedSi = SI;
                ushort src = ax;
                byte len = UInt8[cs1, src];
                src = (ushort)(src + 1);
                AL = len;
                ushort cx = len;                       // mov cl,al (ch=0)
                while (cx != 0) {
                    UInt8[ES, di] = UInt8[cs1, src];
                    src = (ushort)(src + 1);
                    di = (ushort)(di + 1);
                    cx--;
                }
                CX = 0;
                SI = savedSi;
                DI = di;
                return Tail3DE5();
            }
        }
        // 3DB0
        ushort dx = (ushort)(UInt16[DS, 0x0012] ^ UInt16[DS, 0x0010]);
        DX = dx;
        byte cl = UInt8[DS, SI];                        // mov cl,[si]
        CX = (ushort)((CX & 0xFF00) | cl);
        if (cl == 0) {                                  // or cl,cl ; jz 3DE5
            return Tail3DE5();
        }
        byte ch = (byte)(UInt8[DS, 0x00C5] & 0x0F);
        CX = (ushort)((ch << 8) | cl);
        ushort axw = 0xFFFF;
        do {
            axw = (ushort)(axw + 1);                    // inc ax
            bool cf = (dx & 1) != 0;                    // shr dx,1
            dx = (ushort)(dx >> 1);
            DX = dx;
            if (cf) {                                   // jnc 3DD0 NOT taken → call
                AX = axw;
                HashInsertOpenAddr_1000_3DF4_13DF4(0);
                axw = AX;
            }
        } while (dx != 0);                              // or dx,dx ; jnz 3DC8
        ushort dxc = (ushort)(UInt8[DS, 0x476A] - 1);   // mov dl,[0x476A] (dh=0) ; dec dx
        DX = dxc;
        if ((short)dxc > 0) {                            // jle 3DE5 (skip if signed <= 0)
            ushort ax3 = 0x000F;
            do {
                ax3 = (ushort)(ax3 + 1);                // inc ax
                AX = ax3;
                HashInsertOpenAddr_1000_3DF4_13DF4(0);
                dxc = (ushort)(dxc - 1);                // dec dx
                DX = dxc;
            } while (dxc != 0);                          // jnz 3DDE
        }
        return Tail3DE5();
    }

    /// <summary>Shared tail of cs1:0x3D83 at 3DE5: <c>lodsb; ax&amp;=0xFF; dec ax;
    /// [0x47F6]+=ax; ret</c>.</summary>
    private Action Tail3DE5() {
        byte al = UInt8[DS, SI];
        SI = (ushort)(SI + 1);
        ushort ax = al;                                 // xor ah,ah
        ushort di = UInt16[DS, 0x47F6];
        ax = (ushort)(ax - 1);                          // dec ax
        di = (ushort)(di + ax);
        UInt16[DS, 0x47F6] = di;
        DI = di;
        AX = ax;
        return NearRet();
    }

    // --- Thunk leaves (full-symtab harvest 2026-05-15) ---

    /// <summary>cs1:0x02DE — <c>xor cx,cx; jmp 0x0A44</c>.</summary>
    public Action Thunk_ClrCx_Jmp0A44_1000_02DE_102DE(int gotoAddress) {
        CX = 0;
        return NearJump(0x0A44);
    }

    /// <summary>cs1:0x3950 — <c>mov byte[0x46D7],0; mov si,0x3916; jmp 0xDA5F</c>.</summary>
    public Action Thunk_Clr46D7SetSi3916_JmpDA5F_1000_3950_13950(int gotoAddress) {
        UInt8[DS, 0x46D7] = 0;
        SI = 0x3916;
        return NearJump(0xDA5F);
    }

    /// <summary>cs1:0x3901 — <c>mov si,0x3916; mov bp,0x0010; jmp 0xDA25</c>.</summary>
    public Action Thunk_SetSi3916Bp10_JmpDA25_1000_3901_13901(int gotoAddress) {
        SI = 0x3916;
        BP = 0x0010;
        return NearJump(0xDA25);
    }

    /// <summary>cs1:0xA44C — <c>mov al,[0x28E7]; add al,0x08; jmp 0xA435</c>.</summary>
    public Action Thunk_LoadAdd28E7_JmpA435_1000_A44C_1A44C(int gotoAddress) {
        int r = UInt8[DS, 0x28E7] + 0x08;
        AL = (byte)r;
        CarryFlag = r > 0xFF;
        ZeroFlag = (r & 0xFF) == 0;
        SignFlag = (r & 0x80) != 0;
        return NearJump(0xA435);
    }

    // --- Thunk leaves (cont.) ---
    // Each is `[reg setup;] E9 rel16`. NearJump is the faithful near tail-jump; the
    // target (asm or C#) is dispatched by Spice86 and its ret returns to OUR caller.

    /// <summary>cs1:0x02E0 — <c>jmp 0x0A44</c>.</summary>
    public Action Thunk_Jmp0A44_1000_02E0_102E0(int gotoAddress) => NearJump(0x0A44);

    /// <summary>cs1:0x02F8 — <c>jmp 0x07EE</c>.</summary>
    public Action Thunk_Jmp07EE_1000_02F8_102F8(int gotoAddress) => NearJump(0x07EE);

    /// <summary>cs1:0x02FB — <c>jmp 0x09AD</c>.</summary>
    public Action Thunk_Jmp09AD_1000_02FB_102FB(int gotoAddress) => NearJump(0x09AD);

    /// <summary>cs1:0x02FE — <c>jmp 0x076A</c>.</summary>
    public Action Thunk_Jmp076A_1000_02FE_102FE(int gotoAddress) => NearJump(0x076A);

    /// <summary>cs1:0x1DD4 — <c>jmp 0x20A4</c>.</summary>
    public Action Thunk_Jmp20A4_1000_1DD4_11DD4(int gotoAddress) => NearJump(0x20A4);

    /// <summary>cs1:0x1DD7 — <c>jmp 0x1F64</c>.</summary>
    public Action Thunk_Jmp1F64_1000_1DD7_11DD7(int gotoAddress) => NearJump(0x1F64);

    /// <summary>cs1:0x0737 — <c>mov al,0x56; jmp 0xC2F2</c>.</summary>
    public Action Thunk_SetAl56_JmpC2F2_1000_0737_10737(int gotoAddress) {
        AL = 0x56;
        return NearJump(0xC2F2);
    }

    /// <summary>cs1:0x0788 — <c>mov al,0x07; jmp 0x099D</c>.</summary>
    public Action Thunk_SetAl07_Jmp099D_1000_0788_10788(int gotoAddress) {
        AL = 0x07;
        return NearJump(0x099D);
    }

    /// <summary>cs1:0x0820 — <c>mov ax,0x002E; jmp 0x3978</c>.</summary>
    public Action Thunk_SetAx2E_Jmp3978_1000_0820_10820(int gotoAddress) {
        AX = 0x002E;
        return NearJump(0x3978);
    }

    /// <summary>cs1:0x0A3E — <c>mov si,0x0A16; jmp 0xDA5F</c>.</summary>
    public Action Thunk_SetSi0A16_JmpDA5F_1000_0A3E_10A3E(int gotoAddress) {
        SI = 0x0A16;
        return NearJump(0xDA5F);
    }

    /// <summary>cs1:0x39E6 — <c>mov si,0xC0B6; jmp 0xDA5F</c>.</summary>
    public Action Thunk_SetSiC0B6_JmpDA5F_1000_39E6_139E6(int gotoAddress) {
        SI = 0xC0B6;
        return NearJump(0xDA5F);
    }

    /// <summary>cs1:0x4D00 — <c>mov si,0x4BB9; jmp 0xDA5F</c>.</summary>
    public Action Thunk_SetSi4BB9_JmpDA5F_1000_4D00_14D00(int gotoAddress) {
        SI = 0x4BB9;
        return NearJump(0xDA5F);
    }

    /// <summary>cs1:0x40C3 — <c>mov bp,0x40C9; jmp 0x36EE</c> (bp = continuation ptr).</summary>
    public Action Thunk_SetBp40C9_Jmp36EE_1000_40C3_140C3(int gotoAddress) {
        BP = 0x40C9;
        return NearJump(0x36EE);
    }

    /// <summary>cs1:0x920F — <c>add ax,2; jmp 0xC13E</c> (open_sprite_sheet, asm).</summary>
    public Action Thunk_AddAx2_JmpC13E_1000_920F_1920F(int gotoAddress) {
        AX = Alu16.Add(AX, 0x0002);
        return NearJump(0xC13E);
    }

    /// <summary>
    /// cs1:0x127C — CF-returning predicate. <c>cmp al,4; jnz L; cmp [0x002A],0x15;
    /// jc L; cmp [0x002A],0x20; ret</c> where L = <c>clc; ret</c>. Clean two-exit leaf.
    /// Returns CF=0 unless AL==4 and <c>[0x002A] &gt;= 0x15</c>, in which case CF reflects
    /// <c>[0x002A] &lt; 0x20</c>.
    /// </summary>
    public Action CheckAl4AndByte2ARange_1000_127C_1127C(int gotoAddress) {
        if (AL != 0x04) {
            CarryFlag = false;
            return NearRet();
        }
        byte v = UInt8[DS, 0x002A];
        if (v < 0x15) {
            CarryFlag = false;
            return NearRet();
        }
        int r = (v - 0x20) & 0xFF;
        CarryFlag = v < 0x20;
        ZeroFlag = v == 0x20;
        SignFlag = (r & 0x80) != 0;
        return NearRet();
    }

    /// <summary>
    /// cs1:0xD03C — <c>es:</c> string scan. Skips leading non-digit bytes, then consumes
    /// the run of ASCII digits, leaving SI at the first non-digit after the number
    /// (<c>dec si</c> backs up over the byte that ended the digit run). Clean leaf.
    /// </summary>
    /// <remarks>
    /// <code>
    /// D03C: 26 AC / 2C 30 / 3C 09 / 77 F8   ; while (es:[si++]-0x30) >9u : repeat
    /// D044: 26 AC / 2C 30 / 3C 09 / 76 F8   ; while (es:[si++]-0x30)<=9u : repeat
    /// D04C: 4E / C3                          ; dec si ; ret
    /// </code>
    /// </remarks>
    public Action SkipNonDigitsThenDigitsEsSi_1000_D03C_1D03C(int gotoAddress) {
        byte al;
        do {
            al = UInt8[ES, SI];
            SI = (ushort)(SI + 1);
            al = (byte)(al - 0x30);
        } while (al > 9);          // ja (unsigned >)
        do {
            al = UInt8[ES, SI];
            SI = (ushort)(SI + 1);
            al = (byte)(al - 0x30);
        } while (al <= 9);         // jbe (unsigned <=)
        SI = (ushort)(SI - 1);
        return NearRet();
    }

    /// <summary>
    /// cs1:0xCE01 — zero/sentinel init of four words: <c>[0xDBE8]=0, [0xDBEA]=0,
    /// [0xDBEC]=0xFFFF, [0xDBEE]=0xFFFF</c>. Clean leaf.
    /// </summary>
    public Action InitDBE8Region_1000_CE01_1CE01(int gotoAddress) {
        UInt16[DS, 0xDBE8] = 0;
        UInt16[DS, 0xDBEA] = 0;
        UInt16[DS, 0xDBEC] = 0xFFFF;
        UInt16[DS, 0xDBEE] = 0xFFFF;
        return NearRet();
    }

    /// <summary>
    /// cs1:0xB683 — branchless abs-difference / min-clamp on BP vs DX, selected by the
    /// caller's incoming SF (this routine's first instruction is <c>js</c> on entry
    /// flags). Clean leaf.
    /// </summary>
    /// <remarks>
    /// <code>
    /// B683: 78 06        js   B68B
    /// B685: 2B EA        sub  bp, dx
    /// B687: F7 DD        neg  bp
    /// B689: EB 04        jmp  B68F
    /// B68B: 87 D5        xchg dx, bp
    /// B68D: 03 D5        add  dx, bp
    /// B68F: 8B CD        mov  cx, bp
    /// B691: F7 D9        neg  cx
    /// B693: 3B CA        cmp  cx, dx
    /// B695: 73 02        jnc  B699
    /// B697: 8B D5        mov  dx, bp
    /// B699: C3           ret
    /// </code>
    /// </remarks>
    public Action AbsDiffClampBpDx_1000_B683_1B683(int gotoAddress) {
        if (!SignFlag) {
            // B685: sub bp,dx ; neg bp
            BP = (ushort)(0 - (ushort)(BP - DX));
        } else {
            // B68B: xchg dx,bp ; add dx,bp
            ushort t = DX;
            DX = BP;
            BP = t;
            DX = (ushort)(DX + BP);
        }
        // B68F: cx = bp ; neg cx ; cmp cx,dx ; jnc B699 ; (else) dx = bp
        CX = (ushort)(0 - BP);
        // cmp cx,dx → carry set when cx < dx (unsigned); jnc taken when cx >= dx
        if (CX < DX) {
            DX = BP;
        }
        return NearRet();
    }

    /// <summary>
    /// cs1:0x0CF2 — bit-pack transform of AX/BX/DX:
    /// <c>add al,bl; ror ax,5; shr ah,3; bl=ah; dl=al; xchg bl,bh; xchg dl,dh</c>.
    /// Clean leaf. Flags left as the final <c>shr ah,cl</c> set them.
    /// </summary>
    public Action BitPackTransformAxBxDx_1000_0CF2_10CF2(int gotoAddress) {
        byte al = (byte)(AX & 0xFF);
        byte ah = (byte)(AX >> 8);
        byte bl = (byte)(BX & 0xFF);
        byte bh = (byte)(BX >> 8);
        byte dh = (byte)(DX >> 8);
        // add al,bl
        al = (byte)(al + bl);
        // ror ax,5
        ushort ax = (ushort)((ah << 8) | al);
        ax = (ushort)(((ax >> 5) | (ax << 11)) & 0xFFFF);
        al = (byte)(ax & 0xFF);
        ah = (byte)(ax >> 8);
        // shr ah,3
        bool cf = (ah & 0x04) != 0;     // bit shifted out by the last of 3 shifts
        ah = (byte)(ah >> 3);
        // bl = ah ; dl = al ; xchg bl,bh ; xchg dl,dh
        bl = ah;
        byte dl = al;
        byte tb = bl; bl = bh; bh = tb;
        byte td = dl; dl = dh; dh = td;
        AX = (ushort)((ah << 8) | al);
        BX = (ushort)((bh << 8) | bl);
        DX = (ushort)((dh << 8) | dl);
        ZeroFlag = ah == 0;
        CarryFlag = cf;
        return NearRet();
    }

    /// <summary>cs1:0x4AB8 — <c>C6 06 27 47 FF / C3</c>: <c>mov byte [0x4727],0xFF; ret</c>. Clean leaf.</summary>
    public Action SetFlag4727ToFF_1000_4AB8_14AB8(int gotoAddress) {
        UInt8[DS, 0x4727] = 0xFF;
        return NearRet();
    }

    /// <summary>cs1:0x50BE — <c>C6 06 CB 11 00 / C3</c>: <c>mov byte [0x11CB],0; ret</c>. Clean leaf.</summary>
    public Action Clear11CB_1000_50BE_150BE(int gotoAddress) {
        UInt8[DS, 0x11CB] = 0;
        return NearRet();
    }

    /// <summary>cs1:0xA1C4 — <c>C6 06 A5 47 FF / C3</c>: <c>mov byte [0x47A5],0xFF; ret</c>. Clean leaf.</summary>
    public Action Set47A5ToFF_1000_A1C4_1A1C4(int gotoAddress) {
        UInt8[DS, 0x47A5] = 0xFF;
        return NearRet();
    }

    /// <summary>cs1:0xA5AA — <c>C6 06 BE 28 00 / C3</c>: <c>mov byte [0x28BE],0; ret</c>. Clean leaf.</summary>
    public Action Clear28BE_1000_A5AA_1A5AA(int gotoAddress) {
        UInt8[DS, 0x28BE] = 0;
        return NearRet();
    }

    /// <summary>
    /// cs1:0xA1E2 — <c>80 3E A5 47 FF / C3</c>: <c>cmp byte [0x47A5],0xFF; ret</c>.
    /// Clean leaf; returns flags for the caller (ZF set when <c>[0x47A5]==0xFF</c>).
    /// </summary>
    public Action Cmp47A5WithFF_1000_A1E2_1A1E2(int gotoAddress) {
        byte v = UInt8[DS, 0x47A5];
        int r = (v - 0xFF) & 0xFF;
        ZeroFlag = v == 0xFF;
        CarryFlag = v < 0xFF;
        SignFlag = (r & 0x80) != 0;
        return NearRet();
    }

    /// <summary>
    /// cs1:0xA45C — <c>03 16 86 28 / 03 1E 88 28 / C3</c>:
    /// <c>add dx,[0x2886]; add bx,[0x2888]; ret</c>. Clean leaf; flags reflect the
    /// second <c>add</c> (BX), exactly as the asm leaves them.
    /// </summary>
    public Action AddDx2886AddBx2888_1000_A45C_1A45C(int gotoAddress) {
        DX = Alu16.Add(DX, UInt16[DS, 0x2886]);
        BX = Alu16.Add(BX, UInt16[DS, 0x2888]);
        return NearRet();
    }

    /// <summary>
    /// cs1:0x693B — <c>8A 44 03 / 25 0F 00 / D1 E8 / D1 E8 / C3</c>:
    /// <c>mov al,[si+3]; and ax,0x000F; shr ax,1; shr ax,1; ret</c>. Clean leaf —
    /// returns <c>AX = ([ds:si+3] &amp; 0x0F) &gt;&gt; 2</c>.
    /// </summary>
    public Action LoadNibbleShr2FromSi3_1000_693B_1693B(int gotoAddress) {
        byte al = UInt8[DS, (ushort)(SI + 3)];
        ushort ax = (ushort)(al & 0x0F);
        // shr ax,1 ; shr ax,1 — final CF = bit shifted out of the 2nd shift
        bool cf = (ax & 0x02) != 0;
        ax = (ushort)(ax >> 2);
        AX = ax;
        ZeroFlag = ax == 0;
        CarryFlag = cf;
        return NearRet();
    }

    /// <summary>
    /// cs1:0x3310 — <c>32 E4 / D1 E8 ×4 / 05 D1 00 / C3</c>:
    /// <c>xor ah,ah; shr ax,1 (×4); add ax,0x00D1; ret</c>. Clean leaf —
    /// returns <c>AX = (AL &gt;&gt; 4) + 0x00D1</c>; flags from the final <c>add</c>.
    /// </summary>
    public Action Shr4ThenAdd00D1_1000_3310_13310(int gotoAddress) {
        ushort ax = (ushort)(AL >> 4);   // xor ah,ah then shr ×4
        AX = Alu16.Add(ax, 0x00D1);
        return NearRet();
    }

    public Action CheckPcmEnabled_1000_AE2F_1AE2F(int gotoAddress) {
        ushort value = globalsOnDs.Get1138_DBC8_Word16();
        Alu16.And(value, 1);
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xD443 — <c>DispatcherJumpsToBX</c>. Resolves the handler for
    /// table index 0 via <see cref="DispatcherHelperDeterminesWhereToJump_1000_D454_01D454"/>
    /// (called with CX=0) and near-jumps to it, unless the handler is null (BX=0) or its
    /// high-byte flags carry bit 0x40 (AH &amp; 0x40), in which case it just returns.
    /// </summary>
    /// <remarks>
    /// Asm (17 bytes):
    /// <code>
    /// D443: 33 C9         xor  cx, cx
    /// D445: E8 0C 00      call D454            ; → AX/BX handler pair
    /// D448: 0B DB         or   bx, bx
    /// D44A: 74 07         jz   D453
    /// D44C: F6 C4 40      test ah, 0x40
    /// D44F: 75 02         jnz  D453
    /// D451: FF E3         jmp  bx              ; near indirect tail-jump
    /// D453: C3            ret
    /// </code>
    /// The one call is to the already-ported
    /// <see cref="DispatcherHelperDeterminesWhereToJump_1000_D454_01D454"/> (invoked
    /// directly for AX/BX; its NearRet result is discarded — the asm <c>call</c> simply
    /// returns and execution falls through). <c>jmp bx</c> is a near tail-jump within
    /// cs1 — Spice86 dispatches whatever (asm or C#) is registered at <c>cs1:BX</c>.
    /// </remarks>
    public Action DispatcherJumpsToBX_1000_D443_01D443(int gotoAddress) {
        CX = 0;
        DispatcherHelperDeterminesWhereToJump_1000_D454_01D454(0);
        if (BX == 0) {
            return NearRet();
        }
        if ((AH & 0x40) != 0) {
            return NearRet();
        }
        return NearJump(BX);
    }

    /// <summary>
    /// Override for cs1:0xD454 — <c>DispatcherHelperDeterminesWhereToJump</c>. Walks a
    /// variable-stride record table (head pointer at <c>ds[0x21DA]</c>) to the entry
    /// selected by CL, returning the 2-word handler pair in AX/BX. When CL equals the
    /// sentinel <c>ds[0xDCE5]</c> it instead returns a fixed AX=0x00A0 and one of three
    /// BX values chosen by the sign of <c>ds[0xDCE4]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (two ret exits, D454..D489):
    /// <code>
    /// D454: 8B 36 DA 21      mov  si, [0x21DA]
    /// D458: 8B 34            mov  si, [si]
    /// D45A: 46               inc  si
    /// D45B: 32 ED            xor  ch, ch
    /// D45D: 3A 0E E5 DC      cmp  cl, [0xDCE5]
    /// D461: 74 12            je   D475
    /// D463: AC               lodsb
    /// D464: 98               cbw                  ; sign-extend AL → AX
    /// D465: 03 F0            add  si, ax
    /// D467: 8B C1            mov  ax, cx
    /// D469: D1 E0            shl  ax, 1
    /// D46B: D1 E0            shl  ax, 1           ; ax = cx*4
    /// D46D: 03 F0            add  si, ax
    /// D46F: 8B 04            mov  ax, [si]
    /// D471: 8B 5C 02         mov  bx, [si+2]
    /// D474: C3               ret
    /// D475: B8 A0 00         mov  ax, 0x00A0
    /// D478: BB 23 D4         mov  bx, 0xD423
    /// D47B: 80 3E E4 DC 00   cmp  byte [0xDCE4], 0
    /// D480: 78 07            js   D489            ; [DCE4] &lt; 0  → bx = 0xD423
    /// D482: BB 29 D4         mov  bx, 0xD429
    /// D485: 7F 02            jg   D489            ; [DCE4] &gt; 0  → bx = 0xD429
    /// D487: 33 DB            xor  bx, bx          ; [DCE4] == 0 → bx = 0
    /// D489: C3               ret
    /// </code>
    /// Pure leaf (two ret exits, no calls/hardware). All memory reads are DS-relative.
    /// CH is cleared so the table index is the unsigned byte CL; the leading byte of
    /// each record is a signed skip applied before the <c>cx*4</c> stride.
    /// </remarks>
    public Action DispatcherHelperDeterminesWhereToJump_1000_D454_01D454(int gotoAddress) {
        ushort si = UInt16[DS, 0x21DA];
        si = UInt16[DS, si];
        si = (ushort)(si + 1);
        // xor ch,ch — index is the unsigned byte CL
        CX = (ushort)(CX & 0x00FF);
        byte cl = (byte)(CX & 0xFF);
        if (cl == UInt8[DS, 0xDCE5]) {
            SI = si;
            AX = 0x00A0;
            sbyte dce4 = (sbyte)UInt8[DS, 0xDCE4];
            BX = dce4 < 0 ? (ushort)0xD423 : dce4 > 0 ? (ushort)0xD429 : (ushort)0;
            return NearRet();
        }
        // lodsb; cbw; add si,ax (signed skip)
        byte b = UInt8[DS, si];
        si = (ushort)(si + 1);
        si = (ushort)(si + (ushort)(short)(sbyte)b);
        // ax = cx; shl ax,1; shl ax,1 (cx*4); add si,ax
        ushort ax = CX;
        ax = (ushort)(ax << 2);
        si = (ushort)(si + ax);
        SI = si;
        AX = UInt16[DS, si];
        BX = UInt16[DS, (ushort)(si + 2)];
        return NearRet();
    }

    public Action SetCEE8To0_1000_DE4E_01DE4E(int gotoAddress) {
        // Called when skipping some intro screens
        globalsOnDs.Set1138_CEE8_Byte8_keyHit(0);
        return NearRet();
    }

    public Action Inc2788_1000_B2B9_01B2B9(int gotoAddress) {
        // Called when looking at miror or at book, value seems to be always 0 at call time.
        byte value = globalsOnDs.Get1138_2788_Byte8();
        globalsOnDs.Set1138_2788_Byte8((byte)(value + 1));
        return NearRet();
    }

    public Action Fill47F8WithFF_1000_3AE9_013AE9(int gotoAddress) {
        LogSceneBoundaryEntry();
        // Called when leaving or entering a scene. Does not seem to have any effect on game whatever the value is in this
        // area.
        uint address = MemoryUtils.ToPhysicalAddress(DS, 0x47F8);
        Memory.Memset8(address, 0xFF, 2 * 0x2E);
        return NearRet();
    }

    public Action NoOp_1000_0F66_10F66(int gotoAddress) {
        // called before intro
        return NearRet();
    }

    public Action SetUnknown11CATo0_1000_4AC4_14AC4(int gotoAddress) {
        // triggered when orni lifts off and lands
        globalsOnDs.Set1138_11CA_Byte8(0);
        return NearRet();
    }

    public Action SetUnknown11CATo1_1000_4ACA_14ACA(int gotoAddress) {
        // triggered on orni map, flat map and discussion when displaying new dialogue on click and play screens and in
        // visions
        globalsOnDs.Set1138_11CA_Byte8(1);
        return NearRet();
    }

    public Action MemCopy8BytesDsSIToDsDi_1000_5B99_15B99(int gotoAddress) {
        // Called on scene change (example dialogue, room change)
        ES = DS;
        uint sourceAddress = MemoryUtils.ToPhysicalAddress(DS, SI);
        uint destinationAddress = MemoryUtils.ToPhysicalAddress(ES, DI);

        // Moves 4 words from source to dest, so 8 bytes
        Memory.MemCopy(sourceAddress, destinationAddress, 8);
        SI = (ushort)(SI + 8);
        DI = (ushort)(DI + 8);
        return NearRet();
    }

    public Action MemCopy8BytesFrom1470ToD83C_1000_5BA0_15BA0(int gotoAddress) {
        // Called on room change
        SI = 0x1470;
        DI = 0xD83C;
        return MemCopy8BytesDsSIToDsDi_1000_5B99_15B99(0);
    }

    public Action MemCopy8Bytes_1000_5BA8_15BA8(int gotoAddress) {
        // Called on dialogue, screen change, intro demo and globe
        SI = 0x1470;
        DI = 0xD834;
        return MemCopy8BytesDsSIToDsDi_1000_5B99_15B99(0);
    }

    public Action IsUnknownDBC80x100And2943BitmaskNonZero_1000_AEC6_1AEC6(int gotoAddress) {
        // Called continuously
        int value = globalsOnDs.Get1138_2943_Byte8_cmdArgsMemory();
        bool res = true;
        if ((value & 0x10) == 0) {
            IsUnknownDBC80x100_1000_AE28_1AE28(0);
            if (!ZeroFlag) {
                res = false;
            }
        }

        _loggerService.Debug("2943={@Value},res={@Res}", value, res);
        CarryFlag = res;
        if (res) {
            throw FailAsUntested($"isUnknownDBC80x100And2943BitmaskNonZero was called with a true result. value: {value}");
        }

        return NearRet();
    }

    public Action IsUnknownDC2BZero_1000_ABCC_1ABCC(int gotoAddress) {
        ZeroFlag = globalsOnDs.Get1138_DC2B_Byte8() == 0;
        return NearRet();
    }

    public Action IsUnknownDBC80x100_1000_AE28_1AE28(int gotoAddress) {
        // Called constantly in game and at transitions during video
        ushort value = globalsOnDs.Get1138_DBC8_Word16();

        // Seems that this function is called with only JZ / JNZ, but not sure so call the real thing
        Alu16.Sub(value, 0x100);
        return NearRet();
    }

    public Action SetUnknown2788To0_1000_B2BE_1B2BE(int gotoAddress) {
        // Called when game is loaded or when landing with orni. Other values do not seem to have any effect.
        globalsOnDs.Set1138_2788_Byte8(0);
        return NearRet();
    }

    public Action NoOp_1000_D917_01D917(int gotoAddress) {
        // called after first globe display
        return NearRet();
    }

    public Action ShlDXAndCXByAX_1000_DB44_01DB44(int gotoAddress) {
        // Called before setting mouse parameters
        ushort shiftCount = AX;
        CX = (ushort)(CX << shiftCount);
        DX = (ushort)(DX << shiftCount);
        return NearRet();
    }

    public Action NoOp_1000_E26F_01E26F(int gotoAddress) {
        // called after or during most screen transitions
        return NearRet();
    }

    public Action UnknownStructCreation_1000_E75B_01E75B(int gotoAddress) {
        uint destinationAddress = MemoryUtils.ToPhysicalAddress(ES, DI);
        Memory.UInt16[destinationAddress] = AX;
        Memory.UInt8[destinationAddress + 2] = DL;
        uint sourceAddress = MemoryUtils.ToPhysicalAddress(DS, SI) + 0x10;
        Memory.MemCopy(sourceAddress, destinationAddress + 3, 3);
        Memory.MemCopy(sourceAddress + 4, destinationAddress + 6, 4);

        // 10 bytes copied in total
        DI = (ushort)(DI + 10);
        return NearRet();
    }

    public Action CheckNextFreeMemorySegment39B9_1000_E851_01E851(int gotoAddress) {
        // Game stops if carry flag is unset
        ushort value = globalsOnDs.Get1138_39B9_Word16_allocatorNextFreeSegment();
        value += 0x2F13;
        Alu16.Sub(value, globalsOnDs.Get1138_CE68_Word16_allocatorLastFreeSegment());
        return NearRet();
    }
}