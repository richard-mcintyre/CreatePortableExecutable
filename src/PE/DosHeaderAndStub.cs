using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CreatePE.PE;

internal static class DosHeaderAndStub
{
    #region Construction

    static DosHeaderAndStub()
    {
        _stub = new byte[]
        {
            0x4d, 0x5a,         // e_magic          ; MZ
            0x90, 0x00,         // e_cblp           ; Bytes on last page of file
            0x03, 0x00,         // e_cp             ; Pages in file (a page is 16 bytes)
            0x00, 0x00,         // e_crlc           ; Number of relocations
            0x04, 0x00,         // e_cparhdr        ; Size of header in paragraphs
            0x00, 0x00,         // e_minalloc       ; Minimum extra paragraphs needed
            0xff, 0xff,         // e_maxalloc       ; Maximum extra paragraphs needed
            0x00, 0x00,         // e_ss             ; Initial (relative) SS value
            0xb8, 0x00,         // e_sp             ; Initialize SP value
            0x00, 0x00,         // e_csum           ; Checksum
            0x00, 0x00,         // e_ip             ; Initial IP value
            0x00, 0x00,         // e_cs             ; Initial (relative) CS value
            0x40, 0x00,         // e_lfarlc         ; File address of relocation table (in this case start of DOS program)
            0x00, 0x00,         // e_ovno           ; Overlay number
            0x00, 0x00,         // e_res1[0]
            0x00, 0x00,         // e_res1[1]
            0x00, 0x00,         // e_res1[2]
            0x00, 0x00,         // e_res1[3]
            0x00, 0x00,         // e_oemid          ; OEM identifier (for e_oeminfo)
            0x00, 0x00,         // e_oeminfo        ; OEM information; e_oemid specific
            0x00, 0x00,         // e_res2[0]
            0x00, 0x00,         // e_res2[1]
            0x00, 0x00,         // e_res2[2]
            0x00, 0x00,         // e_res2[3]
            0x00, 0x00,         // e_res2[4]
            0x00, 0x00,         // e_res2[5]
            0x00, 0x00,         // e_res2[6]
            0x00, 0x00,         // e_res2[7]
            0x00, 0x00,         // e_res2[8]
            0x00, 0x00,         // e_res2[9]
            0x00, 0x00, 0x00, 0x00,     // e_lfanew     ; File offset to IMAGE_NT_HAEDERS

            0x0e,               // push cs
            0x1f,               // pop ds           ; ds = cs
            0xba, 0x0e, 0x00,   // mov dx, 0eh      ; Location of string
            0xb4, 0x09,         // mov ah, 09h      ; Write string at ds:dx to standard output, string is terminated by '$' character
            0xcd, 0x21,         // int 21h
            0xb8, 0x01, 0x4c,   // mov ax, 4c01h    ; 4c = terminate with return code (in al)
            0xcd, 0x21,         // int 21h
        };

        _message = Encoding.ASCII.GetBytes("This program cannot be run in DOS mode.\r\r\n$");

        // Create the bytes to pad to the next 8 byte boundary (the structure following this (IMAGE_NT_HEADERS)
        // is required to be on an 8 byte boundary)
        uint totalLength = (uint)(_stub.Length + _message.Length);
        _padding = new byte[totalLength.RoundUpToAlignment(8) - totalLength];

        // e_lfanew needs to be the offset to IMAGE_NT_HEADERS
        Span<byte> e_lfanew = _stub.AsSpan(60, 4);   // location of e_lfanew
        uint addr = (uint)(totalLength + _padding.Length);
        MemoryMarshal.Write(e_lfanew, ref addr);
    }

    #endregion

    #region Fields

    private static readonly byte[] _stub;
    private static readonly byte[] _message;
    private static readonly byte[] _padding;

    #endregion

    #region Properties

    public static int Size => _stub.Length + _message.Length + _padding.Length;

    #endregion

    #region Methods

    public static int WriteTo(ref Span<byte> buffer)
    {
        _stub.CopyTo(buffer);

        Span<byte> next = buffer.Slice(_stub.Length);
        _message.CopyTo(next);

        next = next.Slice(_message.Length);
        _padding.CopyTo(next);

        return _stub.Length + _message.Length + _padding.Length;  // number of bytes written to buffer
    }

    public static int WriteTo(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[DosHeaderAndStub.Size];
        int result = WriteTo(ref buffer);
        stream.Write(buffer);

        return result;
    }

    #endregion
}
