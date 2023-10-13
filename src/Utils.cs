using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatePE
{
    internal static class Utils
    {
        public static uint RoundUpToAlignment(this uint value, uint alignment) =>
            (uint)((value + (alignment - 1)) & (-alignment));

        public static uint RoundUpToAlignment(this int value, uint alignment) =>
            ((uint)value).RoundUpToAlignment(alignment);

    }
}
