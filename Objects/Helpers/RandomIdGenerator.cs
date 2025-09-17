using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.Helpers
{
    public static class RandomIdGenerator
    {
        private static readonly Random _random = new Random();
        private static readonly char[] _chars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        public static string GenerateRandomAlphaNumericId(int length = 15)
        {
            var stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(_chars[_random.Next(_chars.Length)]);
            }

            return stringBuilder.ToString();
        }
    }
}

