using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vanigam.CRM.AI.Objects.Converters
{
    public partial class UtcToLocalTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DateTime dt = reader.GetDateTime();
            if (dt.Kind == DateTimeKind.Utc)
            {
                return dt.ToLocalTime();
            }
            return dt;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime());
        }
    }
}

