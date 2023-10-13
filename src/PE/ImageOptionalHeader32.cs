using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CreatePE.PE;

[StructLayout(LayoutKind.Explicit)]
internal struct ImageOptionalHeader32
{
    public ImageOptionalHeader32()
    {
        this.Magic = OptionalHeaderMagic.IMAGE_NT_OPTIONAL_HDR32_MAGIC;
    }

    public static ushort Size => (ushort)Marshal.SizeOf<ImageOptionalHeader32>();

    /// <summary>
    /// The state of the image file
    /// </summary>
    [FieldOffset(0)]
    private OptionalHeaderMagic Magic;

    /// <summary>
    /// The major version number of the linker.
    /// </summary>
    [FieldOffset(2)]
    public byte MajorLinkerVersion;

    /// <summary>
    /// The minor version number of the linker.
    /// </summary>
    [FieldOffset(3)]
    public byte MinorLinkerVersion;

    /// <summary>
    /// The size of the code section, in bytes, or the sum of all such sections if there are multiple code sections.
    /// </summary>
    [FieldOffset(4)]
    public uint SizeOfCode;

    /// <summary>
    /// The size of the initialized data section, in bytes, or the sum of all such sections if there are multiple initialized data sections.
    /// </summary>
    [FieldOffset(8)]
    public uint SizeOfInitializedData;

    /// <summary>
    /// The size of the uninitialized data section, in bytes, or the sum of all such sections if there are multiple uninitialized data sections.
    /// </summary>
    [FieldOffset(12)]
    public uint SizeOfUninitializedData;

    /// <summary>
    /// A pointer to the entry point function, relative to the image base address. 
    /// For executable files, this is the starting address. 
    /// For device drivers, this is the address of the initialization function. 
    /// The entry point function is optional for DLLs. 
    /// When no entry point is present, this member is zero.
    /// </summary>
    [FieldOffset(16)]
    public uint AddressOfEntryPoint;

    /// <summary>
    /// A pointer to the beginning of the code section, relative to the image base.
    /// </summary>
    [FieldOffset(20)]
    public uint BaseOfCode;

    /// <summary>
    /// A pointer to the beginning of the data section, relative to the image base.
    /// </summary>
    [FieldOffset(24)]
    public uint BaseOfData;

    /// <summary>
    /// The preferred address of the first byte of the image when it is loaded in memory. 
    /// This value is a multiple of 64K bytes. 
    /// The default value for DLLs is 0x10000000. 
    /// The default value for applications is 0x00400000, except on Windows CE where it is 0x00010000.
    /// </summary>
    [FieldOffset(28)]
    public uint ImageBase;

    /// <summary>
    /// The alignment of sections loaded in memory, in bytes. 
    /// This value must be greater than or equal to the FileAlignment member. 
    /// The default value is the page size for the system.
    /// </summary>
    [FieldOffset(32)]
    public uint SectionAlignment;

    /// <summary>
    /// The alignment of the raw data of sections in the image file, in bytes. 
    /// The value should be a power of 2 between 512 and 64K (inclusive). 
    /// The default is 512. 
    /// If the SectionAlignment member is less than the system page size, this member must be the same as SectionAlignment.
    /// </summary>
    [FieldOffset(36)]
    public uint FileAlignment;

    /// <summary>
    /// The major version number of the required operating system.
    /// </summary>
    [FieldOffset(40)]
    public ushort MajorOperatingSystemVersion;

    /// <summary>
    /// The minor version number of the required operating system.
    /// </summary>
    [FieldOffset(42)]
    public ushort MinorOperatingSystemVersion;

    /// <summary>
    /// The major version number of the image.
    /// </summary>
    [FieldOffset(44)]
    public ushort MajorImageVersion;

    /// <summary>
    /// The minor version number of the image.
    /// </summary>
    [FieldOffset(46)]
    public ushort MinorImageVersion;

    /// <summary>
    /// The major version number of the subsystem.
    /// </summary>
    [FieldOffset(48)]
    public ushort MajorSubsystemVersion;

    /// <summary>
    /// The minor version number of the subsystem.
    /// </summary>
    [FieldOffset(50)]
    public ushort MinorSubsystemVersion;

