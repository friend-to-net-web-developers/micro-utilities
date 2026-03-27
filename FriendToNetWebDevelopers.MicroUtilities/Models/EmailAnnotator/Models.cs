namespace FriendToNetWebDevelopers.MicroUtilities.Models.EmailAnnotator;


public enum CharacterType
{
    EmailStructural, // @ . + (email sub-address delimiter)
    UriStructural, // @ . (uri userinfo delimiter — + is not semantic)
    AsciiAlpha, // a-z A-Z
    AsciiDigit, // 0-9
    AsciiSpecial, // printable ASCII punctuation, context-dependent allowed chars
    PercentEncoded, // %XX sequences (URI mode only)
    Unicode, // code points > U+007F
    Invalid // characters not permitted in this context
}

public enum InputMode
{
    Email, // RFC 5321 / 6531 — raw Unicode allowed, + is semantic
    Uri // RFC 3986 — userinfo, percent-encoding expected, + is not semantic
}

public record CharacterToken(
    string Char,
    string Codepoint,
    CharacterType Type,
    string? Script,
    string? HomoglyphOf,
    bool IsEmailStructural,
    bool IsSuspicious
);

public record AddressPartAnnotation(
    string Raw,
    string LocalPart,
    string Domain,
    InputMode Mode,
    IReadOnlyList<CharacterToken> LocalTokens,
    IReadOnlyList<CharacterToken> DomainTokens,
    bool ContainsUnicode,
    bool ContainsSuspiciousChars,
    bool ContainsInvalidChars
);

public enum DomainInputForm
{
    PlainAscii,    // alice@example.com — boring, good
    Unicode,       // alice@münchen.de — legitimate but needs scrutiny
    Punycode,      // alice@xn--mnchen-3ya.de — suspicious from a human
    Mixed,         // some labels unicode, some xn-- — malformed, red flag
    Invalid        // couldn't be parsed/converted at all
}