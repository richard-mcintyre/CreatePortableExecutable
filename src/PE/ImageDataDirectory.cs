using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CreatePE.PE;

[StructLayout(LayoutKind.Explicit)]
internal struct ImageDataDirectory
{
    /// <summary>
    /// The relative virtual address of the table.
    /// </summary>
    [FieldOffset(0)]
    public uint VirtualAddress;

    /// <summary>
    /// The size of the table, in bytes.
    /// </summary>
    [FieldOffset(4)]
    public uint Size;
}