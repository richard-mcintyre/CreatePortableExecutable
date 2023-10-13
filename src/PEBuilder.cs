using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CreatePE.Args;
using CreatePE.PE;

namespace CreatePE;

internal class PEBuilder
{
    record SectionInfo(string Name, uint AddressRVA, uint Size, byte[] Content, SectionCharacteristics Flags);

    #region Construction

    public PEBuilder(uint imageBase = 0x00400000,
                     uint sizeOfStackReserve = 0x80000, uint sizeOfStackCommit = 0x11000,
                     uint sizeofHeapReserve = 0x100000, uint sizeOfHeapCommit = 0x1000)
    {
        _imageBase = imageBase;
        _sizeOfStackReserve = sizeOfStackReserve;
        _sizeOfStackCommit = sizeOfStackCommit;
        _sizeOfHeapReserve = sizeofHeapReserve;
        _sizeOfHeapCommit = sizeOfHeapCommit;
    }

    #endregion

    #region Fields

    private const uint FileAlignment = 512;
    private const uint _sectionAlignment = 4096;

    private uint _imageBase;
    private uint _sizeOfStackReserve;
    private uint _sizeOfStackCommit;
    private uint _sizeOfHeapReserve;
    private uint _sizeOfHeapCommit;

    private List<SectionInfo> _sections = new List<SectionInfo>();
    private uint _baseOfCode;
    private uint _sizeOfCode;
    private SectionInfo _codeInfo;

    private uint _baseOfData;
    private uint _sizeOfData;
    private uint _sizeOfInitializedData;

    private ProgramArgsDllImport[] _imports = Array.Empty<ProgramArgsDllImport>();
    private uint _descriptorsRVA;
    private uint _descriptorsSize;
    private uint _importAddressTableRVA;
    private uint _importAddressTableSize;

    #endregion

    #region Properties

    public static uint SectionAlignment => _sectionAlignment;

    #endregion

    #region Methods

    public void AddCode(uint addressRVA, byte[] code)
    {
        _codeInfo = AddSection(".text", addressRVA, code, SectionCharacteristics.ContainsCode | SectionCharacteristics.MemRead);
        _baseOfCode = addressRVA;
        _sizeOfCode += ((uint)code.Length).RoundUpToAlignment(FileAlignment);
    }

    public void AddData(uint addressRVA, uint size)
    {
        _baseOfData = addressRVA;
        AddData(addressRVA, new byte[size]);
    }

    public void AddData(uint addressRVA, byte[] data)
    {
        AddSection(".data", addressRVA, data, SectionCharacteristics.MemRead | SectionCharacteristics.MemWrite);
        _sizeOfData += ((uint)data.Length).RoundUpToAlignment(FileAlignment);
    }

    public ImportedFunction[] AddImports(uint addressRVA, ProgramArgsDllImport[] imports)
    {
        if (imports is null || imports.Length == 0)
            return Array.Empty<ImportedFunction>();

        _imports = imports.ToArray();

        ImportedFunction[] importedFunctions;

        byte[] content = BuildImportSection(_imports, addressRVA,
            out _descriptorsRVA, out _descriptorsSize,
            out _importAddressTableRVA, out _importAddressTableSize,
            out importedFunctions);

        AddSection(".idata", addressRVA, content, SectionCharacteristics.ContainsInitializedData | SectionCharacteristics.MemRead);

        _sizeOfInitializedData = content.Length.RoundUpToAlignment(PEBuilder.FileAlignment);

        return importedFunctions;
    }

    private SectionInfo AddSection(string name, uint addressRVA, byte[] content, SectionCharacteristics flags)
    {
        SectionInfo section = new SectionInfo(name, addressRVA, (uint)content.Length, content, flags);
        _sections.Add(section);

        return section;
    }
    
