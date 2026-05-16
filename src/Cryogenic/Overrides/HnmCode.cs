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