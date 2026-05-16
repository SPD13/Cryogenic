namespace Cryogenic.Overrides;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Partial class containing the inner-expression script VM (CONDIT.HSQ evaluator) at cs1:0xA396.
/// </summary>
/// <remarks>
/// <para>
/// Ports the dispatcher documented in Rebuild/DOCUMENTATION/Tech/16-script-vm.md §2.
/// The dispatcher evaluates a single CONDIT-style boolean/arithmetic expression over
/// 256 zero-page game-state variables (DS:0..0xFF) and returns the 16-bit result in DX
/// with ZF set when DX == 0.
/// </para>
/// <para>
/// Resource layout: DS:[0xAA72] is a far pointer to the START of the offset table at the
/// tail of the loaded CONDIT.HSQ resource. The expression's start offset is fetched from
/// ES:[offsetTableStart + action_index*2 - 2]; the expression bytes follow at ES:SI from
/// that offset onward.
/// </para>
/// <para>
/// The asm helpers operand_source_A30B (operand decoder) and action_handler_A334
/// (verb dispatch) have no external callers — they are only invoked from within
/// dispatcher_A396 itself — so they are inlined as private C# helpers below rather
/// than registered as separate overrides.
/// </para>
/// </remarks>
public partial class Overrides {
    /// <summary>
    /// Registers script-VM function overrides with Spice86.
    /// </summary>
    public void DefineScriptVmCodeOverrides() {
        DefineFunction(cs1, 0xA396, ScriptVmDispatcher_1000_A396_01A396);
    }

    /// <summary>
    /// Override for CS1:A396 — dispatcher_A396, the inner-expression VM.
    /// </summary>
    /// <param name="gotoAddress">Unused.</param>
    /// <returns>NearRet — matches the original `ret` at cs1:0xA3E7.</returns>
    /// <remarks>
    /// Entry contract: AX = action_index (1-based index into CONDIT.HSQ offset table).
    /// Exit contract: DX = expression result; ZF = (DX == 0); SI = first byte after
    /// the expression's terminator; ES = CONDIT resource segment; AX, BX, BP, SP follow
    /// the asm's observable side effects exactly (see code comments).
    /// </remarks>
    public Action ScriptVmDispatcher_1000_A396_01A396(int gotoAddress) {
        // A396: sub sp, 0x32 — allocate 50-byte scratch frame
        SP = (ushort)(SP - 0x32);
        // A399: mov bp, sp
        ushort frameStart = SP;
        ushort frameTop = frameStart;
        BP = frameStart;

        // A39B: shl ax, 1
        ushort actionIndex2 = (ushort)(AX << 1);

        // A39D: les si, [0xAA72] — load far pointer to CONDIT offset-table start
        SegmentedAddress conditPtr = globalsOnDs.GetPtr1138_AA72_Dword32_resConditOffset();
        ES = conditPtr.Segment;
        SI = (ushort)(conditPtr.Offset + actionIndex2);
        // A3A3: mov si, [es:si - 2] — fetch this entry's offset from the table
        SI = UInt16[ES, (ushort)(SI - 2)];

        // A3A7: call A30B — first operand → AX
        ushort ax = ReadScriptOperand();
        // A3AA: mov dx, ax
        ushort dx = ax;

        while (true) {
            // A3AC: es lodsb — read next opcode byte at ES:SI
            byte b = UInt8[ES, SI];
            SI = (ushort)(SI + 1);
            ax = (ushort)((ax & 0xFF00) | b); // AL = b; AH preserved

            // A3AE: cmp al, 0xFF; jz A3CB
            if (b == 0xFF) {
                break;
            }

            // A3B2: test al, 0x80; jnz A3C0 — marker path
            if ((b & 0x80) != 0) {
                // A3C0: mov [bp+0], dx
                UInt16[SS, frameTop] = dx;
                // A3C3: mov [bp+2], ax
                UInt16[SS, (ushort)(frameTop + 2)] = ax;
                // A3C6: add bp, 4
                frameTop = (ushort)(frameTop + 4);
                BP = frameTop;
                // A3C9: jmp A3A7 — read fresh first operand for new sub-expression
                ax = ReadScriptOperand();
                dx = ax;
                continue;
            }

            // A3B6: mov bl, al — normal verb path; verb in BL
            byte verb = b;
            // A3B8: call A30B — second operand → AX
            ax = ReadScriptOperand();
            // A3BB: call A334 — DX = action(DX, verb, AX)
            dx = ApplyScriptVerb(dx, verb, ax);
            // A3BE: jmp A3AC
        }

        // A3CB: mov si, sp — walk frame from bottom to top
        ushort si = frameStart;
        // A3CD: cmp si, bp; jz A3E2 — skip resolve when no markers
        if (si != frameTop) {
            // A3D1: mov [bp+0], dx — stash current DX at top of frame (used as final rhs)
            UInt16[SS, frameTop] = dx;

            // A3D4: lodsw → AX = oldest saved DX; mov dx, ax
            ax = UInt16[SS, si];
            si = (ushort)(si + 2);
            dx = ax;

            // Resolve loop A3D7..A3E0
            while (true) {
                // A3D7: lodsw → AX = saved AX (marker byte in low byte); mov bx, ax
                ushort markerAx = UInt16[SS, si];
                si = (ushort)(si + 2);
                BX = markerAx;
                // A3DA: lodsw → AX = next saved DX (or current DX on last iter)
                ax = UInt16[SS, si];
                si = (ushort)(si + 2);
                // A3DB: call A334 — apply the marker's deferred verb (low byte of BX)
                dx = ApplyScriptVerb(dx, (byte)markerAx, ax);

                // A3DE: cmp si, bp; jc A3D7 — loop while si < frameTop (unsigned)
                if (si >= frameTop) {
                    break;
                }
            }
        }

        // A3E2: add sp, 0x32
        SP = (ushort)(SP + 0x32);
        // BP at exit = frameTop (the marker stack top, or frameStart if no markers).
        // This matches asm semantics: BP is not restored to caller-value.
        BP = frameTop;
        // AX at exit:
        // - 0 markers: AL=0xFF (from lodsb), AH preserved from prior operand path
        // - ≥1 markers: last lodsw value (which is the current DX written at A3D1)
        AX = ax;
        // DX = expression result
        DX = dx;
        // A3E5: or dx, dx — set ZF/SF, clear CF/OF
        Alu16.Or(dx, dx);

        return NearRet();
    }

