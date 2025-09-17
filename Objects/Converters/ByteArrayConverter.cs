using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vanigam.CRM.AI.Objects.Converters
{
    public partial class ByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string base64String = reader.GetString();
            return Convert.FromBase64String(base64String);
        }
        

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            string base64String = Convert.ToBase64String(value).Replace("data:image/jpeg;", string.Empty).Replace("base64,", string.Empty);
            writer.WriteStringValue(base64String);
        }
        
    }
}