    private static byte[] BuildImportSection(ProgramArgsDllImport[] importedDlls, uint sectionVirtualAddress,
            out uint descriptorsRVA, out uint descriptorsSize,
            out uint importAddressTableRVA, out uint importAddressTableSize,
            out ImportedFunction[] functionRVAs)
    {
        descriptorsRVA = 0;
        descriptorsSize = 0;
        importAddressTableRVA = 0;
        importAddressTableSize = 0;
        functionRVAs = Array.Empty<ImportedFunction>();
        List<ImportedFunction> functionRVAsList = new List<ImportedFunction>();

        // Figure out the size of the ILT and IAT, we will put those at the start of the section
        // but we cannot write them until we know the location of the dll/function names

        // The ILT/IAT is a 4 byte entry for each function imported, plus a null entry for each
        // dll.  We will put the IAT at the start, this is the addresses that are overwritten
        // by the windows loader
        uint iatSize = (uint)(importedDlls.Sum(o => o.Functions.Length) * 4 + importedDlls.Length * 4);
        uint iltSize = iatSize;

        importAddressTableRVA = sectionVirtualAddress;  // Putting then at the start of the section
        importAddressTableSize = iatSize;

        // Figure out the size of the import descriptors.  There is one descriptor for each dll, plus a null entry
        int descriptorSize = Marshal.SizeOf<ImageImportDescriptor>();
        uint descriptorTableSize = (uint)(descriptorSize * (importedDlls.Length + 1));

        // We now know the offset we will write the dll and function names
        uint startOfNamesOffset = iltSize + iatSize + descriptorTableSize;

        using (MemoryStream ms = new MemoryStream())
        {
            ms.Position = startOfNamesOffset;

            // Write the names of the dlls and functions
            Dictionary<string, uint> name2Rva = new Dictionary<string, uint>();
            foreach (ProgramArgsDllImport importedDll in importedDlls)
            {
                if (name2Rva.ContainsKey(importedDll.Name) == false)
                {
                    name2Rva.Add(importedDll.Name, (uint)ms.Position);
                    ms.Write(Encoding.UTF8.GetBytes(importedDll.Name));
                    ms.WriteByte(0);
                }

                foreach (string importedFunctionName in importedDll.Functions)
                {
                    if (name2Rva.ContainsKey(importedFunctionName) == false)
                    {
                        name2Rva.Add(importedFunctionName, (uint)ms.Position);

                        ms.Write(BitConverter.GetBytes((ushort)0));     // Hint/Ordinal
                        ms.Write(Encoding.UTF8.GetBytes(importedFunctionName));
                        ms.WriteByte(0);
                    }
                }
            }

            // Write the IAT
            ms.Position = 0;
            Dictionary<string, uint> dllName2Iat = new Dictionary<string, uint>();
            foreach (ProgramArgsDllImport importedDll in importedDlls)
            {
                dllName2Iat.Add(importedDll.Name, (uint)ms.Position);
                foreach (string importedFunctionName in importedDll.Functions)
                {
                    functionRVAsList.Add(new ImportedFunction(importedDll.Name, importedFunctionName, (sectionVirtualAddress + (uint)ms.Position)));
                    ms.Write(BitConverter.GetBytes(name2Rva[importedFunctionName] + sectionVirtualAddress));
                }
                ms.Write(new byte[4]);  // null entry
            }

            // Write the ILT
            Dictionary<string, uint> dllName2Ilt = new Dictionary<string, uint>();
            foreach (ProgramArgsDllImport importedDll in importedDlls)
            {
                dllName2Ilt.Add(importedDll.Name, (uint)ms.Position);
                foreach (string importedFunctionName in importedDll.Functions)
                {
                    ms.Write(BitConverter.GetBytes(name2Rva[importedFunctionName] + sectionVirtualAddress));
                }
                ms.Write(new byte[4]);  // null entry
            }

            // Write the descriptor tables
            descriptorsRVA = (uint)ms.Position + sectionVirtualAddress;
            descriptorsSize = descriptorTableSize;

            Span<byte> buf = stackalloc byte[Marshal.SizeOf<ImageImportDescriptor>()];

            foreach (ProgramArgsDllImport importedDll in importedDlls)
            {
                ImageImportDescriptor descriptor = new ImageImportDescriptor();
                descriptor.OriginalFirstThunk = dllName2Ilt[importedDll.Name] + sectionVirtualAddress;
                descriptor.FirstThunk = dllName2Iat[importedDll.Name] + sectionVirtualAddress;
                descriptor.ImportedDllName = name2Rva[importedDll.Name] + sectionVirtualAddress;

                MemoryMarshal.Write(buf, ref descriptor);
                ms.Write(buf);
            }

            functionRVAs = functionRVAsList.ToArray();

            return ms.ToArray();
        }
    }

    public void WriteTo(Stream stream)
    {
        if (_codeInfo == null)
            throw new Exception($"No code specified");

        DosHeaderAndStub.WriteTo(stream);

        ImageNtHeader32 header = new ImageNtHeader32();
        header.Signature = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("PE\0\0"));