    /// <summary>
    /// This member is reserved and must be 0.
    /// </summary>
    [FieldOffset(52)]
    private uint Win32VersionValue;

    /// <summary>
    /// The size of the image, in bytes, including all headers. Must be a multiple of SectionAlignment.
    /// </summary>
    [FieldOffset(56)]
    public uint SizeOfImage;

    /// <summary>
    /// The combined size of the following items, rounded to a multiple of the value specified in the FileAlignment member.
    ///  - e_lfanew member of IMAGE_DOS_HEADER
    ///  - 4 byte signature
    ///  - size of IMAGE_FILE_HEADER
    ///  - size of optional header
    ///  - size of all section headers
    /// </summary>
    [FieldOffset(60)]
    public uint SizeOfHeaders;

    /// <summary>
    /// The image file checksum. 
    /// The following files are validated at load time: 
    ///  - all drivers, 
    ///  - any DLL loaded at boot time, 
    ///  - and any DLL loaded into a critical system process.
    /// </summary>
    [FieldOffset(64)]
    public uint CheckSum;

    /// <summary>
    /// The subsystem required to run this image.
    /// </summary>
    [FieldOffset(68)]
    public Subsystem Subsystem;

    /// <summary>
    /// The DLL characteristics of the image
    /// </summary>
    [FieldOffset(70)]
    public DllCharacteristics DllCharacteristics;

    /// <summary>
    /// The number of bytes to reserve for the stack. 
    /// Only the memory specified by the SizeOfStackCommit member is committed at load time; 
    /// the rest is made available one page at a time until this reserve size is reached.
    /// </summary>
    [FieldOffset(72)]
    public uint SizeOfStackReserve;

    /// <summary>
    /// The number of bytes to commit for the stack.
    /// </summary>
    [FieldOffset(76)]
    public uint SizeOfStackCommit;

    /// <summary>
    /// The number of bytes to reserve for the local heap. 
    /// Only the memory specified by the SizeOfHeapCommit member is committed at load time; 
    /// the rest is made available one page at a time until this reserve size is reached.
    /// </summary>
    [FieldOffset(80)]
    public uint SizeOfHeapReserve;

    /// <summary>
    /// The number of bytes to commit for the local heap.
    /// </summary>
    [FieldOffset(84)]
    public uint SizeOfHeapCommit;

    /// <summary>
    /// This member is obsolete.
    /// </summary>
    [FieldOffset(88)]
    private uint LoaderFlags;

    /// <summary>
    /// The number of directory entries in the remainder of the optional header. Each entry describes a location and size.
    /// </summary>
    [FieldOffset(92)]
    public uint NumberOfRvaAndSizes;

    [FieldOffset(96)]
    public ImageDataDirectory ExportDirectory;
    [FieldOffset(104)]
    public ImageDataDirectory ImportDirectory;
    [FieldOffset(112)]
    public ImageDataDirectory ResourceDirectory;
    [FieldOffset(120)]
    public ImageDataDirectory ExceptionDirectory;
    [FieldOffset(128)]
    public ImageDataDirectory CertificatesDirectory;
    [FieldOffset(136)]
    public ImageDataDirectory BaseRelocationDirectory;
    [FieldOffset(144)]
    public ImageDataDirectory DebugDirectory;
    [FieldOffset(152)]
    public ImageDataDirectory ArchitectureDirectory;
    [FieldOffset(160)]
    public ImageDataDirectory GlobalPointerDirectory;
    [FieldOffset(168)]
    public ImageDataDirectory ThreadStorageDirectory;
    [FieldOffset(176)]
    public ImageDataDirectory LoadConfigurationDirectory;
    [FieldOffset(184)]
    public ImageDataDirectory BoundImportDirectory;
    [FieldOffset(192)]
    public ImageDataDirectory ImportAddressTableDirectory;
    [FieldOffset(200)]
    public ImageDataDirectory DelayImportDirectory;
    [FieldOffset(208)]
    public ImageDataDirectory COMDescriptorDirectory;
    [FieldOffset(216)]
    private ImageDataDirectory ReservedDirectory;
}

