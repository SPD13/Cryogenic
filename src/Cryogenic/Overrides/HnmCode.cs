namespace Cryogenic.Overrides;

using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Structures;

using System;
using System.IO;

/// <summary>
/// Partial class containing HNM video file format handling overrides.
/// </summary>
/// <remarks>
/// <para>
/// HNM is a proprietary video format used by Cryo Interactive for full-motion video sequences.
/// This file provides overrides for reading HNM file data from disk into memory buffers
/// for decoding and playback.
/// </para>
/// <para>
/// Method names contain underscores to separate segment, offset, and linear addresses
/// for traceability back to the original DOS disassembly.
/// </para>
/// </remarks>
public partial class Overrides {

    /// <summary>
    /// Registers HNM video file handling function overrides with Spice86.
    /// </summary>
    public void DefineHnmCodeOverrides() {
        DefineFunction(cs1, 0xCDBF, HnmReadFromFileHandle_1000_CDBF_01CDBF);
        DefineFunction(cs1, 0xCE1A, HnmReset_1000_CE1A_01CE1A);
        DefineFunction(cs1, 0xCD8F, HnmReadHeaderSize_1000_CD8F_01CD8F);
        DefineFunction(cs1, 0xCDA0, HnmPrepareHeaderRead_1000_CDA0_01CDA0);
    }

    /// <summary>
    /// Override for cs1:0xCDA0 — <c>hnm_prepare_header_read_ida</c>
    /// (<c>sub_EC70</c>). Chain-port over already-C# pieces:
    /// <see cref="HnmReset_1000_CE1A_01CE1A"/> (sub_ECEA) →
    /// <see cref="HnmReadHeaderSize_1000_CD8F_01CD8F"/> (sub_EC5F); on its
    /// short-read CF it near-returns (the raw-asm <c>retn</c> at
    /// <c>cs1:0xCE00</c>); else it computes the decode-buffer pointers
    /// (<c>[0xDC10]</c>/<c>[0xDC0C]</c>), stores the header word via
    /// <c>stosw</c>, sets <c>CX</c>=size-2, and falls through into
    /// <see cref="HnmReadFromFileHandle_1000_CDBF_01CDBF"/> (<c>cs1:0xCDBF</c>).
    /// Pure orchestration over C# + compute — no INT/port/far/lds — exact port.
    /// </summary>
    /// <remarks>
    /// Asm (cs1:0xCDA0..0xCDBE), cross-verified against the cs1 dump:
    /// <code>
    /// call sub_ECEA(0xCE1A) ; call sub_EC5F(0xCD8F) ; jb locret_ECD0(0xCE00=retn)
    /// mov di,[0xCE74] ; sub di,ax ; sub di,2
    /// mov [0xDC10],di ; stosw ; mov [0xDC0C],di
    /// mov cx,ax ; sub cx,2
    /// (fall through into HnmReadFromFileHandle @0xCDBF)
    /// </code>
    /// </remarks>
    public Action HnmPrepareHeaderRead_1000_CDA0_01CDA0(int gotoAddress) {
        HnmReset_1000_CE1A_01CE1A(0);                       // call sub_ECEA (0xCE1A)
        HnmReadHeaderSize_1000_CD8F_01CD8F(0);              // call sub_EC5F (0xCD8F)
        if (CarryFlag) {                                    // jb locret_ECD0 (0xCE00 = retn)
            return NearRet();
        }
        ushort di = (ushort)(UInt16[DS, 0xCE74] - AX - 2); // mov di,[0xCE74]; sub di,ax; sub di,2
        UInt16[DS, 0xDC10] = di;                            // mov [0xDC10],di
        UInt16[ES, di] = AX;                                // stosw (ES:[DI]=AX)
        di = (ushort)(di + 2);                              //   DI += 2
        UInt16[DS, 0xDC0C] = di;                            // mov [0xDC0C],di
        DI = di;
        CX = (ushort)(AX - 2);                              // mov cx,ax; sub cx,2
        return NearJump(0xCDBF);                             // fall through -> HnmReadFromFileHandle (C#)
    }

    /// <summary>
    /// Override for cs1:0xCD8F — <c>hnm_read_header_size_ida</c>. Reads the 2-byte HNM
    /// header size into the decode buffer via
    /// <see cref="HnmReadFromFileHandle_1000_CDBF_01CDBF"/> (CX=2), then returns
    /// <c>AX = es:[si-2]</c> (the word just read) where <c>es:si</c> comes from the far
    /// pointer at <c>ds:[0xDC0C]</c>.
    /// </summary>
    /// <remarks>
    /// Asm (17 bytes):
    /// <code>
    /// CD8F: B9 02 00      mov cx, 0x0002
    /// CD92: E8 2A 00      call CDBF            ; HnmReadFromFileHandle
    /// CD95: 72 08         jc  CD9F             ; (error path)
    /// CD97: C4 36 0C DC   les si, [0xDC0C]
    /// CD9B: 26 8B 44 FE   mov ax, es:[si-2]
    /// CD9F: C3            ret
    /// </code>
    /// The only call is the already-ported C# <c>0xCDBF</c>, which models
    /// "read succeeds or throws" (no CF-error return), so the asm <c>jc</c> error path
    /// is unreachable in the C# model and the success path is taken unconditionally.
    /// </remarks>
    public Action HnmReadHeaderSize_1000_CD8F_01CD8F(int gotoAddress) {
        CX = 0x0002;
        HnmReadFromFileHandle_1000_CDBF_01CDBF(0);
        ushort si = UInt16[DS, 0xDC0C];
        ES = UInt16[DS, 0xDC0E];
        SI = si;
        AX = UInt16[ES, (ushort)(si - 2)];
        return NearRet();
    }

