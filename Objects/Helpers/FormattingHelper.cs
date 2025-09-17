using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Vanigam.CRM.Objects.Helpers
{
    public static class FormattingHelper
    {
        public const string PHONE_FORMAT_REGEX = @"^\(?(\d\d\d)\)?(.?)(\d\d\d)(.?)(\d\d\d\d)( ext\. \d{1,7})?$";
        public const string EMAIL_FORMAT_REGEX = @"^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$";
        public const string SECURE_EMAIL_FORMAT_REGEX = @"^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$";
        public const string SSN_FORMAT_REGEX = @"(\d\d\d)(-?)(\d\d)(-?)(\d\d\d\d)";
        public const string ZIP_FORMAT_REGEX = @"(\d{5})(-?\d{4})?";

        public const string IP_FORMAT_REGEX = @"(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})";

        private const string PHONE_CENTRICITY_TRASH_FORMAT_REGEX = @"^\(?(\d\d\d)\)?(.?)(\d\d\d)(.?)(\d\d\d\d)(C.*)$";

        public static string PhoneNumberToNumbersOnlyNoExt(this string phoneNumber)
        {
            string result = null;
            if (IsPhoneNumber(phoneNumber))
            {
                result = Regex.Replace(phoneNumber, PHONE_FORMAT_REGEX, "$1$3$5").Replace("ext. ", "");
            }
            return result;
        }

        public static string PhoneNumberToNumbersOnly(this string value)
        {
            return value != null ? new String(value.Where(Char.IsDigit).ToArray()) : null;
        }
        public static string PhoneNumberToNumbersExtOnly(string phoneNumber)
        {
            string result = null;
            if (IsPhoneNumber(phoneNumber))
            {
                result = Regex.Replace(phoneNumber, PHONE_FORMAT_REGEX, "$6");
            }
            return result;
        }

        public static string FormatPhoneNumber(this string phoneNumber)
        {
            string result = null;
            if (phoneNumber != null)
            {
                phoneNumber = phoneNumber.Trim();

                // This line breaks other code, if CDA needs this line please create another method
                //phoneNumber = phoneNumber.ToUpper();

                phoneNumber = phoneNumber.Replace("CHOME", "").Replace("chome", "");
                phoneNumber = phoneNumber.Replace("CMOBILE", "").Replace("cmobile", "");
                phoneNumber = phoneNumber.Replace("CCELL", "").Replace("ccell", "");
                phoneNumber = phoneNumber.Replace("CWORK", "").Replace("cwork", "");
                phoneNumber = phoneNumber.Replace("COFFICE", "").Replace("coffice", "");
                phoneNumber = phoneNumber.Replace("CFAX", "").Replace("cfax", "");
                //for CDA
                //------
                phoneNumber = phoneNumber.Replace("TEL:", "").Replace("tel:", "");
                phoneNumber = phoneNumber.Replace("+1", "");
                if (phoneNumber.StartsWith("-"))
                {
                    phoneNumber = phoneNumber.Substring(1);
                }

                // This line breaks other code, if CDA needs this removed please create another method
                //phoneNumber = phoneNumber.Replace("-", "");
                //------
                phoneNumber = phoneNumber.Replace("TEL:", "").Replace("tel", "");


                if (Regex.IsMatch(phoneNumber, PHONE_FORMAT_REGEX, RegexOptions.IgnoreCase))
                {
                    result = Regex.Replace(phoneNumber, PHONE_FORMAT_REGEX, "($1) $3-$5$6");
                    if (result.EndsWith("000-0000"))
                    {
                        result = null;
                    }
                }
                else if (Regex.IsMatch(phoneNumber, PHONE_CENTRICITY_TRASH_FORMAT_REGEX))
                {
                    result = Regex.Replace(phoneNumber, PHONE_CENTRICITY_TRASH_FORMAT_REGEX, "($1) $3-$5");
                }
                else if (Regex.IsMatch(phoneNumber, @"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}"))
                {
                    result = phoneNumber;
                }
            }
            return result;
        }

        public static string FormatPhoneNumberNoExt(string phoneNumber)
        {
            string result = null;
            if (IsPhoneNumber(phoneNumber))
            {
                phoneNumber = phoneNumber.Trim();
                result = Regex.Replace(phoneNumber, PHONE_FORMAT_REGEX, "($1) $3-$5");
                if (result.StartsWith("000") && result.EndsWith("0000"))
                {
                    result = null;
                }
            }
            return result;
        }

        public static string FormatPhoneNumberNumbersOnlyNoExt(string phoneNumber)
        {
            string result = null;
            if (IsPhoneNumber(phoneNumber))
            {
                phoneNumber = phoneNumber.Trim();
                result = Regex.Replace(phoneNumber, PHONE_FORMAT_REGEX, "$1$3$5");
                if (result.StartsWith("000") && result.EndsWith("0000"))
                {
                    result = null;
                }
            }
            return result;
        }

        public static string FormatPhoneNumberNumbersOnlyNoExtWithCountryCode(string phoneNumber)
        {
            var result = FormatPhoneNumberNumbersOnlyNoExt(phoneNumber);
            if (result.Length == 10)
            {
                result = "+1" + result;
            }
            return result;
        }

        public static bool IsPhoneNumber(string phoneNumber)
        {
            return !string.IsNullOrEmpty(phoneNumber) && Regex.IsMatch(phoneNumber, PHONE_FORMAT_REGEX, RegexOptions.IgnoreCase);
        }

        public static bool IsIpAddress(string ipAddress)
        {
            return !string.IsNullOrEmpty(ipAddress) && Regex.IsMatch(ipAddress, IP_FORMAT_REGEX, RegexOptions.IgnoreCase);
        }
        public static bool IsEmailAddress(string emailAddress)
        {
            return !string.IsNullOrEmpty(emailAddress) && Regex.IsMatch(emailAddress, EMAIL_FORMAT_REGEX, RegexOptions.IgnoreCase);
        }

        public static string FormatEmail(string inputEmail)
        {
            string result = inputEmail?.Trim();
            if (!string.IsNullOrEmpty(result) && Regex.IsMatch(result, EMAIL_FORMAT_REGEX))
            {
                return result;
            }
            return null;
        }
        public static string FormatSecureEmail(string inputEmail)
        {
            string result = inputEmail?.Trim();
            if (!string.IsNullOrEmpty(result) && Regex.IsMatch(result, SECURE_EMAIL_FORMAT_REGEX))
            {
                return result;
            }
            return null;
        }


        public static string FormatSSN(string inputSSN)
        {
            string result = null;
            if (inputSSN != null && Regex.IsMatch(inputSSN, SSN_FORMAT_REGEX))
            {
                inputSSN = inputSSN.Trim();
                result = Regex.Replace(inputSSN, SSN_FORMAT_REGEX, "$1-$3-$5");
            }
            return result;
        }

        public static string FormatIP(string inputIP)
        {
            string result = null;
            if (inputIP != null && Regex.IsMatch(inputIP, IP_FORMAT_REGEX))
            {
                inputIP = inputIP.Trim();
                result = Regex.Replace(inputIP, IP_FORMAT_REGEX, "$1.$2.$3.$4");
            }
            return result;
        }

        public static string FormatZip(string inputZip)
        {
            string result = null;
            if (inputZip != null && Regex.IsMatch(inputZip, ZIP_FORMAT_REGEX))
            {
                inputZip = inputZip.Trim();
                if (Regex.IsMatch(inputZip, @"(\d{5})-?(\d{4})"))
                {
                    result = Regex.Replace(inputZip, @"(\d{5})-?(\d{4})?", "$1-$2");
                }
                else if (Regex.IsMatch(inputZip, @"^(\d{5}).*"))
                {
                    result = Regex.Replace(inputZip, @"^(\d{5}).*", "$1");
                }
            }
            return result;
        }

        private readonly static Regex digitsOnly = new Regex(@"[^\d]");
        public static string NumbersOnly(string input)
        {
            if (input == null)
            {
                return "";
            }
            return digitsOnly.Replace(input, "");
        }

        // http://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string ByteArrayToBinHexString(byte[] bytes)
        {
            return bytes.Select(b => b.ToString("X2")).Aggregate((s1, s2) => s1 + s2);
        }

        public static string RemoveSpecialCharactersAndPunctuation(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}

