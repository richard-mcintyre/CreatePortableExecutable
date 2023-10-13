using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatePE;

public record ImportedFunction(string DllName, string FunctionName, uint Address);

