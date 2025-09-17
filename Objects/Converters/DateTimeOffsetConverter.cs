using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Vanigam.CRM.AI.Objects.Converters
{
    public class DateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
    {
        public DateTimeOffsetConverter()
            : base(
                d => ToUniversalTime(d),
                d => ToUniversalTime(d))
        {
        }

        static DateTimeOffset ToUniversalTime(DateTimeOffset value)
        {
            var newValue = new DateTimeOffset(value.UtcDateTime);
            return newValue;
        }
    }
}

