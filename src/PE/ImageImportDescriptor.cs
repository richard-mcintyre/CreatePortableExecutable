using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CreatePE.PE;

[StructLayout(LayoutKind.Explicit)]
internal struct ImageImportDescriptor
{
    [FieldOffset(0)]
    public uint OriginalFirstThunk;

    [FieldOffset(4)]
    public uint TimeDateStamp;

    [FieldOffset(8)]
    public uint ForwarderChain;

    [FieldOffset(12)]
    public uint ImportedDllName;

    [FieldOffset(16)]
    public uint FirstThunk;
}