    /// <summary>
    /// Reads one operand from ES:SI, advancing SI. Mirrors operand_source_A30B (cs1:0xA30B).
    /// </summary>
    /// <returns>The 16-bit operand value (zero-extended for byte ops).</returns>
    /// <remarks>
    /// Tag encodings (Tech/16 §2):
    ///   0x00..0x7F + addr byte : DS-zero-page word (or byte if tag==1) fetch
    ///   0x80 + byte           : byte literal (zero-extended)
    ///   0x81..0xFF + word LE  : 16-bit literal
    ///
    /// Side effects on AH (observed via the asm's xor ah,ah / lodsw paths):
    ///   tag 0x00 / 0x01 / 0x80 → AH = 0
    ///   tag 0x81..0xFF        → AH = high byte of the loaded word
    /// We update AX (this.AX) to match these semantics so the marker push site sees
    /// the exact same AX the asm would have observed.
    /// </remarks>
    private ushort ReadScriptOperand() {
        // A30B: es lodsb — AL = tag
        byte tag = UInt8[ES, SI];
        SI = (ushort)(SI + 1);
        ushort result;

        if (tag < 0x80) {
            // A311..A329 — zero-page memory read
            // mov bl, [es:si]; inc si; xor bh, bh
            byte addr = UInt8[ES, SI];
            SI = (ushort)(SI + 1);
            if (tag == 0x01) {
                // A322: byte fetch — mov al, [bx]; xor ah, ah
                byte val = UInt8[DS, addr];
                result = val;
            } else {
                // A31C: word fetch — mov ax, [bx]
                result = UInt16[DS, addr];
            }
        } else if (tag == 0x80) {
            // A32C: byte literal — es lodsb; xor ah, ah
            byte val = UInt8[ES, SI];
            SI = (ushort)(SI + 1);
            result = val;
        } else {
            // A331: word literal — es lodsw
            result = UInt16[ES, SI];
            SI = (ushort)(SI + 2);
        }

        return result;
    }

    /// <summary>
    /// Applies one verb to (lhs, rhs). Mirrors action_handler_A334 (cs1:0xA334).
    /// </summary>
    /// <param name="lhs">DX at call site.</param>
    /// <param name="verb">AL/BL at call site; only low 5 bits select the op.</param>
    /// <param name="rhs">AX at call site.</param>
    /// <returns>New DX (the asm result-in-DX contract).</returns>
    /// <remarks>
    /// <para>
    /// Handler table at cs1:0xA376 (byte-indexed; only even slots are reachable since
    /// the dispatcher uses `bx = verb & 0x1F` directly as a byte offset into a word
    /// table — odd values would land mid-word and crash. The shipped CONDIT data uses
    /// only even values.).
    /// </para>
    /// <para>
    /// Note: ndisasm shows the asm `cmp dx, ax; jng A372` for slot 0x08 and
    /// `cmp dx, ax; jnl A372` for slot 0x0A. These actually compute (signed) DX ≤ AX
    /// and (signed) DX ≥ AX respectively — Tech/16's "&gt;s" / "&lt;s" labels are
    /// inverted. The shipped CONDIT data uses neither, so the inversion is invisible
    /// at runtime; we still port the asm's actual behaviour.
    /// </para>
    /// </remarks>
    private static ushort ApplyScriptVerb(ushort lhs, byte verb, ushort rhs) {
        const ushort TRUE_VALUE = 0xFFFF;
        // and bx, 0x1F — verb selector is the low 5 bits
        switch (verb & 0x1F) {
            case 0x00: return lhs == rhs ? TRUE_VALUE : (ushort)0;              // ==
            case 0x02: return lhs < rhs ? TRUE_VALUE : (ushort)0;               // <u
            case 0x04: return lhs > rhs ? TRUE_VALUE : (ushort)0;               // >u
            case 0x06: return lhs != rhs ? TRUE_VALUE : (ushort)0;              // !=
            case 0x08: return (short)lhs <= (short)rhs ? TRUE_VALUE : (ushort)0; // <=s (asm jng → true)
            case 0x0A: return (short)lhs >= (short)rhs ? TRUE_VALUE : (ushort)0; // >=s (asm jnl → true)
            case 0x0C: return (ushort)(lhs + rhs);                              // +
            case 0x0E: return (ushort)(lhs - rhs);                              // -
            case 0x10: return (ushort)(lhs & rhs);                              // &
            case 0x12: return (ushort)(lhs | rhs);                              // |
            default: return 0;                                                  // unused slots → A36F (xor dx,dx)
        }
    }
}
