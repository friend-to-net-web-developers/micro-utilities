namespace FriendToNetWebDevelopers.MicroUtilities.Models.EmailAnnotator;

/// <summary>
/// Classifies the type of a character token within an address string.
/// </summary>
public enum CharacterType
{
    /// <summary>Structural character in an email address — <c>@</c>, <c>.</c>, <c>+</c> (sub-address delimiter).</summary>
    EmailStructural,

    /// <summary>Structural character in a URI userinfo component — <c>@</c>, <c>.</c>.</summary>
    UriStructural,

    /// <summary>ASCII letter (a–z, A–Z).</summary>
    AsciiAlpha,

    /// <summary>ASCII digit (0–9).</summary>
    AsciiDigit,

    /// <summary>Printable ASCII punctuation that is permitted in this context.</summary>
    AsciiSpecial,

    /// <summary>Percent-encoded sequence (<c>%XX</c>). URI mode only.</summary>
    PercentEncoded,

    /// <summary>Unicode code point above U+007F.</summary>
    Unicode,

    /// <summary>Character not permitted in this address context.</summary>
    Invalid
}

/// <summary>
/// Specifies the address format being annotated, which determines which characters
/// are considered structural, allowed, or invalid.
/// </summary>
public enum InputMode
{
    /// <summary>
    /// RFC 5321 / 6531 email address. Raw Unicode is allowed in the local part and domain.
    /// <c>+</c> is a semantic sub-address delimiter.
    /// </summary>
    Email,

    /// <summary>
    /// RFC 3986 URI userinfo or host component. Percent-encoding is expected.
    /// <c>+</c> is not semantic and is treated as an allowed special character.
    /// </summary>
    Uri
}

/// <summary>
/// Represents a single character (or percent-encoded sequence) within an address string,
/// along with its classification and security-relevant metadata.
/// </summary>
/// <param name="Char">
/// The character or sequence as a string. Uses <see cref="string"/> rather than <see cref="char"/>
/// to accommodate surrogate pairs (supplementary plane characters) and percent-encoded sequences.
/// </param>
/// <param name="Codepoint">The Unicode code point in <c>U+XXXX</c> notation.</param>
/// <param name="Type">The character's classification within this address context.</param>
/// <param name="Script">
/// The Unicode script block name for non-ASCII characters (e.g. <c>"Cyrillic"</c>, <c>"Greek"</c>).
/// Null for ASCII characters.
/// </param>
/// <param name="HomoglyphOf">
/// The ASCII character this code point visually resembles, if it appears in the homoglyph table.
/// Null if no known homoglyph match. When non-null, <see cref="IsSuspicious"/> will be true.
/// </param>
/// <param name="IsStructural">
/// True when this character is structural in the current address mode — i.e.
/// <see cref="Type"/> is <see cref="CharacterType.EmailStructural"/> or
/// <see cref="CharacterType.UriStructural"/>. Derived from <see cref="Type"/> for
/// convenience; callers may also check <c>Type == CharacterType.EmailStructural</c> directly.
/// </param>
/// <param name="IsSuspicious">
/// True when the character is a known homoglyph of an ASCII character, or when the token
/// is otherwise flagged as potentially deceptive (e.g. mixed-script input).
/// </param>
public record CharacterToken(
    string Char,
    string Codepoint,
    CharacterType Type,
    string? Script,
    string? HomoglyphOf,
    bool IsStructural,
    bool IsSuspicious
);

/// <summary>
/// Contains the full token-level annotation of an address string, split into its
/// local part and domain (for email) or userinfo and host (for URIs).
/// </summary>
/// <param name="Raw">The original, unmodified input string.</param>
/// <param name="LocalPart">The portion of the address before the last <c>@</c>.</param>
/// <param name="Domain">The portion of the address after the last <c>@</c>. Empty if no <c>@</c> was present.</param>
/// <param name="Mode">The <see cref="InputMode"/> used when annotating this address.</param>
/// <param name="LocalTokens">Per-character tokens for the local part.</param>
/// <param name="DomainTokens">Per-character tokens for the domain.</param>
/// <param name="ContainsUnicode">True if any token has <see cref="CharacterType.Unicode"/>.</param>
/// <param name="ContainsSuspiciousChars">True if any token has <see cref="CharacterToken.IsSuspicious"/> set.</param>
/// <param name="ContainsInvalidChars">True if any token has <see cref="CharacterType.Invalid"/>.</param>
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

/// <summary>
/// Describes the encoding form of a domain name input, used to detect
/// suspicious or malformed submissions.
/// </summary>
public enum DomainInputForm
{
    /// <summary>All-ASCII domain with no <c>xn--</c> labels. The common, expected form.</summary>
    PlainAscii,

    /// <summary>Domain contains non-ASCII Unicode characters. Legitimate for IDNs but warrants scrutiny.</summary>
    Unicode,

    /// <summary>
    /// Domain contains <c>xn--</c> Punycode labels. Suspicious when submitted by a human —
    /// browsers and mail clients normalise to Unicode for display.
    /// </summary>
    Punycode,

    /// <summary>
    /// Domain contains a mix of Unicode and Punycode labels. Structurally malformed — strong red flag.
    /// </summary>
    Mixed,

    /// <summary>Domain could not be parsed or converted. Treat as invalid input.</summary>
    Invalid
}