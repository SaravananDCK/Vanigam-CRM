using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.Helpers
{
    public static class PasswordHelper
    {
        //public static string GeneratePassword(int length = 8)
        //{
        //    const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        //    StringBuilder res = new();
        //    Random rnd = new();
        //    while (0 < length--)
        //    {
        //        res.Append(valid[rnd.Next(valid.Length)]);
        //    }
        //    return res.ToString();
        //}
        public static string GenerateRandomPassword(int length = 8)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}

