using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CreatePE.PE;

[StructLayout(LayoutKind.Explicit)]
internal struct ImageNtHeader32
{
    public ImageNtHeader32()
    {
        this.FileHeader = new ImageFileHeader();
        this.OptionalHeader = new ImageOptionalHeader32();
    }

    public static ushort Size => (ushort)Marshal.SizeOf<ImageNtHeader32>();

    /// <summary>
    /// A 4-byte signature identifying the file as a PE image. The bytes are "PE\0\0".
    /// </summary>
    [FieldOffset(0)]
    public uint Signature;

    /// <summary>
    /// An IMAGE_FILE_HEADER structure that specifies the file header.
    /// </summary>
    [FieldOffset(4)]
    public ImageFileHeader FileHeader;

    /// <summary>
    /// An IMAGE_OPTIONAL_HEADER structure that specifies the optional file header.
    /// </summary>
    [FieldOffset(24)]
    public ImageOptionalHeader32 OptionalHeader;
}
