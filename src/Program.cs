using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CreatePE.Args;

namespace CreatePE;

class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("USAGE: CreatePE <json file> <output file>");
            return;
        }

        ProgramArgs progArgs = ProcessArgs(args[0]);
        if (progArgs is null)
            return;

        PEBuilder builder = new PEBuilder(
            progArgs.ImageBase,
            progArgs.Stack.Reserve, progArgs.Stack.Commit,
            progArgs.Heap.Reserve, progArgs.Heap.Commit);

        builder.AddCode(progArgs.Code.AddressRVA, File.ReadAllBytes(progArgs.Code.Content));

        if (String.IsNullOrWhiteSpace(progArgs.Data.Content))
        {
            builder.AddData(progArgs.Data.AddressRVA, progArgs.Data.Size);
        }
        else
        {
            builder.AddData(progArgs.Data.AddressRVA, File.ReadAllBytes(progArgs.Data.Content));
        }

        ImportedFunction[] importedFunctions = builder.AddImports(progArgs.Imports.AddressRVA, progArgs.Imports.Libraries);

        using (FileStream stream = new FileStream(args[1], FileMode.Create))
        {
            builder.WriteTo(stream);
        }

        // Write out the address of the imported functions
        foreach (ImportedFunction func in importedFunctions)
        {
            Console.WriteLine($"{func.FunctionName}\t\t\tequ\t0x{(progArgs.ImageBase + func.Address):x}");
        }
    }

    private static ProgramArgs ProcessArgs(string jsonFileName)
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.Converters.Add(new Hex32JsonConverter());

        ProgramArgs programArgs = JsonSerializer.Deserialize<ProgramArgs>(File.ReadAllText(jsonFileName), options);

        if (programArgs.Code is null)
        {
            Console.WriteLine("No code specified.");
            return null;
        }

        FileInfo codeFileInfo = new FileInfo(programArgs.Code.Content);
        if (codeFileInfo.Exists == false)
        {
            Console.WriteLine($"{programArgs.Code.Content} does not exist.");
            return null;
        }

        if (programArgs.Data is null)
        {
            // It appears the windows loader requires a data section...
            uint addressRVA = ((uint)(programArgs.Code.AddressRVA + codeFileInfo.Length)).RoundUpToAlignment(PEBuilder.SectionAlignment);

            programArgs.Data = new DataSection(addressRVA, Size: 1, Content: null);
        }
        else
        {
            if (programArgs.Data.AddressRVA == 0)
            {
                Console.WriteLine("Data section address must be specified.");
            }

            if (String.IsNullOrWhiteSpace(programArgs.Data.Content))
            {
                if (programArgs.Data.Size == 0)
                {
                    Console.WriteLine("Data section size or content must be specified.");
                    return null;
                }
            }
            else
            {
                FileInfo dataFileInfo = new FileInfo(programArgs.Data.Content);
                if (dataFileInfo.Exists == false)
                {
                    Console.WriteLine($"{programArgs.Data.Content} does not exist.");
                    return null;
                }
            }
        }

        return programArgs;
    }

    
}
