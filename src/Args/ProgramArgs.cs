using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatePE.Args;

public class ProgramArgs
{
    /// <summary>
    /// Virtual address that is added to every section virtual address
    /// </summary>
    public uint ImageBase { get; set; } = 0x00400000;

    /// <summary>
    /// Reserve and commit size of the stack
    /// </summary>
    public ReserveAndCommit Stack { get; set; } = new ReserveAndCommit(0x80000, 0x11000);

    /// <summary>
    /// Reserve and commit size of the heap
    /// </summary>
    public ReserveAndCommit Heap { get; set; } = new ReserveAndCommit(0x100000, 0x1000);

    /// <summary>
    /// Gets/sets the contents of the code section
    /// </summary>
    public CodeSection Code { get; set; }

    /// <summary>
    /// Gets/sets the contents
    /// </summary>
    public DataSection Data { get; set; }

    /// <summary>
    /// Functions imported from other libraries
    /// </summary>
    public ImportsSection Imports { get; set; }
}

public record CodeSection(uint AddressRVA, string Content);

public record DataSection(uint AddressRVA, uint Size, string Content);

public record ImportsSection(uint AddressRVA, ProgramArgsDllImport[] Libraries);