    /// <summary>
    /// Override for cs1:0xCE1A — <c>hnm_reset_ida</c>. Resets the HNM decoder state block
    /// at <c>ds:0xDC0C..0xDC20</c>.
    /// </summary>
    /// <remarks>
    /// Asm (33 bytes):
    /// <code>
    /// CE1A: A1 DE DB     mov ax, [0xDBDE]
    /// CE1D: A3 0E DC     mov [0xDC0E], ax
    /// CE20: A3 12 DC     mov [0xDC12], ax
    /// CE23: 33 C0        xor ax, ax
    /// CE25: A3 0C DC     mov [0xDC0C], ax
    /// CE28: A3 10 DC     mov [0xDC10], ax
    /// CE2B: A3 1A DC     mov [0xDC1A], ax
    /// CE2E: A3 20 DC     mov [0xDC20], ax
    /// CE31: A3 16 DC     mov [0xDC16], ax
    /// CE34: A1 74 CE     mov ax, [0xCE74]
    /// CE37: A3 18 DC     mov [0xDC18], ax
    /// CE3A: C3           ret
    /// </code>
    /// Pure leaf. Seeds <c>[0xDC0E]</c> / <c>[0xDC12]</c> with the active framebuffer
    /// (<c>[0xDBDE]</c>), zeroes five decoder cursors, and copies the default chunk
    /// pointer from <c>[0xCE74]</c> into <c>[0xDC18]</c>.
    /// </remarks>
    public Action HnmReset_1000_CE1A_01CE1A(int gotoAddress) {
        ushort dbde = UInt16[DS, 0xDBDE];
        AX = dbde;
        UInt16[DS, 0xDC0E] = dbde;
        UInt16[DS, 0xDC12] = dbde;
        AX = 0;
        UInt16[DS, 0xDC0C] = 0;
        UInt16[DS, 0xDC10] = 0;
        UInt16[DS, 0xDC1A] = 0;
        UInt16[DS, 0xDC20] = 0;
        UInt16[DS, 0xDC16] = 0;
        ushort ce74 = UInt16[DS, 0xCE74];
        AX = ce74;
        UInt16[DS, 0xDC18] = ce74;
        return NearRet();
    }

    /// <summary>
    /// Override for CS1:CDBF - Reads data from an open HNM video file into a memory buffer.
    /// </summary>
    /// <param name="gotoAddress">Target address for potential jumps (unused in this override).</param>
    /// <returns>A near return action to exit the function.</returns>
    /// <exception cref="UnhandledOperationException">
    /// Thrown if the actual bytes read doesn't match the requested amount, indicating an untested code path
    /// where the original DOS code would loop to retry the read.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Performs a sequential read from the HNM file using the DOS file manager. Updates global
    /// state tracking file position, remaining bytes, and buffer pointers.
    /// </para>
    /// <para>
    /// The function reads CX bytes from the file at the current offset into the target buffer,
    /// then advances all related pointers and counters.
    /// </para>
    /// </remarks>
    public Action HnmReadFromFileHandle_1000_CDBF_01CDBF(int gotoAddress) {
        DosFileManager dosFileManager = Machine.Dos.FileManager;
        ushort fileHandle = globalsOnDs.Get1138_35A6_Word16_IsAnimateMenuUnneeded();
        if (fileHandle == 0) {
            return NearRet();
        }

        ushort readLength = CX;
        uint offset = globalsOnDs.Get1138_DC04_DWord32_hnmFileOffset();
        uint targetMemory = globalsOnDs.GetPtr1138_DC0C_Dword32_hnmFileReadBufferSegment().Linear;
        _loggerService.Debug("Read {@ReadLength} bytes from hnm file handle {@FileHandle} at offset {@Offset}", readLength, fileHandle, offset);
        dosFileManager.MoveFilePointerUsingHandle(SeekOrigin.Begin, fileHandle, (int)offset);
        DosFileOperationResult result = dosFileManager.ReadFileOrDevice(fileHandle, readLength, targetMemory);
        uint? actualReadLength = result.Value;
        if (actualReadLength != readLength) {
            throw this.FailAsUntested("The original code loops here when read bytes from hnm are not as expected.");
        }
        globalsOnDs.Set1138_DC08_DWord32_hnmFileRemain(globalsOnDs.Get1138_DC08_DWord32_hnmFileRemain() - actualReadLength.Value);
        globalsOnDs.Set1138_DC04_DWord32_hnmFileOffset(offset + actualReadLength.Value);
        globalsOnDs.Set1138_DC0C_Word16_hnmFileReadBufferSegment((ushort)(globalsOnDs.Get1138_DC0C_Word16_hnmFileReadBufferSegment() + actualReadLength.Value));
        globalsOnDs.Set1138_DC1A_Word16((ushort)(globalsOnDs.Get1138_DC1A_Word16() + actualReadLength.Value));
        return NearRet();
    }
}