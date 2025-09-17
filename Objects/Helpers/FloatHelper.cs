using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.Helpers
{
    public static class FloatHelper
    {
        public static bool IsBetween(this float value, float lowerBound, float upperBound, float tolerance = 0.00001f)
        {
            return (value >= lowerBound - tolerance) && (value <= upperBound + tolerance);
        }
        public static bool IsBetween(this float? value, float? lowerBound, float? upperBound, float tolerance = 0.00001f)
        {
            return (value >= lowerBound - tolerance) && (value <= upperBound + tolerance);
        }

        // Check if a float value is between two other values considering relative tolerance
        public static bool IsBetweenRelative(this float value, float lowerBound, float upperBound, float relativeTolerance = 0.00001f)
        {
            float diff1 = Math.Abs(value - lowerBound);
            float diff2 = Math.Abs(value - upperBound);
            lowerBound = Math.Abs(lowerBound);
            upperBound = Math.Abs(upperBound);
            float largest1 = (value > lowerBound) ? value : lowerBound;
            float largest2 = (value > upperBound) ? value : upperBound;

            return (diff1 <= largest1 * relativeTolerance) && (diff2 <= largest2 * relativeTolerance);
        }
        public static string ToFloatString(this float? value)
        {
            if (value == null || value.Value == 0)
            {
                return "-";
            }
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}

