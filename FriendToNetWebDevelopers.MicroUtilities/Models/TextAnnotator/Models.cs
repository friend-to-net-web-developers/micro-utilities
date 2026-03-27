namespace FriendToNetWebDevelopers.MicroUtilities.Models.TextAnnotator;

public enum TextCharacterType
{
    Letter,       // a-z, A-Z
    Digit,        // 0-9
    Special,      // ASCII 32-126, not letter or digit
    Whitespace,   // space, tab, newline, etc.
    Unicode       // > 127
}

public record TextCharacterToken(
    string Character,      // string instead of char to accommodate surrogate pairs
    int Index,             // index in the original string (of the high surrogate)
    TextCharacterType Type,
    int CodePoint,         // actual Unicode code point
    string? UnicodeEscape  // \uXXXX for BMP, \U00XXXXXX for supplementary
);