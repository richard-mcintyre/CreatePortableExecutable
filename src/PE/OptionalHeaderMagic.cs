using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatePE.PE;

[Flags]
internal enum OptionalHeaderMagic : ushort
{
    /// <summary>
    /// The file is an executable image.
    /// </summary>
    IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,

    /// <summary>
    /// The file is an executable image.
    /// </summary>
    IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b,

    /// <summary>
    /// The file is a ROM image.
    /// </summary>
    IMAGE_ROM_OPTIONAL_HDR_MAGIC = 0x107,
}