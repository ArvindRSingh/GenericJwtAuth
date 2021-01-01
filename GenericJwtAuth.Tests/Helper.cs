using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericJwtAuth.Tests
{
    public enum GenerateRandomStringOptions : int
    {
        IncludeAlphabets = 2,
        CaseSensitive = 4,
        IncludeDigits = 8,
        IncludeNonAlphaNumericCharacters = 16
    }
    public static class Helper
    {
        public static string GenerateRandomString(int length, GenerateRandomStringOptions options)
        {
            char[] digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            char[] spl = new char[] { '`', '~', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '+' };

            string[] charSet;

            Random random = new Random();
            StringBuilder sb = new StringBuilder();

            if ((options & GenerateRandomStringOptions.IncludeAlphabets) == GenerateRandomStringOptions.IncludeAlphabets)
            {
                if ((options & GenerateRandomStringOptions.CaseSensitive) == GenerateRandomStringOptions.CaseSensitive)
                {
                    charSet = new string[] { "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyz" };
                }
                else
                {
                    charSet = new string[] { "abcdefghijklmnopqrstuvwxyz" };
                }


                for (int i = 0; i < charSet.Length; i++)
                {
                    string randomChars = new string(Enumerable.Repeat(charSet[i], length - 2)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
                    sb.Append(randomChars);
                }
            }

            if ((options & GenerateRandomStringOptions.IncludeNonAlphaNumericCharacters) == GenerateRandomStringOptions.IncludeNonAlphaNumericCharacters)
            {
                sb.Append(spl[random.Next(spl.Length)]);
            }

            if ((options & GenerateRandomStringOptions.IncludeNonAlphaNumericCharacters) == GenerateRandomStringOptions.IncludeNonAlphaNumericCharacters)
            {
                sb.Append(digits[random.Next(9)]);
            }

            return sb.ToString();
        }
    }
}
