using FriendToNetWebDevelopers.MicroUtilities.Models.TextAnnotator;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    public static class TextAnnotator
    {
        /// <summary>
        /// Analyzes the input string and produces a sequence of <see cref="TextCharacterToken"/> objects
        /// with metadata about each character in the string, including its type and encoded representation.
        /// </summary>
        /// <param name="input">The input string to be tokenized and analyzed.</param>
        /// <returns>
        /// An enumerable collection of <see cref="TextCharacterToken"/> that represents each character
        /// in the input string along with its associated metadata.
        /// </returns>
        public static IEnumerable<TextCharacterToken> Annotate(string input)
        {
            var i = 0;
            while (i < input.Length)
            {
                // Check for surrogate pair
                if (char.IsHighSurrogate(input[i]) && i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
                {
                    var codePoint = char.ConvertToUtf32(input[i], input[i + 1]);
                    var chars = input.Substring(i, 2);
                    yield return new TextCharacterToken(chars, i, TextCharacterType.Unicode, codePoint, $"\\U{codePoint:X8}");
                    i += 2;
                }
                else
                {
                    var c = input[i];
                    int codePoint = c;
                    yield return new TextCharacterToken(c.ToString(), i, _classify(c), codePoint, _getEscape(c));
                    i++;
                }
            }
        }

        private static TextCharacterType _classify(char c) => c switch
        {
            _ when char.IsWhiteSpace(c)   => TextCharacterType.Whitespace,
            _ when c > 127                => TextCharacterType.Unicode,
            _ when char.IsLetter(c)       => TextCharacterType.Letter,
            _ when char.IsDigit(c)        => TextCharacterType.Digit,
            _ when c >= 32 && c <= 126    => TextCharacterType.Special,
            _                             => TextCharacterType.Unicode
        };

        private static string? _getEscape(char c) =>
            c > 127 ? $"\\u{(int)c:X4}" : null;
    }
}