        header.FileHeader.Machine = Machine.I386;
        header.FileHeader.NumberOfSections = (ushort)_sections.Count;
        header.FileHeader.TimeDateStamp = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() & 0xFFFFFFFF);
        header.FileHeader.PointerToSymbolTable = 0;
        header.FileHeader.NumberOfSymbols = 0;
        header.FileHeader.SizeOfOptionalHeader = ImageOptionalHeader32.Size;
        header.FileHeader.Characteristics = Characteristics.RelocsStripped | Characteristics.ExecutableImage | Characteristics.Bit32Machine;

        header.OptionalHeader.MajorLinkerVersion = 14;
        header.OptionalHeader.MinorLinkerVersion = 37;
        header.OptionalHeader.SizeOfCode = _sizeOfCode;
        header.OptionalHeader.SizeOfInitializedData = _sizeOfInitializedData;
        header.OptionalHeader.SizeOfUninitializedData = _sizeOfData;
        header.OptionalHeader.AddressOfEntryPoint = 0x1000;
        header.OptionalHeader.BaseOfCode = _baseOfCode;
        header.OptionalHeader.BaseOfData = _baseOfData;
        header.OptionalHeader.ImageBase = _imageBase;
        header.OptionalHeader.SectionAlignment = PEBuilder.SectionAlignment;
        header.OptionalHeader.FileAlignment = PEBuilder.FileAlignment;
        header.OptionalHeader.MajorOperatingSystemVersion = 6;
        header.OptionalHeader.MinorOperatingSystemVersion = 0;
        header.OptionalHeader.MajorImageVersion = 6;
        header.OptionalHeader.MinorImageVersion = 0;
        header.OptionalHeader.MajorSubsystemVersion = 6;
        header.OptionalHeader.MinorSubsystemVersion = 0;

        uint sizeOfImage = (uint)(_sections.Sum(o => o.Size.RoundUpToAlignment(PEBuilder.SectionAlignment)) +
            DosHeaderAndStub.Size +
            ImageNtHeader32.Size +
            (PESectionHeader.Size * header.FileHeader.NumberOfSections));
        sizeOfImage = sizeOfImage.RoundUpToAlignment(PEBuilder.SectionAlignment);
            
        header.OptionalHeader.SizeOfImage = sizeOfImage;           // Must be multiple of SectionAlignment
        header.OptionalHeader.SizeOfHeaders = (uint)(4 + ImageNtHeader32.Size + (PESectionHeader.Size * header.FileHeader.NumberOfSections));
        header.OptionalHeader.CheckSum = 0;
        header.OptionalHeader.Subsystem = Subsystem.WindowsCui;
        header.OptionalHeader.DllCharacteristics = 0;
        header.OptionalHeader.SizeOfStackReserve = _sizeOfStackReserve;
        header.OptionalHeader.SizeOfStackCommit = _sizeOfStackCommit;
        header.OptionalHeader.SizeOfHeapReserve = _sizeOfHeapReserve;
        header.OptionalHeader.SizeOfHeapCommit = _sizeOfHeapCommit;
        header.OptionalHeader.NumberOfRvaAndSizes = 16;

        header.OptionalHeader.ImportDirectory.VirtualAddress = _descriptorsRVA;
        header.OptionalHeader.ImportDirectory.Size = _descriptorsSize;

        header.OptionalHeader.ImportAddressTableDirectory.VirtualAddress = _importAddressTableRVA;
        header.OptionalHeader.ImportAddressTableDirectory.Size = _importAddressTableSize;

        Span<byte> headerBuffer = new byte[ImageNtHeader32.Size];
        MemoryMarshal.Write(headerBuffer, ref header);
        stream.Write(headerBuffer);

        // Determine where the section raw data is going to be written to
        uint sectionDataOffset = (uint)(stream.Position + (Marshal.SizeOf<SectionHeader>() * _sections.Count));
        sectionDataOffset = sectionDataOffset.RoundUpToAlignment(PEBuilder.FileAlignment);

        uint curOffset = sectionDataOffset;
        Span<byte> sectionBuffer = new byte[Marshal.SizeOf<SectionHeader>()];
        foreach (SectionInfo sectionInfo in _sections)
        {
            PESectionHeader section = new PESectionHeader();

            section.Name = BitConverter.ToUInt64(Encoding.UTF8.GetBytes(sectionInfo.Name.PadRight(8, '\0')));
            section.VirtualAddress = sectionInfo.AddressRVA;
            section.VirtualSize = sectionInfo.Size.RoundUpToAlignment(PEBuilder.SectionAlignment);
            section.SizeOfRawData = sectionInfo.Size;
            section.PointerToRawData = curOffset;
            section.PointerToRelocations = 0;
            section.PointerToLinenumbers = 0;
            section.NumberOfRelocations = 0;
            section.NumberOfLinenumbers = 0;
            section.Characteristics = sectionInfo.Flags;

            MemoryMarshal.Write(sectionBuffer, ref section);
            stream.Write(sectionBuffer);

            curOffset += sectionInfo.Size.RoundUpToAlignment(PEBuilder.FileAlignment);
        }

        // Align to next file alignment boundary
        stream.Write(new byte[((uint)stream.Position).RoundUpToAlignment(PEBuilder.FileAlignment) - stream.Position]);

        foreach (SectionInfo sectionInfo in _sections)
        {
            byte[] rawData = new byte[sectionInfo.Content.Length.RoundUpToAlignment(PEBuilder.FileAlignment)];
            Array.Copy(sectionInfo.Content, 0, rawData, 0, sectionInfo.Content.Length);
            stream.Write(rawData);
        }
    }

    #endregion
}
