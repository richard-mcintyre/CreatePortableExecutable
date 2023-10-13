using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CreatePE.PE;

[StructLayout(LayoutKind.Explicit)]
internal struct ImageFileHeader
{
    /// <summary>
    /// The architecture type of the computer. 
    /// An image file can only be run on the specified computer or a system that emulates the specified computer
    /// </summary>
    [FieldOffset(0)]
    public Machine Machine;

    /// <summary>
    /// The number of sections. This indicates the size of the section table, which immediately follows the headers. 
    /// Note that the Windows loader limits the number of sections to 96.
    /// </summary>
    [FieldOffset(2)]
    public ushort NumberOfSections;

    /// <summary>
    /// The low 32 bits of the time stamp of the image. 
    /// This represents the date and time the image was created by the linker. 
    /// The value is represented in the number of seconds elapsed since midnight (00:00:00), January 1, 1970 UTC, according to the system clock.
    /// </summary>
    [FieldOffset(4)]
    public uint TimeDateStamp;

    /// <summary>
    /// The offset of the symbol table, in bytes, or zero if no COFF symbol table exists.
    /// </summary>
    [FieldOffset(8)]
    public uint PointerToSymbolTable;

    /// <summary>
    /// The number of symbols in the symbol table.
    /// </summary>
    [FieldOffset(12)]
    public uint NumberOfSymbols;

    /// <summary>
    /// The size of the optional header, in bytes. This value should be 0 for object files.
    /// </summary>
    [FieldOffset(16)]
    public ushort SizeOfOptionalHeader;

    /// <summary>
    /// The characteristics of the image.
    /// </summary>
    [FieldOffset(18)]
    public Characteristics Characteristics;
}
