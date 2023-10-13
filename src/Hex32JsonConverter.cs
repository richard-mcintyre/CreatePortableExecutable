using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace CreatePE;

internal class Hex32JsonConverter : JsonConverter<uint>
{
    public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string str = reader.GetString();

        if (str.StartsWith("0x"))
            return Convert.ToUInt32(str, 16);

        return Convert.ToUInt32(str, 10);
    }

    public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options) =>
        throw new NotImplementedException();
}
