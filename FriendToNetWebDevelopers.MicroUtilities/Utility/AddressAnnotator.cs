using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using FriendToNetWebDevelopers.MicroUtilities.Models.EmailAnnotator;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    /// <summary>
    /// Provides methods to process and annotate address strings, such as email addresses or URIs,
    /// by analysing their structure, tokens, and character properties.
    /// </summary>
    public static class AddressAnnotator
    {
        // Partial homoglyph table — high-priority confusables only.
        // Characters that are visually indistinguishable from ASCII equivalents in common fonts
        // and are frequently used in phishing/spoofing attacks.
        // Sourced from Unicode TR39 confusables: https://www.unicode.org/reports/tr39/#confusables
        // Extend this table as needed — the full TR39 list contains thousands of entries.
        private static readonly IReadOnlyDictionary<int, char> Homoglyphs =
            new Dictionary<int, char>
            {
                // Cyrillic
                [0x0430] = 'a', [0x0435] = 'e', [0x043E] = 'o', [0x0440] = 'p',
                [0x0441] = 'c', [0x0445] = 'x', [0x0443] = 'y', [0x0456] = 'i',
                // Greek
                [0x03BF] = 'o', [0x03B1] = 'a', [0x03B5] = 'e', [0x03BD] = 'v',
                // Latin lookalikes (extended)
                [0x00F6] = 'o', [0x00FC] = 'u', [0x00E4] = 'a',
            };

        // Email-allowed ASCII special characters in the local part per RFC 5321.
        // Note: '+' is intentionally excluded here — it is classified as EmailStructural
        // by _classifyChar before this set is consulted, so including it would be dead data.
        private static readonly HashSet<char> EmailSpecialChars =
            ['!', '#', '$', '%', '&', '\'', '*', '-', '/', '=', '?', '^', '_', '`', '{', '|', '}', '~', '.'];

        // URI userinfo allowed ASCII special characters per RFC 3986.
        private static readonly HashSet<char> UriSpecialChars =
            ['!', '$', '&', '\'', '(', ')', '*', '+', ',', ';', '=', '-', '.', '_', '~', '%'];

        /// <summary>
        /// Analyses and annotates the user information portion of a given URI by evaluating
        /// its structure, tokens, and character properties, excluding any password component.
        /// </summary>
        /// <param name="uri">
        /// The URI containing the user information to analyse. Should be a valid absolute URI.
        /// </param>
        /// <returns>
        /// An <see cref="AddressPartAnnotation"/> containing the annotated userinfo along with
        /// metadata including tokens, suspicious or invalid characters, and Unicode indicators.
        /// Returns an annotation for an empty string if no userinfo is present.
        /// </returns>
        public static AddressPartAnnotation AnnotateUserInfo(Uri uri)
        {
            // Re-parse from the raw original string so percent-encoding is not decoded
            var original = uri.OriginalString;

            // Find the authority section (after "://")
            var authorityStart = original.IndexOf("://", StringComparison.Ordinal);
            var authority = authorityStart >= 0
                ? original[(authorityStart + 3)..]
                : original;

            // Strip path, query, and fragment
            var authorityEnd = authority.IndexOfAny(['/', '?', '#']);
            if (authorityEnd >= 0)
                authority = authority[..authorityEnd];

            // Find '@' to identify userinfo
            var atIndex = authority.IndexOf('@');
            if (atIndex < 0)
                return Annotate(string.Empty, InputMode.Uri);

            var userInfo = authority[..atIndex];

            // Drop the password portion (":password") — never annotate credentials
            var passwordColon = userInfo.IndexOf(':');
            if (passwordColon >= 0)
                userInfo = userInfo[..passwordColon];

            return Annotate(userInfo, InputMode.Uri);
        }

        /// <summary>
        /// Analyses and annotates the host component of a given URI.
        /// </summary>
        /// <param name="uri">The URI whose host will be annotated.</param>
        /// <returns>
        /// An <see cref="AddressPartAnnotation"/> containing the annotated host along with
        /// metadata including tokens, suspicious or invalid characters, and Unicode indicators.
        /// </returns>
        public static AddressPartAnnotation AnnotateHost(Uri uri) => Annotate(uri.Host, InputMode.Uri);

        /// <summary>
        /// Analyses and annotates a <see cref="MailAddress"/>, producing token-level detail
        /// about its local part and domain.
        /// </summary>
        /// <param name="address">The email address to annotate.</param>
        /// <returns>
        /// An <see cref="AddressPartAnnotation"/> containing the raw address, separated local
        /// and domain parts, per-character tokens, and metadata such as Unicode presence,
        /// suspicious characters, and invalid characters.
        /// </returns>
        public static AddressPartAnnotation Annotate(MailAddress address) =>
            Annotate(address.Address, InputMode.Email);

        /// <summary>
        /// Analyses and annotates an address string, producing token-level detail about
        /// its local part and domain (or userinfo and host for URIs).
        /// </summary>
        /// <param name="address">
        /// The address string to annotate. Can be an email address or a URI userinfo/host string
        /// depending on <paramref name="mode"/>.
        /// </param>
        /// <param name="mode">
        /// Specifies the processing mode. Use <see cref="InputMode.Email"/> for RFC 5321 / 6531
        /// email addresses, or <see cref="InputMode.Uri"/> for RFC 3986 URI components.
        /// Defaults to <see cref="InputMode.Email"/>.
        /// </param>
        /// <returns>
        /// An <see cref="AddressPartAnnotation"/> containing the raw address, separated local
        /// and domain parts, per-character tokens, and metadata such as Unicode presence,
        /// suspicious characters, and invalid characters.
        /// </returns>
        public static AddressPartAnnotation Annotate(string address, InputMode mode = InputMode.Email)
        {
            var atIndex = address.LastIndexOf('@');

            var local = atIndex >= 0 ? address[..atIndex] : address;
            var domain = atIndex >= 0 ? address[(atIndex + 1)..] : string.Empty;

            var localTokens = TokenizeSegment(local, mode);
            var domainTokens = TokenizeSegment(domain, mode);

            var allTokens = localTokens.Concat(domainTokens).ToList();

            return new AddressPartAnnotation(
                Raw: address,
                LocalPart: local,
                Domain: domain,
                Mode: mode,
                LocalTokens: localTokens,
                DomainTokens: domainTokens,
                ContainsUnicode: allTokens.Any(t => t.Type == CharacterType.Unicode),
                ContainsSuspiciousChars: allTokens.Any(t => t.IsSuspicious),
                ContainsInvalidChars: allTokens.Any(t => t.Type == CharacterType.Invalid)
            );
        }

        private static IReadOnlyList<CharacterToken> TokenizeSegment(string segment, InputMode mode)
        {
            var tokens = new List<CharacterToken>();
            var i = 0;

            while (i < segment.Length)
            {
                // Percent-encoded sequence (%XX) — URI mode only
                if (mode == InputMode.Uri && segment[i] == '%'
                                          && i + 2 < segment.Length
                                          && IsHexChar(segment[i + 1])
                                          && IsHexChar(segment[i + 2]))
                {
                    var encoded = segment.Substring(i, 3);
                    tokens.Add(new CharacterToken(
                        Char: encoded,
                        Codepoint: $"U+{(int)segment[i]:X4}",
                        Type: CharacterType.PercentEncoded,
                        Script: null,
                        HomoglyphOf: null,
                        IsStructural: false,
                        IsSuspicious: false
                    ));
                    i += 3;
                    continue;
                }

                // Surrogate pair (supplementary plane character, e.g. emoji)
                var codepoint = char.IsHighSurrogate(segment[i]) && i + 1 < segment.Length
                    ? char.ConvertToUtf32(segment[i], segment[i + 1])
                    : segment[i];
                var charLen = codepoint > 0xFFFF ? 2 : 1;
                var ch = segment.Substring(i, charLen);

                tokens.Add(ClassifyChar(ch, codepoint, mode));
                i += charLen;
            }

            return tokens;
        }

        private static CharacterToken ClassifyChar(string ch, int codepoint, InputMode mode)
        {
            var codepointStr = $"U+{codepoint:X4}";

            switch (codepoint)
            {
                // '@' separator — structural in both modes
                case '@':
                    return new CharacterToken(ch, codepointStr,
                        mode == InputMode.Email
                            ? CharacterType.EmailStructural
                            : CharacterType.UriStructural,
                        null, null, true, false);

                // ASCII range (U+0000–U+007F)
                case <= 0x7F:
                {
                    var c = (char)codepoint;

                    if (char.IsLetter(c))
                        return new CharacterToken(ch, codepointStr,
                            CharacterType.AsciiAlpha, "Basic Latin", null, false, false);

                    if (char.IsDigit(c))
                        return new CharacterToken(ch, codepointStr,
                            CharacterType.AsciiDigit, "Basic Latin", null, false, false);

                    // Dot is structural in both modes.
                    // '+' is structural/semantic in email only (sub-address delimiter per RFC 5321).
                    //     In URI mode, '+' is an allowed special character, handled below.
                    var isDot = c == '.';
                    var isEmailPlus = c == '+' && mode == InputMode.Email;
                    var isStructural = isDot || isEmailPlus;

                    if (isStructural)
                        return new CharacterToken(ch, codepointStr,
                            mode == InputMode.Email
                                ? CharacterType.EmailStructural
                                : CharacterType.UriStructural,
                            null, null, true, false);

                    // Check against the mode-appropriate allowed specials set.
                    // Note: '+' in URI mode reaches here and is correctly found in UriSpecialChars.
                    var allowed = mode == InputMode.Email
                        ? EmailSpecialChars.Contains(c)
                        : UriSpecialChars.Contains(c);

                    return new CharacterToken(ch, codepointStr,
                        allowed ? CharacterType.AsciiSpecial : CharacterType.Invalid,
                        null, null, false, !allowed);
                }
            }

            // Non-ASCII Unicode
            var script = GetScript(codepoint);
            var homoglyph = Homoglyphs.TryGetValue(codepoint, out char similar)
                ? similar.ToString()
                : null;

            return new CharacterToken(ch, codepointStr,
                CharacterType.Unicode, script, homoglyph, false, homoglyph != null);
        }

        private static string GetScript(int codepoint) => codepoint switch
        {
            >= 0x0400 and <= 0x04FF => "Cyrillic",
            >= 0x0370 and <= 0x03FF => "Greek",
            >= 0x0600 and <= 0x06FF => "Arabic",
            >= 0x4E00 and <= 0x9FFF => "CJK Unified Ideographs",
            >= 0x3040 and <= 0x309F => "Hiragana",
            >= 0x30A0 and <= 0x30FF => "Katakana",
            >= 0x0080 and <= 0x024F => "Latin Extended",
            >= 0x0250 and <= 0x02AF => "IPA Extensions",
            >= 0x1E00 and <= 0x1EFF => "Latin Extended Additional",
            _ => "Unknown"
        };

        private static bool IsHexChar(char c) =>
            c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

        // JSON serialisation helper

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serialises an <see cref="AddressPartAnnotation"/> to indented JSON.
        /// </summary>
        /// <param name="annotation">The annotation to serialise.</param>
        /// <returns>A JSON string representation of the annotation.</returns>
        public static string ToJson(AddressPartAnnotation annotation) =>
            JsonSerializer.Serialize(annotation, JsonOptions);
    }
}