using System.Text;
using System.Text.RegularExpressions;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    public static partial class Text
    {
        /// <summary>
        /// Encodes a string by converting Unicode characters into escape sequences.
        /// High-surrogate and low-surrogate character pairs are converted into UTF-32 escape sequences,
        /// while other characters above the ASCII range are converted into UTF-16 escape sequences.
        /// </summary>
        /// <param name="input">The input string to encode.</param>
        /// <returns>A string where Unicode characters are replaced by their respective escape sequences.</returns>
        public static string EncodeUnicodeEscapes(string input)
        {
            var sb = new StringBuilder();
            var i = 0;
            while (i < input.Length)
            {
                if (char.IsHighSurrogate(input[i]) && i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
                {
                    var codePoint = char.ConvertToUtf32(input[i], input[i + 1]);
                    sb.Append($"\\U{codePoint:X8}");
                    i += 2;
                }
                else
                {
                    var c = input[i];
                    sb.Append(c > 127 ? $"\\u{(int)c:X4}" : c.ToString());
                    i++;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Decodes a string by converting Unicode escape sequences into their corresponding characters.
        /// UTF-32 and UTF-16 escape sequences are replaced with their respective Unicode characters.
        /// </summary>
        /// <param name="input">The input string containing Unicode escape sequences to decode.</param>
        /// <returns>A string where escape sequences are replaced with their corresponding Unicode characters.</returns>
        public static string DecodeUnicodeEscapes(string input)
        {
            return Utf32Regex().Replace(input, m => m.Groups[1].Success
                    ? char.ConvertFromUtf32(Convert.ToInt32(m.Groups[1].Value, 16))
                    : ((char)Convert.ToInt32(m.Groups[2].Value, 16)).ToString());
        }

        [GeneratedRegex(@"\\U([0-9A-Fa-f]{8})|\\u([0-9A-Fa-f]{4})")]
        private static partial Regex Utf32Regex();
    }
}
